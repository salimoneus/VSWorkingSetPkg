using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace Company.VSWorkingSetPkg
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("daa557e0-f63f-443b-acfa-c51976b62449")]
    public class VSWorkingSetToolWindow : ToolWindowPane
    {
        public delegate void OpenItemDelegate(string item, int position);
        public event OpenItemDelegate OpenItem;
        MyControl control = new MyControl();

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public VSWorkingSetToolWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            this.Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            base.Content = control;

            control.OpenItem += new MyControl.OpenItemDelegate(control_OpenItem);
        }

        void control_OpenItem(string item, int position)
        {
            if (OpenItem != null)
            {
                OpenItem(item, position);
            }
        }

        public void AddItem(string item)
        {
            control.AddItem(item);
        }

        public WorkingSet GetWorkingSet()
        {
            return control.GetWorkingSet();
        }

        public void SetWorkingSet(WorkingSet workingSet)
        {
            control.SetWorkingSet(workingSet);
        }

        public void UpdateItemPosition(string item, int position)
        {
            control.UpdateItemPosition(item, position);
        }

        public void RemoveItem(string item)
        {
            control.RemoveItem(item);
        }
    }
}
