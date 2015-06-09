using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project
{
    sealed class DefaultRustLauncher : IProjectLauncher
    {
        private readonly RustProjectNode _project;
        private RustProjectConfig _projectConfig;
        private readonly Configuration.Debug _debugConfig;

        public DefaultRustLauncher(RustProjectNode project)
        {
            Utilities.ArgumentNotNull("project", project);
            _project = project;
            string currConfig = _project.GetProjectProperty(ProjectFileConstants.Configuration);
            _projectConfig = (RustProjectConfig)_project.ConfigProvider.GetProjectConfiguration(currConfig);
            _debugConfig = Configuration.Debug.LoadFrom(new ProjectConfig[] { _projectConfig });
        }

        public int LaunchProject(bool debug)
        {
            if (_debugConfig.StartAction == Configuration.StartAction.Project &&
                _project.GetProjectProperty("OutputType") != "exe")
            {
                throw new InvalidOperationException("A project with an Output Type of Library cannot be started directly.");
            }

            string startupFilePath;
            if (_debugConfig.StartAction == Configuration.StartAction.Project)
                startupFilePath = GetProjectStartupFile();
            else
                startupFilePath = _debugConfig.ExternalProgram;

            return LaunchFile(startupFilePath, debug);
        }

        private string GetProjectStartupFile()
        {
            var startupFilePath = Path.Combine(_project.GetProjectProperty("TargetDir"), _project.GetProjectProperty("TargetFileName"));
            if (string.IsNullOrEmpty(startupFilePath))
            {
                throw new ApplicationException("Visual Rust could not resolve path for your executable. Your installation of Visual Rust or .rsproj file might be corrupted.");
            }

            if (!File.Exists(startupFilePath))
                _project.Build("Build");

            return startupFilePath;
        }

        public int LaunchFile(string file, bool debug)
        {
            if (debug)
            {
                if (_projectConfig.DebugType == Constants.BuiltinDebugger)
                    LaunchInBuiltinDebugger(file);
                else
                    LaunchInGdbDebugger(file);
            }
            else
            {
                var processStartInfo = CreateProcessStartInfo(file, debug);
                Process.Start(processStartInfo);
            }

            return VSConstants.S_OK;

        }

        private void LaunchInBuiltinDebugger(string file)
        {
            VsDebugTargetInfo4[] targets = new VsDebugTargetInfo4[1];
            targets[0].dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            targets[0].guidLaunchDebugEngine = Constants.NativeOnlyEngine;
            targets[0].bstrExe = file;
            if (!string.IsNullOrEmpty(_debugConfig.CommandLineArgs))
                targets[0].bstrArg = _debugConfig.CommandLineArgs;
            if (!string.IsNullOrEmpty(_debugConfig.WorkingDir))
                targets[0].bstrCurDir = _debugConfig.WorkingDir;

            VsDebugTargetProcessInfo[] results = new VsDebugTargetProcessInfo[targets.Length];

            IVsDebugger4 vsDebugger = (IVsDebugger4)_project.GetService(typeof(SVsShellDebugger));
            vsDebugger.LaunchDebugTargets4((uint)targets.Length, targets, results);
        }

        private void LaunchInGdbDebugger(string file)
        {
            VsDebugTargetInfo4[] targets = new VsDebugTargetInfo4[1];
            targets[0].dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            targets[0].bstrExe = file;
            targets[0].guidLaunchDebugEngine = Constants.GdbEngine;

            string gdbPath = GetDebuggingProperty<string>("DebuggerLocation");
            if (string.IsNullOrWhiteSpace(gdbPath))
            {
                gdbPath = "gdb.exe";
            }

            string gdbArgs =
                "-q " +	// quiet
                "-interpreter=mi " + // use machine interface
                GetDebuggingProperty<string>("ExtraArgs"); // add extra options from Visual Rust/Debugging options page

            var options = new StringBuilder();
            using (var writer = XmlWriter.Create(options, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                writer.WriteStartElement("PipeLaunchOptions", "http://schemas.microsoft.com/vstudio/MDDDebuggerOptions/2014");
                writer.WriteAttributeString("PipePath", gdbPath);
                writer.WriteAttributeString("PipeArguments", gdbArgs);
                writer.WriteAttributeString("ExePath", EscapePath(file));
                if (!string.IsNullOrEmpty(_debugConfig.CommandLineArgs))
                    writer.WriteAttributeString("ExeArguments", _debugConfig.CommandLineArgs);
                if (!string.IsNullOrEmpty(_debugConfig.WorkingDir))
                    writer.WriteAttributeString("WorkingDirectory", EscapePath(_debugConfig.WorkingDir));
                else
                    writer.WriteAttributeString("WorkingDirectory", EscapePath(Path.GetDirectoryName(file)));
                // this affects the number of bytes the engine reads when disassembling commands, 
                // x64 has the largest maximum command size, so it should be safe to use for x86 as well
                writer.WriteAttributeString("TargetArchitecture", "x64");

                // isn't used for now
                //writer.WriteAttributeString("AdditionalSOLibSearchPath", ...); // set solib-search-path ...

                // GDB engine expects to find a shell on the other end of the pipe, so the first thing it sends over is "gdb --interpreter=mi",
                // (which GDB complains about, since this isn't a valid command).  
                // Since we are launching GDB directly, here we create a noop alias for "gdb" to make the error message go away.
                writer.WriteElementString("Command", "alias -a gdb=echo");
                // launch debuggee in a new console window
                writer.WriteElementString("Command", "set new-console on");
                if (!string.IsNullOrEmpty(_debugConfig.DebuggerScript))
                {
                    foreach (string cmd in _debugConfig.DebuggerScript.Split('\r', '\n'))
                        if (!string.IsNullOrEmpty(cmd))
                            writer.WriteElementString("Command", cmd);
                }

                writer.WriteEndElement();
            }
            targets[0].bstrOptions = options.ToString();

            VsDebugTargetProcessInfo[] results = new VsDebugTargetProcessInfo[targets.Length];

            IVsDebugger4 vsDebugger = (IVsDebugger4)_project.GetService(typeof(SVsShellDebugger));
            vsDebugger.LaunchDebugTargets4((uint)targets.Length, targets, results);

            // Type "gdb <command>" in the VS Command Window
            var commandWnd = (IVsCommandWindow)_project.GetService(typeof(SVsCommandWindow));
            commandWnd.ExecuteCommand("alias gdb Debug.GDBExec");
        }

        private string EscapePath(string path)
        {
            return String.Format("\"{0}\"", path);
        }

        private T GetDebuggingProperty<T>(string key)
        {
            var env = (EnvDTE.DTE)_project.GetService(typeof(EnvDTE.DTE));
            return (T)env.get_Properties("Visual Rust", "Debugging").Item(key).Value;
        }

        private ProcessStartInfo CreateProcessStartInfo(string startupFile, bool debug)
        {
            var commandLineArgs = _debugConfig.CommandLineArgs;
            if(!debug)
            {
                commandLineArgs = String.Format(@"/c """"{0}"" {1} & pause""", startupFile, commandLineArgs);
                startupFile = Path.Combine(System.Environment.SystemDirectory, "cmd.exe");
            }
            var startInfo = new ProcessStartInfo(startupFile, commandLineArgs);
            startInfo.WorkingDirectory = _debugConfig.WorkingDir;
            startInfo.UseShellExecute = false;
            InjectRustBinPath(startInfo);
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