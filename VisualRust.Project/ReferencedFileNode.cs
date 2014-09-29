using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseFileNode = Microsoft.VisualStudio.Project.FileNode;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace VisualRust.Project
{
    class ReferencedFileNode : BaseFileNode
    {
        private string filePath;
        public override string FilePath { get { return filePath; } }

        public ReferencedFileNode(ProjectNode root, string file)
            : base(root, null)
        {
            this.filePath = file;
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new ReferencedFileNodeProperties(this);
        }

        // Disable rename
        public override string GetEditLabel()
        {
            return null;
        }

        public override int SetEditLabel(string label)
        {
            throw new InvalidOperationException(Properties.Resources.ErrorRenameReference);
        }

        // Disable deletion
        protected override bool CanDeleteItem(Microsoft.VisualStudio.Shell.Interop.__VSDELETEITEMOPERATION deleteOperation)
        {
            return false;
        }

        protected override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (cmdGroup == Microsoft.VisualStudio.Project.VsMenus.guidStandardCommandSet2K && (VsCommands2K)cmd == VsCommands2K.INCLUDEINPROJECT)
            {
                HierarchyNode parent = this.Parent;
                parent.RemoveChild(this);
                BaseFileNode node = this.ProjectMgr.CreateFileNode(this.FilePath);
                parent.AddChild(node);
                IVsUIHierarchyWindow uiWindow = UIHierarchyUtilities.GetUIHierarchyWindow(this.ProjectMgr.Site, SolutionExplorer);
                if (uiWindow != null)
                {
                    ErrorHandler.ThrowOnFailure(uiWindow.ExpandItem(this.ProjectMgr.InteropSafeIVsUIHierarchy, this.ID, EXPANDFLAGS.EXPF_SelectItem));
                }
                return VSConstants.S_OK;
            }
            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {

            if (cmdGroup == Microsoft.VisualStudio.Project.VsMenus.guidStandardCommandSet97 )
            {
                if ((VsCommands)cmd == VsCommands.Rename || (VsCommands)cmd == VsCommands.Cut)
                {
                    result |= QueryStatusResult.NOTSUPPORTED;
                    return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
                }
            }
            else if (cmdGroup == Microsoft.VisualStudio.Project.VsMenus.guidStandardCommandSet2K)
            {
                if ((VsCommands2K)cmd == VsCommands2K.EXCLUDEFROMPROJECT || (VsCommands2K)cmd == VsCommands2K.RUNCUSTOMTOOL)
                {
                    result |= QueryStatusResult.NOTSUPPORTED;
                    return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
                }
                else if ((VsCommands2K)cmd == VsCommands2K.INCLUDEINPROJECT)
                {
                    result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                    return (int)VSConstants.S_OK;
                }
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }
    }
}
