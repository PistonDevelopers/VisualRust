using Microsoft.VisualStudio.Project;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace VisualRust.Project
{
    abstract class BaseFileNode : FileNode
    {
        protected abstract bool CanUserMove { get; }
        public string AbsoluteFilePath { get; private set; }
        public new RustProjectNode ProjectMgr { get; private set; }

        public BaseFileNode(RustProjectNode node, ProjectElement elm, string path)
            : base(node, elm)
        {
            Contract.Assert(System.IO.Path.IsPathRooted(path));
            AbsoluteFilePath = path;
            ProjectMgr = node;
        }

        public override string FilePath { get { return this.AbsoluteFilePath; } }

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
        protected override bool CanDeleteItem(Microsoft.VisualStudio.Shell.Interop.__VSDELETEITEMOPERATION deleteOperation)
        {
            if (!CanUserMove)
                return false;
            else
                return base.CanDeleteItem(deleteOperation);
        }


        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (!CanUserMove)
            {
                if (cmdGroup == Microsoft.VisualStudio.Project.VsMenus.guidStandardCommandSet97
                    &&(VsCommands)cmd == VsCommands.Rename 
                    || (VsCommands)cmd == VsCommands.Cut)
                {
                    result |= QueryStatusResult.NOTSUPPORTED;
                    return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
                }
                if (cmdGroup == Microsoft.VisualStudio.Project.VsMenus.guidStandardCommandSet2K
                    && (VsCommands2K)cmd == VsCommands2K.EXCLUDEFROMPROJECT)
                {
                    result |= QueryStatusResult.NOTSUPPORTED;
                    return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
                }
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }
    }
}
