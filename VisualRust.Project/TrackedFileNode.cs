using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.OLE.Interop;
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
    class TrackedFileNode : BaseFileNode
    {
        private const string ModuleTrackingKey = "AutoImportModules";

        public bool IsEntryPoint { get; set; }

        public TrackedFileNode(RustProjectNode root, ProjectElement elm)
            : base(root, elm, elm.GetFullPathForElement())
        {
        }

        public override object GetIconHandle(bool open)
        {
            if (IsRustFile || GetModuleTracking())
            {
                return ProjectMgr.RustImageHandler.GetIconHandle((int)IconIndex.RustFile);
            }
            return base.GetIconHandle(open);
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new FileNodeProperties(this);
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
            string value = this.ItemNode.GetEvaluatedMetadata(ModuleTrackingKey);
            if (String.IsNullOrWhiteSpace(value))
                return true;
            bool retValue;
            if(!Boolean.TryParse(value, out retValue))
                return true;
            return retValue;
        }

        public void SetModuleTracking(bool value)
        {
            this.ItemNode.SetMetadata(ModuleTrackingKey, value.ToString());
            if (value)
                this.ProjectMgr.EnableAutoImport(this);
            else
                this.ProjectMgr.DisableAutoImport(this);
            this.ReDraw(UIHierarchyElement.Icon);
        }

        protected override int ExcludeFromProject()
        {
            ((RustProjectNode)this.ProjectMgr).ExcludeFileNode(this);
            return VSConstants.S_OK;
        }

        protected override bool CanUserMove
        {
            get { return !IsEntryPoint; }
        }
    }
}
