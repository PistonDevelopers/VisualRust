using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace VisualRust.Project
{
    class RustFolderNode : CommonFolderNode
    {
        private RustProjectNode rustProjectNode;
        private ProjectElement element;

        public RustFolderNode(RustProjectNode rustProjectNode, ProjectElement element)
            : base(rustProjectNode, element)
        { }
        public bool IsEntryPoint { get; set; }

        // Disable rename
        public override string GetEditLabel()
        {
            if (IsEntryPoint)
                return null;
            else
                return base.GetEditLabel();
        }

        public override int SetEditLabel(string label)
        {
            if (IsEntryPoint)
                throw new InvalidOperationException(Properties.Resources.ErrorRenameEntrypoint);
            else
                return base.SetEditLabel(label);
        }

        // Disable deletion
        internal override bool CanDeleteItem(Microsoft.VisualStudio.Shell.Interop.__VSDELETEITEMOPERATION deleteOperation)
        {
            if (IsEntryPoint)
                return false;
            else
                return base.CanDeleteItem(deleteOperation);
        }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (IsEntryPoint)
            {
                if (cmdGroup == Microsoft.VisualStudioTools.Project.VsMenus.guidStandardCommandSet97
                    && (VsCommands)cmd == VsCommands.Rename
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
            if (cmdGroup == Microsoft.VisualStudioTools.Project.VsMenus.guidStandardCommandSet2K
                && (VsCommands2K)cmd == VsCommands2K.INCLUDEINPROJECT
                && ItemNode.ItemTypeName == null)
            {
                result |= QueryStatusResult.NOTSUPPORTED;
                return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }
    }
}
