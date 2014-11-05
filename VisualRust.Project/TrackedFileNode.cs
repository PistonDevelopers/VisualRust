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
            SetModuleTracking(true);
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
            return ParseBool(this.ItemNode.GetEvaluatedMetadata(ModuleTrackingKey));
        }

        public void SetModuleTracking(bool value)
        {
            this.ItemNode.SetMetadata(ModuleTrackingKey, value.ToString());
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
