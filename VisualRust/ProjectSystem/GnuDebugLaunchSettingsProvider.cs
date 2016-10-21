using MICore.Xml.LaunchOptions;
using Microsoft.VisualStudio.ProjectSystem.Debuggers;
using Microsoft.VisualStudio.ProjectSystem.Utilities.DebuggerProviders;
using Microsoft.VisualStudio.ProjectSystem.VS.Debuggers;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using VisualRust.Options;
using VisualRust.Shared;

namespace VisualRust.ProjectSystem
{
    internal class GnuDebugLaunchSettingsProvider : IDebugLaunchSettingsProvider
    {
        protected readonly static Guid MIDebugEngineGuid = new Guid(Microsoft.MIDebugEngine.EngineConstants.EngineId);

        public async Task<DebugLaunchSettings> GetLaunchSettingsAsync(string executable, string arguments, string workingDirectory,
           DebugLaunchOptions options, Cargo cargo, TargetTriple triple)
        {
            EnvDTE.DTE env = (EnvDTE.DTE)VisualRustPackage.GetGlobalService(typeof(EnvDTE.DTE));
            var target = new DebugLaunchSettings(options)
            {
                LaunchOperation = DebugLaunchOperation.CreateProcess,
                LaunchDebugEngineGuid = MIDebugEngineGuid,
                Executable = executable,
                Arguments = arguments,
                CurrentDirectory = workingDirectory,
                Options = ToXmlString(await BuildLaunchOptionsAsync(executable, arguments, workingDirectory, env, cargo, triple))
            };
            return target;
        }

        private async Task<PipeLaunchOptions> BuildLaunchOptionsAsync(string path, string args, string workingDir,
            EnvDTE.DTE dte, Cargo cargo, TargetTriple triple)
        {
            // We could go through LocalLaunchOptions, but this way we can pass additional args
            PipeLaunchOptions options = new PipeLaunchOptions();
            options.PipePath = GetGdbPath(dte, triple);
            options.PipeArguments = String.Format("-q -interpreter=mi {0}", GetDebuggingConfigProperty<string>(dte, nameof(DebuggingOptionsPage.GdbExtraArguments)));
            options.ExePath = EscapePath(path);
            options.ExeArguments = args;
            options.SetupCommands = GetSetupCommands();
            // this affects the number of bytes the engine reads when disassembling commands, 
            // x64 has the largest maximum command size, so it should be safe to use for x86 as well
            options.TargetArchitecture = TargetArchitecture.x64;
            await SetWorkingDirectoryAsync(options, path, workingDir, cargo);
            return options;
        }

        private async Task SetWorkingDirectoryAsync(PipeLaunchOptions options, string path, string workingDir, Cargo cargo)
        {
            options.WorkingDirectory = EscapePath(workingDir);
            // GDB won't search working directory by default, but this is expected on Windows.
            string additionalPath = workingDir;

            // TODO: this is probably wrong, because the sysroot does not directly contain SOLibs

            string rustInstallPath = await cargo.GetSysrootAsync();
            if (rustInstallPath != null)
            {
                // prepend toolchain directory
                additionalPath = rustInstallPath + ";" + additionalPath;
            }
            options.AdditionalSOLibSearchPath = additionalPath;
        }

        string GetGdbPath(EnvDTE.DTE dte, TargetTriple triple)
        {
            string gdbPath = null;
            //bool useCustomPath = false;
            //bool.TryParse(GetDebuggingConfigProperty<bool>(dte, ""));
            bool useCustomPath = GetDebuggingConfigProperty<bool>(dte, nameof(DebuggingOptionsPage.UseCustomGdb));
            if (useCustomPath)
            {
                gdbPath = GetDebuggingConfigProperty<string>(dte, nameof(DebuggingOptionsPage.CustomGdbPath));
            }

            if (gdbPath != null)
                return gdbPath;

            return Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "gdb",
                    GetArchitectureName(triple),
                    "bin\\gdb");
        }

        // We need to run the right gdb depending on the architecture of the debuggee
        private string GetArchitectureName(TargetTriple triple)
        {
            if (triple != null)
            {
                if (triple.Arch == "i686" || triple.Arch == "x86_64")
                    return triple.Arch;
            }
            if (System.Environment.Is64BitOperatingSystem)
                return "x86_64";
            return "i686";
        }

        private static string ToXmlString<T>(T obj)
        {
            var builder = new StringBuilder();
            var serializer = new XmlSerializer(typeof(T));
            using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                serializer.Serialize(writer, (object)obj);
            }
            return builder.ToString();
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
                   //.Union(GetScriptLines())
                   .Select(cmd => new Command { Value = cmd })
                   .ToArray();
        }

        //string[] GetScriptLines()
        //{
        //    string debuggerScript = env.DebugConfig.DebuggerScript;
        //    if (String.IsNullOrEmpty(debuggerScript))
        //        return new string[0];
        //    return debuggerScript.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        //}

        private T GetDebuggingConfigProperty<T>(EnvDTE.DTE env, string key)
        {
            return (T)env.get_Properties("Visual Rust", "Debugging").Item(key).Value;
        }
    }
}
