using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project.Launcher
{
    class ReleaseLauncher : IRustProjectLauncher
    {
        readonly LauncherEnvironment env;

        public ReleaseLauncher(LauncherEnvironment env)
        {
            this.env = env;
        }

        public void Launch(string path, string args, string workingDir)
        {
            ProcessStartInfo processStartInfo = CreateProcessStartInfo(path, args, workingDir);
            Process.Start(processStartInfo);
        }

        ProcessStartInfo CreateProcessStartInfo(string path, string args, string workingDir)
        {
            string startupFile = Path.Combine(System.Environment.SystemDirectory, "cmd.exe");
            string cmdArgs = String.Format(@"/c """"{0}"" {1} & pause""", path, args);
            var startInfo = new ProcessStartInfo(startupFile, cmdArgs);
            startInfo.WorkingDirectory = workingDir;
            startInfo.UseShellExecute = false;
            InjectRustBinPath(startInfo);
            return startInfo;
        }

        void InjectRustBinPath(ProcessStartInfo startInfo)
        {
            string installPath = env.GetRustInstallPath();
            if(installPath == null)
                return;
            string envPath = Environment.GetEnvironmentVariable("PATH");
            string newEnvPath = String.Format("{0};{1}", envPath, installPath);
            startInfo.EnvironmentVariables["PATH"] = newEnvPath;
        }
    }
}
