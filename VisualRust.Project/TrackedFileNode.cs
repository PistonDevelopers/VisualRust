using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;
using System.Diagnostics.Contracts;

namespace VisualRust.Project
{
    class TrackedFileNode : BaseFileNode
    {
        private const string ModuleTrackingKey = "AutoImportModules";

        public bool IsEntryPoint { get; set; }

        public TrackedFileNode(RustProjectNode root, ProjectElement elm)
            : base(root, elm, elm.GetMetadata(ProjectFileConstants.Include))
        {
        }

        public override int ImageIndex
        {
            get
            {
                int baseIdx = base.ImageIndex;
                if (baseIdx == (int)ProjectNode.ImageName.MissingFile || baseIdx == (int)ProjectNode.ImageName.ExcludedFile)
                    return baseIdx;
                else if (IsRustFile || GetModuleTracking())
                    return (int)IconIndex.NoIcon;
                else
                    return baseIdx;
            }
        }

        public override object GetIconHandle(bool open)
        {
            if (IsRustFile || GetModuleTracking())
                return ProjectMgr.RustImageHandler.GetIconHandle((int)IconIndex.RustFile);
            return base.GetIconHandle(open);
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            if (this.ItemNode.IsExcluded)
                return new ExcludedFileNodeProperties(this);
            else
                return new FileNodeProperties(this);
        }

        private static bool ParseBool(string name)
        {
            bool retValue;
            if (!Boolean.TryParse(name, out retValue))
                return false;
            return retValue;
        }

        public override bool GetModuleTracking()
        {
            if (ItemNode.IsExcluded)
                return false;
            string value = this.ItemNode.GetMetadata(ModuleTrackingKey);
            if (String.IsNullOrWhiteSpace(value))
                return true;
            bool retValue;
            if(!Boolean.TryParse(value, out retValue))
                return true;
            return retValue;
        }

        public void SetModuleTracking(bool value)
        {
            if (ItemNode.IsExcluded)
                throw new InvalidOperationException();
            this.ItemNode.SetMetadata(ModuleTrackingKey, value.ToString());
            if (value)
                this.ProjectMgr.EnableAutoImport(this);
            else
                this.ProjectMgr.DisableAutoImport(this);
            this.ProjectMgr.ReDrawNode(this, UIHierarchyElement.Icon);
        }

        internal override int ExcludeFromProject()
        {
            ((RustProjectNode)this.ProjectMgr).ExcludeFileNode(this);
            return VSConstants.S_OK;
        }

        protected override bool CanUserMove
        {
            get { return !IsEntryPoint; }
        }

        internal override int IncludeInProject(bool includeChildren)
        {
            Contract.Assert(this.ItemNode.IsExcluded);
            int result = base.IncludeInProject(includeChildren);
            ProjectMgr.OnNodeIncluded(this);
            return result;
        }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
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
