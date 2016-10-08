//------------------------------------------------------------------------------
// <copyright file="TestCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;

namespace VSIXTests
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class TestCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("bbf15d13-4d79-4433-9d92-eee1bfe7ab47");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private TestCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                commandService.AddCommand(new MenuCommand(this.MenuItemCallback, new CommandID(CommandSet, Commands.Publish)));
                commandService.AddCommand(new MenuCommand(this.MenuItemCallback, new CommandID(CommandSet, Commands.PublishTo)));
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static TestCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new TestCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var command = (MenuCommand)(sender);

            switch (command.CommandID.ID)
            {
                case (Commands.Publish):
                    {
                        Publish();
                        return;
                    }
                case (Commands.PublishTo):
                    {
                        PublishTo();
                        return;
                    }
            }
        }

        private void Publish()
        {
            DTE dte = (DTE)this.ServiceProvider.GetService(typeof(DTE));

            Projects projects = dte.Solution.Projects;


            IntPtr hierarchyPtr, selectionContainerPtr;
            Object prjItemObject = null;
            IVsMultiItemSelect mis;
            uint prjItemId;
            IVsMonitorSelection monitorSelection = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
            monitorSelection.GetCurrentSelection(out hierarchyPtr, out prjItemId, out mis, out selectionContainerPtr);
            IVsHierarchy selectedHierarchy = Marshal.GetTypedObjectForIUnknown(hierarchyPtr, typeof(IVsHierarchy)) as IVsHierarchy;
            if (selectedHierarchy != null)
            {
                ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(prjItemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out prjItemObject));
            }
            Project selectedPrjItem = prjItemObject as Project;

            foreach (var p in selectedPrjItem.ProjectItems)
            {
                ProjectItem item = (ProjectItem)(p);

                string n = item.Name;
                int c = item.FileCount;

                if (n == "Properties")
                {
                    foreach (var f in item.ProjectItems)
                    {
                        ProjectItem item2 = (ProjectItem)(f);

                        string nn = item2.Name;
                        
                    }
                }
            }


        }

        private void PublishTo()
        {
            // SEE: https://blog.mastykarz.nl/active-project-extending-visual-studio-sharepoint-development-tools-tip-1/

            ;
        }
    }
}
