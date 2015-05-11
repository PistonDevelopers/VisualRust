using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;
using Microsoft.VisualStudio.Shell;

namespace VisualRust.Project
{
    abstract class BaseFileNode : CommonFileNode
    {
        private const string CreateFileMessage = 
            "The item '{0}' does not exist in the project directory. It may have been moved, renamed or deleted.\n\n"
            + "Do you want to create it now?";
        private bool isDeleted = false;
        protected abstract bool CanUserMove { get; }
        public new RustProjectNode ProjectMgr { get; private set; }

        public BaseFileNode(RustProjectNode node, ProjectElement elm, string path)
            : base(node, elm)
        {
            ProjectMgr = node;
        }

        public abstract bool GetModuleTracking();

        public bool IsRustFile
        {
            get { return String.Equals(".rs", Path.GetExtension(this.Url), StringComparison.OrdinalIgnoreCase); }
        }

        protected virtual void OnFileDeleted()
        {
            TreeOperations.DeleteSubnode(ProjectMgr, this.Url);
        }

        protected virtual void OnFileCreated()
        {
            ProjectMgr.ReparseFileNode(this);
        }

        // Disable rename
        public override string GetEditLabel()
        {
            if (!CanUserMove)
                return null;
            else
                return base.GetEditLabel();
        }

        public override int SetEditLabel(string label)
        {
            if (!CanUserMove)
                throw new InvalidOperationException(Properties.Resources.ErrorRenameEntrypoint);
            else
                return base.SetEditLabel(label);
        }


        // Disable deletion
        internal override bool CanDeleteItem(Microsoft.VisualStudio.Shell.Interop.__VSDELETEITEMOPERATION deleteOperation)
        {
            if (!CanUserMove)
                return false;
            else
                return base.CanDeleteItem(deleteOperation);
        }


        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (!CanUserMove)
            {
                if (cmdGroup == Microsoft.VisualStudioTools.Project.VsMenus.guidStandardCommandSet97
                    &&(VsCommands)cmd == VsCommands.Rename 
                    || (VsCommands)cmd == VsCommands.Cut)
                {
                    result |= QueryStatusResult.NOTSUPPORTED;
                    return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
                }
                if (cmdGroup == Microsoft.VisualStudioTools.Project.VsMenus.guidStandardCommandSet2K
                    && (VsCommands2K)cmd == VsCommands2K.EXCLUDEFROMPROJECT)
                {
                    result |= QueryStatusResult.NOTSUPPORTED;
                    return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
                }
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        protected internal override bool IsFileOnDisk(bool showMessage)
        {
            bool fileExist = IsFileOnDisk(this.Url);

            if (!fileExist && showMessage && !Utilities.IsInAutomationFunction(this.ProjectMgr.Site)) {
                string message = String.Format(CreateFileMessage, this.Caption);
                string title = string.Empty;
                OLEMSGICON icon = OLEMSGICON.OLEMSGICON_CRITICAL;
                OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_YESNO;
                OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
                if(6 == VsShellUtilities.ShowMessageBox(this.ProjectMgr.Site, title, message, icon, buttons, defaultButton))
                {
                    try
                    {
                        File.Create(this.Url);
                        return true;
                    }
                    catch(IOException)
                    {
                        VsShellUtilities.ShowMessageBox(
                            this.ProjectMgr.Site,
                            title, 
                            String.Format("Could not create file '{0}'.", this.Url),
                            OLEMSGICON.OLEMSGICON_CRITICAL,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    }
                }
            }

            return fileExist;
        }
    }
}
