using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.OLE.Interop;
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
    class FileNode : BaseFileNode
    {
        private const string ModuleTrackingKey = "AutoImportModules";

        public bool IsEntryPoint { get; set; }

        public FileNode(ProjectNode root, ProjectElement elm) : base(root, elm)
        {
            SetModuleTracking(true);
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new FileNodeProperties(this);
        }

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
        protected override bool CanDeleteItem(Microsoft.VisualStudio.Shell.Interop.__VSDELETEITEMOPERATION deleteOperation)
        {
            if (IsEntryPoint)
                return false;
            else
                return base.CanDeleteItem(deleteOperation);
        }

        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (IsEntryPoint 
                && ((cmdGroup == VsMenus.guidStandardCommandSet97 && ((VsCommands)cmd == VsCommands.Rename || (VsCommands)cmd == VsCommands.Cut))
                    || (cmdGroup == VsMenus.guidStandardCommandSet2K && ((VsCommands2K)cmd == VsCommands2K.EXCLUDEFROMPROJECT || (VsCommands2K)cmd == VsCommands2K.RUNCUSTOMTOOL))))
            {
                        result |= QueryStatusResult.NOTSUPPORTED;
                        return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        private static bool ParseBool(string name)
        {
            bool retValue;
            if (!Boolean.TryParse(name, out retValue))
                return false;
            return retValue;
        }

        public bool GetModuleTracking()
        {
            return ParseBool(this.ItemNode.GetEvaluatedMetadata(ModuleTrackingKey));
        }

        public void SetModuleTracking(bool value)
        {
            this.ItemNode.SetMetadata(ModuleTrackingKey, value.ToString());
        }
    }
}
