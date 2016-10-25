using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisualRust.Shared;

namespace VisualRust.Build
{
    /// <summary>
    /// Task that wraps the 'cargo clean' command.
    /// </summary>
    public class CargoClean : CargoTask
    {
        protected override bool ExecuteCargo(Cargo cargo)
        {
            var arguments = "clean";
            Log.LogCommandLine(String.Join(" ", cargo.Executable, arguments));

            var psi = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                FileName = cargo.Executable,
                UseShellExecute = false,
                WorkingDirectory = cargo.WorkingDirectory,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            var process = new Process();
            process.StartInfo = psi;
            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0;
        }
    }
}
