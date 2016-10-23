using Microsoft.Build.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisualRust.Shared;
using VisualRust.Shared.Message;

namespace VisualRust.Build
{
    /// <summary>
    /// Task that wraps the 'cargo build' command.
    /// </summary>
    public class CargoBuild : CargoTask
    {
        private bool release = false;
        /// <summary>
        /// Sets the --release flag.
        /// </summary>
        public bool Release {
            get { return release; }
            set { release = value; }
        }

        protected override bool ExecuteCargo(Cargo cargo)
        {
            var args = GetArguments().ToArray();
            var argString = CommandHelper.EscapeArguments(args);
            Log.LogCommandLine(String.Join(" ", cargo.Executable, argString));

            var psi = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                FileName = cargo.Executable,
                UseShellExecute = false,
                WorkingDirectory = cargo.WorkingDirectory,
                Arguments = argString,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            var process = new Process();
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;

            string cargoErrorPrefix = "error: ";

            using (AutoResetEvent exitedWaitHandle = new AutoResetEvent(false))
            {
                // errors from cargo itself (wrong command line parameters, etc) are still printed to stderr
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        if (e.Data.TrimStart().StartsWith(cargoErrorPrefix))
                        {
                            string error = e.Data.Substring(cargoErrorPrefix.Length);
                            if (!error.StartsWith("Could not compile"))
                            {
                                // mark Cargo errors as coming from Cargo.toml ... that might not be true in all cases, but it's
                                // better than having them reported as coming from VisualRust.Rust.targets
                                LogCargoError(error);
                            }
                        }
                    }
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        var reader = new JsonTextReader(new StringReader(e.Data));
                        var message = Cargo.JsonSerializer.Deserialize<CargoMessage>(reader);
                        Rustc.LogRustcMessage(message.message, Manifest.Directory.FullName, Log);
                    }
                };

                process.Exited += (sender, e) => exitedWaitHandle.Set();

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                exitedWaitHandle.WaitOne();

                return process.ExitCode == 0;
            }
        }

        IEnumerable<string> GetArguments()
        {
            var args = new List<string> { "build", "--message-format", "json" };
            if (Release)
            {
                args.Add("--release");
            }
            return args;
        }
    }
}
