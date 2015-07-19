using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Execution;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using VisualRust.Project.Configuration.MsBuild;

namespace VisualRust.Project
{
    class RustProjectConfig : CommonProjectConfig, IVsProjectCfgDebugTypeSelection
    {
        private static bool initialized = false;
        private static bool isGdbSupported = false;
        private static bool isGdbInstalled = false;
        public Configuration.MsBuildConfiguration UserCfg { get; private set; }

        public string DebugType { get; internal set; }

        public RustProjectConfig(RustProjectNode project, string configuration)
            : base(project, configuration)
        {
            this.UserCfg = new Configuration.MsBuildConfiguration(project.UserConfig, configuration, "default");
            if (!initialized)
            {
                // Determine IDE version and whether GDB engine is installed (only in VS2015+)

                var env = (EnvDTE.DTE)project.GetService(typeof(SDTE));
                Version ver;
                if (Version.TryParse(env.Version, out ver))
                {
                    isGdbSupported = ver.Major >= 14;
                }

                var debugger = (IVsDebugger2)project.GetService(typeof(SVsShellDebugger));
                string name;
                Guid gdbEngine = Constants.GdbEngine;
                if (debugger.GetEngineName(ref gdbEngine, out name) == 0)
                {
                    isGdbInstalled = true;
                    isGdbSupported = true;
                }

                initialized = true;
            }

            DebugType = isGdbInstalled ? Constants.GdbDebugger : Constants.BuiltinDebugger;
        }

        public void GetDebugTypes(out Array pbstrDebugTypes)
        {
            if (isGdbSupported)
                pbstrDebugTypes = new string[] { Constants.BuiltinDebugger, Constants.GdbDebugger };
            else
                pbstrDebugTypes = new string[] { Constants.BuiltinDebugger };
        }

        public void GetDebugTypeName(string bstrDebugType, out string pbstrDebugTypeName)
        {
            pbstrDebugTypeName = bstrDebugType;
        }

        public void GetCurrentDebugType(out string pbstrDebugType)
        {
            pbstrDebugType = DebugType;
        }

        public void SetCurrentDebugType(string bstrDebugType)
        {
            if (bstrDebugType == Constants.GdbDebugger && !isGdbInstalled)
            {
                throw new ApplicationException("GDB debugging engine is not available.\n" +
                    "Please make sure that the 'Core Tools for Visual C++ Mobile Development' component has been installed.");
            }

            DebugType = bstrDebugType;
        }
    }
}
