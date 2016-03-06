using MICore.Xml.LaunchOptions;
using Microsoft.MIDebugEngine;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using VisualRust.Shared;

namespace VisualRust.Project.Launcher
{
    class GnuDebugLauncher : IRustProjectLauncher
    {
        readonly LauncherEnvironment env;
        readonly TargetTriple triple;

        public GnuDebugLauncher(LauncherEnvironment env, TargetTriple triple)
        {
            this.env = env;
            this.triple = triple;
        }

        public void Launch(string path, string args, string workingDir)
        {
            PipeLaunchOptions options = BuildLaunchOptions(path, args, workingDir);
            VsDebugTargetInfo4 target = new VsDebugTargetInfo4
            {
                dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess,
                bstrExe = path,
                guidLaunchDebugEngine = new Guid(EngineConstants.EngineId),
                bstrOptions = ToXmlString(options)
            };
            env.LaunchVsDebugger(target);
            IVsCommandWindow commandWnd = env.GetService<SVsCommandWindow, IVsCommandWindow>();
            commandWnd.ExecuteCommand("Tools.Alias gdb Debug.VRDebugExec");
        }

        private PipeLaunchOptions BuildLaunchOptions(string path, string args, string workingDir)
        {
            // We could go through LocalLaunchOptions, but this way we can pass additional args
            PipeLaunchOptions options = new PipeLaunchOptions();
            options.PipePath = GetGdbPath();
            options.PipeArguments = String.Format("-q -interpreter=mi {0}", GetExtraArguments());
            options.ExePath = EscapePath(path);
            options.ExeArguments = args;
            options.SetupCommands = GetSetupCommands();
            // this affects the number of bytes the engine reads when disassembling commands, 
            // x64 has the largest maximum command size, so it should be safe to use for x86 as well
            options.TargetArchitecture = TargetArchitecture.x64;
            SetWorkingDirectory(options, path, workingDir);
            return options;
        }

        private void SetWorkingDirectory(PipeLaunchOptions options, string path, string workingDir)
        {
            string rustInstallPath = env.GetRustInstallPath();
            if (String.IsNullOrWhiteSpace(workingDir))
            {
                options.WorkingDirectory = EscapePath(Path.GetDirectoryName(path));
                if (rustInstallPath != null)
                    options.AdditionalSOLibSearchPath = rustInstallPath;
            }
            else
            {
                options.WorkingDirectory = EscapePath(workingDir);
                // GDB won't search working directory by default, but this is expected on Windows.
                string additionalPath;
                if (rustInstallPath != null)
                    additionalPath = rustInstallPath + ";" + workingDir;
                else
                    additionalPath = workingDir;
                options.AdditionalSOLibSearchPath = additionalPath;
            }
        }

        private static string EscapePath(string path)
        {
            if (String.IsNullOrEmpty(path))
                return "";
            return String.Format("\"{0}\"", path);
        }

        Command[] GetSetupCommands()
        {
            return new string[] { "-gdb-set new-console on" }
                   .Union(GetScriptLines())
                   .Select(cmd => new Command { Value = cmd })
                   .ToArray();
        }

        string GetGdbPath()
        {
            string gdbPath = GetCustomPath();
            if (gdbPath != null)
                return gdbPath;
            return Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "gdb",
                    GetArchitectureName(),
                    "bin\\gdb");
        }

        private string GetArchitectureName()
        {
            if(triple != null)
            {
                if(triple.Arch == "i686" || triple.Arch == "x86_64")
                    return triple.Arch;
            }
            if (System.Environment.Is64BitOperatingSystem)
                return "x86_64";
            return "i686";
        }

        static string ToXmlString<T>(T obj)
        {
            var builder = new StringBuilder();
            var serializer = new XmlSerializer(typeof(T));
            using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                serializer.Serialize(writer, (object)obj);
            }
            return builder.ToString();
        }

        string GetCustomPath()
        {
            bool useCustomPath = env.GetDebugConfigurationProperty<bool>("UseCustomGdbPath");
            if(!useCustomPath)
                return null;
            return env.GetDebugConfigurationProperty<string>("DebuggerLocation");
        }

        string GetExtraArguments()
        {
            return env.GetDebugConfigurationProperty<string>("ExtraArgs");
        }

        string[] GetScriptLines()
        {
            string debuggerScript = env.DebugConfig.DebuggerScript;
            if(String.IsNullOrEmpty(debuggerScript))
                return new string[0];
            return debuggerScript.Split(new [] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}