using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;
using System.Text.RegularExpressions;
using VisualRust.Shared;

namespace VisualRust.Project.Launcher
{
    class LauncherEnvironment
    {
        readonly RustProjectNode project;
        readonly Configuration.Debug debugConfig;
        readonly RustProjectConfig projectConfig;

        public LauncherEnvironment(RustProjectNode project, Configuration.Debug debugConfig, RustProjectConfig  projConfig)
        {
            this.project = project;
            this.debugConfig = debugConfig;
            this.projectConfig = projConfig;
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

        public T GetDebugConfigurationProperty<T>(string key)
        {
            var env = (EnvDTE.DTE)project.GetService(typeof(EnvDTE.DTE));
            return (T)env.Properties["Visual Rust", "Debugging"].Item(key).Value;
        }

        public string GetProjectProperty(string key)
        {
            return project.GetProjectProperty(key);
        }

        public TargetTriple GetTargetTriple()
        {
            TargetTriple confTriple = TryGetTripleFromConfiguration();
            if(confTriple != null)
                return confTriple;
            return TryGetTripleFromRustc();
        }

        public void ForceBuild()
        {
            project.Build("Build");
        }

        TargetTriple TryGetTripleFromConfiguration()
        {
            string triple = Configuration.Build.LoadFrom(new ProjectConfig[] { projectConfig }).PlatformTarget;
            return TargetTriple.Create(triple);
        }

        TargetTriple TryGetTripleFromRustc()
        {
            string defaultInstallPath = Shared.Environment.FindInstallPath("default");
            if(defaultInstallPath == null)
                return null;
            string rustcPath = Path.Combine(defaultInstallPath, "rustc.exe");
            string rustcHost = GetRustcHost(rustcPath);
            return TargetTriple.Create(rustcHost);
        }

        static string GetRustcHost(string exePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = exePath,
                RedirectStandardOutput = true,
                Arguments = "-Vv"
            };
            Process proc = Process.Start(psi);
            string verboseVersion = proc.StandardOutput.ReadToEnd();
            Match hostMatch = Regex.Match(verboseVersion, "^host:\\s*(.+)$", RegexOptions.Multiline);
            if (hostMatch.Groups.Count == 1)
                return null;
            return hostMatch.Groups[1].Value;
        }
    }
}
