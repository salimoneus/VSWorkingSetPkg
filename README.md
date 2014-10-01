VSWorkingSetPkg
===============

This Visual Studio extension package includes C# code to:

    Create a dockable custom tool window in Visual Studio
    Access the tool window via the Tools menu
    Add a context menu item to any file in the Solution Explorer window
    Invoke an operation in the extension for the selected file (only supports .cpp currently)
    Handle various Visual Studio events (opening/closing a solution, opening/closing documents, etc)
    Open a file in the editor and autoscroll to the last active line (kinda sorta working)
    Saves serialized XML user data to Roaming profile folder 
