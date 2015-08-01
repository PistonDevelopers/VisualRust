using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using VisualRust.Project.Configuration;

namespace VisualRust.Project
{
    class RustProjectConfig : CommonProjectConfig, IVsProjectCfgDebugTypeSelection
    {
        private MsBuildConfiguration userCfg;
        private string debugType;

        public MsBuildConfiguration UserCfg
        {
            get { return userCfg ?? (userCfg = LoadUserConfiguration()); }
        }

        public RustProjectConfig(RustProjectNode project, string configuration)
            : base(project, configuration)
        { }

        private MsBuildConfiguration LoadUserConfiguration()
        {
            return new MsBuildConfiguration(((RustProjectNode)this.ProjectMgr).UserConfig, this.ConfigName, "default");
        }

        public void GetDebugTypes(out Array pbstrDebugTypes)
        {
            pbstrDebugTypes = new[] { Constants.GdbDebugger };
        }

        public void GetDebugTypeName(string bstrDebugType, out string pbstrDebugTypeName)
        {
            pbstrDebugTypeName = bstrDebugType;
        }

        public void GetCurrentDebugType(out string pbstrDebugType)
        {
            pbstrDebugType = debugType;
        }

        public void SetCurrentDebugType(string bstrDebugType)
        {
            debugType = bstrDebugType;
        }
    }
}
