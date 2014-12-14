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
    class UntrackedFileNode : BaseFileNode
    {
        public UntrackedFileNode(RustProjectNode root, string file)
            : base(root, null, file)
        { }

        protected override bool CanUserMove
        {
            get { return false; }
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new ReferencedFileNodeProperties(this);
        }

        public override object GetIconHandle(bool open)
        {
            if (IsRustFile)
                return this.ProjectMgr.RustImageHandler.GetIconHandle((int)IconIndex.UntrackedRustFile);
            return base.GetIconHandle(open);
        }

        protected override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (cmdGroup == Microsoft.VisualStudio.Project.VsMenus.guidStandardCommandSet2K && (VsCommands2K)cmd == VsCommands2K.INCLUDEINPROJECT)
            {
                ((RustProjectNode)this.ProjectMgr).IncludeFileNode(this);
                return VSConstants.S_OK;
            }
            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (cmdGroup == Microsoft.VisualStudio.Project.VsMenus.guidStandardCommandSet2K
                && (VsCommands2K)cmd == VsCommands2K.INCLUDEINPROJECT)
            {
                result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                return (int)VSConstants.S_OK;
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }
    }
}
