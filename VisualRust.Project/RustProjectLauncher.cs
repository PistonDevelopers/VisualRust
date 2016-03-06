using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.MIDebugEngine;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using VisualRust.Project.Launcher;
using VisualRust.Shared;

namespace VisualRust.Project
{
    sealed class RustProjectLauncher : IProjectLauncher
    {
        private readonly LauncherEnvironment environment;
        private readonly Configuration.Debug debugConfig;

        public RustProjectLauncher(RustProjectNode project)
        {
            Utilities.ArgumentNotNull("project", project);
            string currConfig = project.GetProjectProperty(ProjectFileConstants.Configuration);
            RustProjectConfig projectConfig = (RustProjectConfig)project.ConfigProvider.GetProjectConfiguration(currConfig);
            debugConfig = Configuration.Debug.LoadFrom(new[] { projectConfig.UserCfg });
            if (debugConfig.StartAction == Configuration.StartAction.Project &&
                project.GetProjectProperty("OutputType") != "exe")
            {
                throw new InvalidOperationException("A project with an Output Type of Library cannot be started directly.");
            }
            this.environment = new LauncherEnvironment(project, debugConfig, projectConfig);
        }

        public int LaunchProject(bool debug)
        {
            string startupFilePath;
            if (debugConfig.StartAction == Configuration.StartAction.Project)
                startupFilePath = GetStartupExecutable();
            else
                startupFilePath = debugConfig.ExternalProgram;
            return LaunchFile(startupFilePath, debug);
        }

        public int LaunchFile(string file, bool debug)
        {
            if (debugConfig.StartAction == Configuration.StartAction.Project 
                && !File.Exists(file))
            {
                environment.ForceBuild();
            }
            IRustProjectLauncher launcher = ChooseLauncher(debug);
            launcher.Launch(
                file,
                debugConfig.CommandLineArgs,
                debugConfig.WorkingDir);
            return VSConstants.S_OK;
        }

        string GetStartupExecutable()
        {
            return Path.Combine(environment.GetProjectProperty("TargetDir"), environment.GetProjectProperty("TargetFileName"));
        }

        private IRustProjectLauncher ChooseLauncher(bool debug)
        {
            if(!debug)
                return new ReleaseLauncher(environment);
            TargetTriple triple = environment.GetTargetTriple();
            if(triple != null && triple.Abi == "msvc")
                return new MsvcDebugLauncher(environment);
            else
                return new GnuDebugLauncher(environment, triple);
        }
    }
}