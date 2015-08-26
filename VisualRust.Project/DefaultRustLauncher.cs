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

namespace VisualRust.Project
{
    sealed class DefaultRustLauncher : IProjectLauncher
    {
        private enum BuildArchitecture
        {
            Unknown,
            i686,
            x86_64,
        }

        private readonly RustProjectNode project;
        private readonly Configuration.Debug debugConfig;
        private readonly RustProjectConfig projectConfig;

        public DefaultRustLauncher(RustProjectNode project)
        {
            Utilities.ArgumentNotNull("project", project);
            this.project = project;
            string currConfig = this.project.GetProjectProperty(ProjectFileConstants.Configuration);
            projectConfig = (RustProjectConfig)this.project.ConfigProvider.GetProjectConfiguration(currConfig);
            debugConfig = Configuration.Debug.LoadFrom(new[] { projectConfig.UserCfg });
        }

        public int LaunchProject(bool debug)
        {
            if (debugConfig.StartAction == Configuration.StartAction.Project &&
                project.GetProjectProperty("OutputType") != "exe")
            {
                throw new InvalidOperationException("A project with an Output Type of Library cannot be started directly.");
            }

            string startupFilePath;
            if (debugConfig.StartAction == Configuration.StartAction.Project)
                startupFilePath = GetProjectStartupFile();
            else
                startupFilePath = debugConfig.ExternalProgram;

            return LaunchFile(startupFilePath, debug);
        }

        private string GetProjectStartupFile()
        {
            var startupFilePath = Path.Combine(project.GetProjectProperty("TargetDir"), project.GetProjectProperty("TargetFileName"));
            if (string.IsNullOrEmpty(startupFilePath))
            {
                throw new ApplicationException("Visual Rust could not resolve path for your executable. Your installation of Visual Rust or .rsproj file might be corrupted.");
            }

            if (!File.Exists(startupFilePath))
                project.Build("Build");

            return startupFilePath;
        }

        public int LaunchFile(string file, bool debug)
        {
            if (debug)
            {
                LaunchInGdbDebugger(file);
            }
            else
            {
                var processStartInfo = CreateProcessStartInfo(file);
                Process.Start(processStartInfo);
            }

            return VSConstants.S_OK;

        }

        private void LaunchInGdbDebugger(string file)
        {
            var targets = new VsDebugTargetInfo4[1];
            targets[0].dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            targets[0].bstrExe = file;
            targets[0].guidLaunchDebugEngine = new Guid(EngineConstants.EngineId);
            
            bool useCustomPath = GetDebugProperty<bool>("UseCustomGdbPath");
            string gdbPath;
            if (useCustomPath)
            {
                gdbPath = GetDebugProperty<string>("DebuggerLocation");
            }
            else
            {
                gdbPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "gdb",
                    GuessArchitecture(),
                    "bin\\gdb");
            }
            string gdbArgs =
                "-q " +	// quiet
                "-interpreter=mi " + // use machine interface
                GetDebugProperty<string>("ExtraArgs"); // add extra options from Visual Rust/Debugging options page

