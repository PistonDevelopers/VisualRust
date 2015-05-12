using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project
{
    sealed class DefaultRustLauncher : IProjectLauncher
    {
        private readonly RustProjectNode _project;

        public DefaultRustLauncher(RustProjectNode project)
        {
            Utilities.ArgumentNotNull("project", project);
            _project = project;
        }

        public int LaunchProject(bool debug)
        {
            if(_project.GetProjectProperty("OutputType") != "exe")
                throw new InvalidOperationException("A project with an Output Type of Library cannot be started directly.");
            var startupFilePath = GetProjectStartupFile();
            return LaunchFile(startupFilePath, debug);
        }

        private string GetProjectStartupFile()
        {
            var startupFilePath = Path.Combine(_project.GetProjectProperty("TargetDir"), _project.GetProjectProperty("TargetFileName"));
            
            if (string.IsNullOrEmpty(startupFilePath))
            {
                throw new ApplicationException("Visual Rust could not resolve path for your executable. Your installation of Visual Rust or .rsproj file might be corrupted.");
            }

            return startupFilePath;
        }

        public int LaunchFile(string file, bool debug)
        {
            StartWithoutDebugger(file);
            return VSConstants.S_OK;
        }

        private void StartWithoutDebugger(string startupFile)
        {
            if(!File.Exists(startupFile))
                _project.Build("Build");
            var processStartInfo = CreateProcessStartInfoNoDebug(startupFile);
            Process.Start(processStartInfo);
        }

        private ProcessStartInfo CreateProcessStartInfoNoDebug(string startupFile)
        {
            var commandLineArgs = string.Empty;
            var startInfo = new ProcessStartInfo(startupFile, commandLineArgs);
            InjectRustBinPath(startInfo);
            startInfo.UseShellExecute = false;
            return startInfo;
        }

        private void InjectRustBinPath(ProcessStartInfo startInfo)
        {
            EnvDTE.Project proj = _project.GetAutomationObject() as EnvDTE.Project;
            if(proj == null)
                return;
            string currentConfigName = Utilities.GetActiveConfigurationName(proj);
            if(currentConfigName == null)
                return;
            ProjectConfig  currentConfig = _project.ConfigProvider.GetProjectConfiguration(currentConfigName);
            if(currentConfig == null)
                return;
            string currentTarget = currentConfig.GetConfigurationProperty("PlatformTarget", true);
            if(currentTarget == null)
                currentTarget =  Shared.Environment.DefaultTarget;
            string installPath = Shared.Environment.FindInstallPath(currentTarget);
            if(installPath == null)
                return;
            string envPath = Environment.GetEnvironmentVariable("PATH");
            string newEnvPath = String.Format("{0};{1}", envPath, installPath);
            startInfo.EnvironmentVariables["PATH"] = newEnvPath;
        }
    }
}