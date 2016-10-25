using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VisualRust.Shared
{
    // TODO: add rustup support, including automatic detection of installed targets
    [DebuggerDisplay("Cargo [{path,nq}]")]
    public class Cargo
    {
        public static readonly JsonSerializer JsonSerializer = new JsonSerializer { MissingMemberHandling = MissingMemberHandling.Ignore };
        public string Executable { get; }
        public string RustcExecutable { get; }

        public string WorkingDirectory { get; set; }

        private Cargo(string exe)
        {
            Executable = exe;
            RustcExecutable = Path.Combine(Path.GetDirectoryName(exe), "rustc.exe");
            WorkingDirectory = null;
        }

        /// <summary>
        /// Returns a wrapper for an installed cargo.exe that understands --message-format,
        /// or null if none was found.
        /// </summary>
        public static Cargo FindSupportedInstallation()
        {
            var installPath = Environment.GetAllInstallPaths()
                .Select(p => Path.Combine(Path.Combine(p, "bin"), "cargo.exe"))
                .FirstOrDefault(p => {
                    if (!File.Exists(p)) return false;
                    var version = GetCargoVersion(p);
                    return version.HasValue && version.Value.Date >= new DateTime(2016, 10, 06);
                });

            if (installPath == null)
            {
                return null;
            }

            return new Cargo(installPath);
        }

        public Task<CommandResult> RunAsync(params string[] arguments)
        {
            return RunAsync(arguments, default(CancellationToken));
        }

        public Task<CommandResult> RunAsync(string[] arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            return CommandHelper.RunAsync(Executable, arguments, WorkingDirectory, cancellationToken);
        }

        public async Task<string> GetSysrootAsync()
        {
            var result = await CommandHelper.RunAsync(RustcExecutable, "--print=sysroot");
            return result.Output.Trim();
        }

        public async Task<TargetTriple> GetHostTargetTripleAsync()
        {
            var result = await CommandHelper.RunAsync(RustcExecutable, "-vV");
            Match hostMatch = Regex.Match(result.Output, "^host:\\s*(.+)$", RegexOptions.Multiline);
            if (hostMatch.Groups.Count < 2)
                return null;
            return TargetTriple.Create(hostMatch.Groups[1].Value.TrimEnd());
        }

        public async Task<Message.CargoMetadata> ReadMetadataAsync(bool includeDependencies)
        {
            string[] args;
            if (includeDependencies)
            {
                args = new string[] { "metadata" };
            }
            else
            {
                args = new string[] { "metadata", "--no-deps" };
            }
            

            var result = await RunAsync(args);
            return JsonSerializer.Deserialize<Message.CargoMetadata>(new JsonTextReader(new StringReader(result.Output)));
        }

        protected static ToolVersion? GetCargoVersion(string exePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = exePath,
                RedirectStandardOutput = true,
                Arguments = "-V",
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            try
            {
                Process proc = Process.Start(psi);
                string versionOutput = proc.StandardOutput.ReadToEnd();
                return ToolVersion.Parse(versionOutput);
            }
            catch (Win32Exception)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }
    }
}
