using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.IO;
using System.Xml.Serialization;
using System.Threading;

namespace Company.VSWorkingSetPkg
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(VSWorkingSetToolWindow))]
    [Guid(GuidList.guidVSWorkingSetPkgPkgString)]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    public sealed class VSWorkingSetPkgPackage : Package
    {
        private EnvDTE.DocumentEvents m_documentEvents;
        private EnvDTE.DTEEvents m_dteEvents;
        private EnvDTE.SolutionEvents m_solutionEvents;

        private VSWorkingSetToolWindow m_toolWindow;
        private static string EmptySolution = "EmptySolution";
        private string m_activeSolution = EmptySolution;
        private string m_openedItem;
        private int m_position = 0;
        //MonitorQueue monitorQueue = new MonitorQueue();

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public VSWorkingSetPkgPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        private VSWorkingSetToolWindow GetToolWindow()
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            VSWorkingSetToolWindow window = (VSWorkingSetToolWindow)this.FindToolWindow(typeof(VSWorkingSetToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            return window;
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            IVsWindowFrame windowFrame = (IVsWindowFrame)m_toolWindow.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private void ShowToolWindow()
        {
            ShowToolWindow(null, null);
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidVSWorkingSetPkgCmdSet, (int)PkgCmdIDList.cmdVSWorkingSetCommand);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidVSWorkingSetPkgCmdSet, (int)PkgCmdIDList.cmdVSWorkingSetToolWindow);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand(menuToolWin);
                // Create the command for the query status menu item.
                CommandID queryStatusCommandID = new CommandID(GuidList.guidDynamicVSWorkingSetMenu, (int)PkgCmdIDList.cmdidQueryStatus);
                OleMenuCommand queryStatusMenuCommand = new OleMenuCommand(MenuItemCallback, queryStatusCommandID);
                queryStatusMenuCommand.BeforeQueryStatus += this.queryStatusMenuCommand_BeforeQueryStatus;
                mcs.AddCommand(queryStatusMenuCommand);
            }

            IServiceContainer serviceContainer = this as IServiceContainer;
            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(SDTE));
            m_documentEvents = dte.Events.DocumentEvents;
            m_dteEvents = dte.Events.DTEEvents;
            m_solutionEvents = dte.Events.SolutionEvents;
            m_dteEvents.OnBeginShutdown += new EnvDTE._dispDTEEvents_OnBeginShutdownEventHandler(DTEEvents_OnBeginShutdown);
            m_documentEvents.DocumentOpened += new EnvDTE._dispDocumentEvents_DocumentOpenedEventHandler(DocumentEvents_DocumentOpened);
            m_documentEvents.DocumentClosing += new EnvDTE._dispDocumentEvents_DocumentClosingEventHandler(DocumentEvents_DocumentClosing);
            m_solutionEvents.Opened += new EnvDTE._dispSolutionEvents_OpenedEventHandler(SolutionEvents_Opened);
            m_solutionEvents.BeforeClosing += new EnvDTE._dispSolutionEvents_BeforeClosingEventHandler(SolutionEvents_BeforeClosing);
            m_toolWindow = GetToolWindow();
            m_toolWindow.OpenItem += new VSWorkingSetToolWindow.OpenItemDelegate(m_toolWindow_OpenItem);

            ReadConfigData(m_activeSolution);
        }

        void SolutionEvents_BeforeClosing()
        {
            WriteConfigData(m_activeSolution);
            m_activeSolution = EmptySolution;
            ReadConfigData(m_activeSolution);
        }

        void SolutionEvents_Opened()
        {
            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(SDTE));
            m_activeSolution = (dte.Solution.FullName == String.Empty ? EmptySolution : dte.Solution.FullName);
            ReadConfigData(m_activeSolution);
        }

        void DocumentEvents_DocumentClosing(EnvDTE.Document Document)
        {
            EnvDTE.TextSelection objSel = Document.Selection as EnvDTE.TextSelection;
            if (objSel != null)
            {
                m_toolWindow.UpdateItemPosition(Document.FullName, objSel.ActivePoint.Line);
            }
        }

        void DTEEvents_OnBeginShutdown()
        {
            if (m_activeSolution == EmptySolution)
            {
                WriteConfigData(m_activeSolution);
            }
        }

        private string GetConfigPath(string solution)
        {
            string path = "VSWorkingSet\\userSettings." + solution.Replace("\\", ".").Replace(":", "") + ".xml";
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), path);
        }

        private void ReadConfigData(string solution)
        {
            //Thread writeThread = new Thread(DoReadConfigData);
            //writeThread.Start(solution);
            DoReadConfigData(solution);
        }

        private void DoReadConfigData(object solution)
        {
            string fileName = GetConfigPath(solution.ToString());

            try
            {
                //monitorQueue.Enter();

                FileStream ReadFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                XmlSerializer SerializerObj = new XmlSerializer(typeof(WorkingSet));
                WorkingSet loadedData = (WorkingSet)SerializerObj.Deserialize(ReadFileStream);

                m_toolWindow.SetWorkingSet(loadedData);

                ReadFileStream.Close();
            }
            catch (Exception)
            {
                m_toolWindow.SetWorkingSet(null);
            }
            finally
            {
                //monitorQueue.Exit();
            }
        }

        private void WriteConfigData(string solution)
        {
            //Thread writeThread = new Thread(DoWriteConfigData);
            //writeThread.Start(solution);
            DoWriteConfigData(solution);
        }

        private void DoWriteConfigData(object solution)
        {
            string fileName = GetConfigPath(solution.ToString());

            try
            {
                //monitorQueue.Enter();

                WorkingSet set = m_toolWindow.GetWorkingSet();
                if (set != null)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileName));
                    TextWriter WriteFileStream = new StreamWriter(fileName, false);
                    XmlSerializer SerializerObj = new XmlSerializer(typeof(WorkingSet));

                    SerializerObj.Serialize(WriteFileStream, set);

                    WriteFileStream.Close();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
            finally
            {
                //monitorQueue.Exit();
            }
        }

        void m_toolWindow_OpenItem(string item, int position)
        {
            EnvDTE.DTE dte = (EnvDTE.DTE)GetService(typeof(SDTE));
            m_openedItem = item;
            m_position = position;

            bool success = true;

            if (!System.IO.File.Exists(item))
            {
                success = false;
            }
            else
            {
                try
                {
                    dte.ItemOperations.OpenFile(item);
                }
                catch (Exception)
                {
                    success = false;
                }
            }

            if (success == false)
            {
                if (MessageBox.Show("Unable to open file: " + item + "\n\nRemove from list?", "VSWorkingSet", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    m_toolWindow.RemoveItem(item);
                }
            }
        }

        private void DocumentEvents_DocumentOpened(EnvDTE.Document Document)
        {
            if (!System.IO.File.Exists(Document.FullName))
            {
                return;
            }

            AddToolWindowItem(Document.FullName);

            if (Document.FullName == m_openedItem)
            {
                EnvDTE.TextSelection objSel = Document.Selection as EnvDTE.TextSelection;
                if (objSel != null)
                {
                    objSel.GotoLine(m_position);
                }
            }
        }

        private string GetExecutedMenuItem()
        {
            IntPtr hierarchyPtr, selectionContainerPtr;
            uint projectItemId;
            IVsMultiItemSelect mis;

            try
            {
                IVsMonitorSelection monitorSelection = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
                monitorSelection.GetCurrentSelection(out hierarchyPtr, out projectItemId, out mis, out selectionContainerPtr);

                IVsHierarchy hierarchy = Marshal.GetTypedObjectForIUnknown(hierarchyPtr, typeof(IVsHierarchy)) as IVsHierarchy;

                if (hierarchy != null)
                {
                    string pbstrName;
                    hierarchy.GetCanonicalName(projectItemId, out pbstrName);
                    return pbstrName;
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        private void queryStatusMenuCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;

            if (menuCommand == null)
            {
                return;
            }

            string menuItem = GetExecutedMenuItem();
            if (menuItem != null)
            {
                if (menuItem.EndsWith(".cpp"))
                {
                    menuCommand.Visible = true;
                }
                else
                {
                    menuCommand.Visible = false;
                }
            }
        }

        private void AddToolWindowItem(string item)
        {
            ShowToolWindow();
            m_toolWindow.AddItem(item);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            string menuItem = GetExecutedMenuItem();
            if (menuItem != null)
            {
                AddToolWindowItem(menuItem);
            }
        }
    }
}
