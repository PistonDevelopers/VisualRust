using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualRust.Project.Launcher
{
    class LauncherEnvironment
    {
        readonly RustProjectNode project;
        readonly Configuration.Debug debugConfig;

        public LauncherEnvironment(RustProjectNode project, Configuration.Debug debugConfig)
        {
            this.project = project;
            this.debugConfig = debugConfig;
        }

        public string GetRustInstallPath()
        {
            EnvDTE.Project proj = project.GetAutomationObject() as EnvDTE.Project;
            if (proj == null)
                return null;
            string currentConfigName = Utilities.GetActiveConfigurationName(proj);
            if (currentConfigName == null)
                return null;
            ProjectConfig currentConfig = project.ConfigProvider.GetProjectConfiguration(currentConfigName);
            if (currentConfig == null)
                return null;
            string currentTarget = currentConfig.GetConfigurationProperty("PlatformTarget", true);
            if (currentTarget == null)
                currentTarget = Shared.Environment.DefaultTarget;
            return Shared.Environment.FindInstallPath(currentTarget);
        }

        public TInterface GetService<TService, TInterface>()
        {
            return (TInterface)project.GetService(typeof(TService));
        }

        public void LaunchVsDebugger(VsDebugTargetInfo4 target)
        {
            var targets = new VsDebugTargetInfo4[1] { target };
            VsDebugTargetProcessInfo[] results = new VsDebugTargetProcessInfo[targets.Length];
            IVsDebugger4 vsDebugger = (IVsDebugger4)project.GetService(typeof(SVsShellDebugger));
            vsDebugger.LaunchDebugTargets4((uint)targets.Length, targets, results);
        }

        public string GetStartupExecutable()
        {
            return Path.Combine(project.GetProjectProperty("TargetDir"), project.GetProjectProperty("TargetFileName"));
        }

        public string GdbCustomPath()
        {
            bool useCustomPath = GetDebugProperty<bool>("UseCustomGdbPath");
            if(!useCustomPath)
                return null;
            return GetDebugProperty<string>("DebuggerLocation");
        }

        public string GdbExtraArguments()
        {
            return GetDebugProperty<string>("ExtraArgs");
        }

        public string[] GdbScriptLines()
        {
            if(String.IsNullOrEmpty(debugConfig.DebuggerScript))
                return new string[0];
            return debugConfig.DebuggerScript.Split(new [] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        T GetDebugProperty<T>(string key)
        {
            var env = (EnvDTE.DTE)project.GetService(typeof(EnvDTE.DTE));
            return (T)env.Properties["Visual Rust", "Debugging"].Item(key).Value;
        }

        public bool IsMsvcToolchain()
        {
            return false;
        }

        public string GetArchitectureName()
        {
            if (Environment.Is64BitOperatingSystem)
                return "x86_64";
            return "i686";
        }
    }
}