            var options = new StringBuilder();
            using (var writer = XmlWriter.Create(options, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                writer.WriteStartElement("PipeLaunchOptions", "http://schemas.microsoft.com/vstudio/MDDDebuggerOptions/2014");
                writer.WriteAttributeString("PipePath", gdbPath);
                writer.WriteAttributeString("PipeArguments", gdbArgs);
                writer.WriteAttributeString("ExePath", EscapePath(file));
                if (!string.IsNullOrEmpty(debugConfig.CommandLineArgs))
                {
                    writer.WriteAttributeString("ExeArguments", debugConfig.CommandLineArgs);
                }
                if (!string.IsNullOrEmpty(debugConfig.WorkingDir))
                {
                    writer.WriteAttributeString("WorkingDirectory", EscapePath(debugConfig.WorkingDir));
                    // GDB won't search working directory by default, but this is expected on Windows.
                    string rustBinPath = RustBinPath();
                    string additionalPath;
                    if (rustBinPath != null)
                        additionalPath = rustBinPath + ";" + debugConfig.WorkingDir;
                    else
                        additionalPath = debugConfig.WorkingDir;
                    writer.WriteAttributeString("AdditionalSOLibSearchPath", additionalPath);
                }
                else
                {
                    writer.WriteAttributeString("WorkingDirectory", EscapePath(Path.GetDirectoryName(file)));
                    string rustBinPath = RustBinPath();
                    if(rustBinPath != null)
                        writer.WriteAttributeString("AdditionalSOLibSearchPath", rustBinPath);
                }
                // this affects the number of bytes the engine reads when disassembling commands, 
                // x64 has the largest maximum command size, so it should be safe to use for x86 as well
                writer.WriteAttributeString("TargetArchitecture", "x64");
                
                writer.WriteStartElement("SetupCommands");
                // launch debuggee in a new console window
                writer.WriteElementString("Command", "-gdb-set new-console on");
                if (!string.IsNullOrEmpty(debugConfig.DebuggerScript))
                {
                    foreach (string cmd in debugConfig.DebuggerScript.Split('\r', '\n'))
                        if (!string.IsNullOrEmpty(cmd))
                            writer.WriteElementString("Command", cmd);
                }
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
            targets[0].bstrOptions = options.ToString();

            VsDebugTargetProcessInfo[] results = new VsDebugTargetProcessInfo[targets.Length];

            IVsDebugger4 vsDebugger = (IVsDebugger4)project.GetService(typeof(SVsShellDebugger));
            vsDebugger.LaunchDebugTargets4((uint)targets.Length, targets, results);            
            var commandWnd = (IVsCommandWindow)project.GetService(typeof(SVsCommandWindow));
            commandWnd.ExecuteCommand("alias gdb Debug.VRDebugExec");
        }

        private string GuessArchitecture()
        {
            BuildArchitecture configArch = GetArchFromConfiguration();
            if (configArch != BuildArchitecture.Unknown)
                return ArchitectureToString(configArch);
            BuildArchitecture rustcArch = GetArchFromRustc();
            if (rustcArch != BuildArchitecture.Unknown)
                return ArchitectureToString(rustcArch);
            if (Environment.Is64BitOperatingSystem)
                return "x86_64";
            return "i686";
        }

        private BuildArchitecture GetArchFromConfiguration()
        {
            string configuredTarget = Configuration.Build.LoadFrom(new ProjectConfig[] {this.projectConfig}).PlatformTarget;
            return ArchitectureFromTargetTriple(configuredTarget);
        }

        private static BuildArchitecture ArchitectureFromTargetTriple(string configuredTarget)
        {
            int platformNameLength = configuredTarget.IndexOf('-');
            if (platformNameLength == -1)
                return BuildArchitecture.Unknown;
            return ParseArchitecture(configuredTarget.Substring(0, platformNameLength));
        }

        private BuildArchitecture GetArchFromRustc()
        {
            string defaultInstallPath = Shared.Environment.FindInstallPath("default");
            if(defaultInstallPath == null)
                return BuildArchitecture.Unknown;
            string rustcPath = Path.Combine(defaultInstallPath, "rustc.exe");
            string rustcHost = GetRustcHost(rustcPath);
            return ArchitectureFromTargetTriple(rustcHost);
        }

        private static string ArchitectureToString(BuildArchitecture b)
        {
            switch (b)
            {
                case  BuildArchitecture.i686:
                    return "i686";
                case BuildArchitecture.x86_64:
                    return "x86_64";
                default:
                    throw new ArgumentException(null, "b");
            }
        }

        private static BuildArchitecture ParseArchitecture(string name)
        {
            switch (name)
            {
                case "i686":
                    return BuildArchitecture.i686;
                case "x86_64":
                    return BuildArchitecture.x86_64;
                default:
                    return BuildArchitecture.Unknown;
            }
        }

        private static string EscapePath(string path)
        {
            return String.Format("\"{0}\"", path);
        }

        private T GetDebugProperty<T>(string key)
        {
            var env = (EnvDTE.DTE)project.GetService(typeof(EnvDTE.DTE));
            return (T)env.Properties["Visual Rust", "Debugging"].Item(key).Value;
        }

        private ProcessStartInfo CreateProcessStartInfo(string startupFile)
        {
            var commandLineArgs = debugConfig.CommandLineArgs;
            commandLineArgs = String.Format(@"/c """"{0}"" {1} & pause""", startupFile, commandLineArgs);
            startupFile = Path.Combine(System.Environment.SystemDirectory, "cmd.exe");
            var startInfo = new ProcessStartInfo(startupFile, commandLineArgs);
            startInfo.WorkingDirectory = debugConfig.WorkingDir;
            startInfo.UseShellExecute = false;
            InjectRustBinPath(startInfo);
            return startInfo;
        }

        private string RustBinPath()
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

        private void InjectRustBinPath(ProcessStartInfo startInfo)
        {
            string installPath = RustBinPath();
            if(installPath == null)
                return;
            string envPath = Environment.GetEnvironmentVariable("PATH");
            string newEnvPath = String.Format("{0};{1}", envPath, installPath);
            startInfo.EnvironmentVariables["PATH"] = newEnvPath;
        }

        public static string GetRustcHost(string exePath)
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
            Match hostMatch = Regex.Match(verboseVersion, "^host: (.+)$", RegexOptions.Multiline);
            if (hostMatch.Groups.Count == 1)
                return null;
            return hostMatch.Groups[1].Value;
        }
    }
}