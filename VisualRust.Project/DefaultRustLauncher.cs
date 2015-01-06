using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project
{
    /// <summary>
    /// Simple realization of project launcher for executable file
    /// TODO need realize for library
    /// </summary>
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
            var startupFilePath = GetProjectStartupFile();
            return LaunchFile(startupFilePath, debug);
        }

        private string GetProjectStartupFile()
        {
            var startupFilePath = Path.Combine(_project.GetProjectProperty("TargetDir"), _project.GetProjectProperty("TargetFileName"));

            //var startupFilePath = _project.GetStartupFile();
            
            if (string.IsNullOrEmpty(startupFilePath))
            {
                throw new ApplicationException("Startup file is not defined in project");
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
            var processStartInfo = CreateProcessStartInfoNoDebug(startupFile);
            Process.Start(processStartInfo);
        }

        private ProcessStartInfo CreateProcessStartInfoNoDebug(string startupFile)
        {
            // TODO add command line arguments
            var commandLineArgs = string.Empty;
            var startInfo = new ProcessStartInfo(startupFile, commandLineArgs);

            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = _project.GetWorkingDirectory();

            return startInfo;
        }
    }
}