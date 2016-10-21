using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VisualRust.Shared
{
    public struct CommandResult
    {
        public string Output { get; }
        public int ExitCode { get; }

        public CommandResult(string output, int exitCode)
        {
            Output = output;
            ExitCode = exitCode;
        }
    }

    public static class CommandHelper
    {
        private static Regex escapeInvalidChar = new Regex("[\x00\x0a\x0d]", RegexOptions.Compiled);//  these can not be escaped
        private static Regex escapeNeedsQuotes = new Regex(@"\s|""", RegexOptions.Compiled);//          contains whitespace or two quote characters
        private static Regex escapeEscapeQuote = new Regex(@"(\\*)(""|$)", RegexOptions.Compiled);//    one or more '\' followed with a quote or end of string

        /// <summary>
        /// Quotes all arguments that contain whitespace, or begin with a quote and returns a single
        /// argument string for use with Process.Start().
        /// </summary>
        /// <param name="args">A list of strings for arguments, may not contain null, '\0', '\r', or '\n'</param>
        /// <returns>The combined list of escaped/quoted strings</returns>
        /// <exception cref="System.ArgumentNullException">Raised when one of the arguments is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Raised if an argument contains '\0', '\r', or '\n'</exception>
        public static string EscapeArguments(string[] args)
        {
            // SOURCE OF THIS CODE: http://csharptest.net/529/how-to-correctly-escape-command-line-arguments-in-c/
            StringBuilder arguments = new StringBuilder();
            
            for (int carg = 0; args != null && carg < args.Length; carg++)
            {
                if (args[carg] == null) { throw new ArgumentNullException("args[" + carg + "]"); }
                if (escapeInvalidChar.IsMatch(args[carg])) { throw new ArgumentOutOfRangeException("args[" + carg + "]"); }
                if (args[carg] == String.Empty) { arguments.Append("\"\""); }
                else if (!escapeNeedsQuotes.IsMatch(args[carg])) { arguments.Append(args[carg]); }
                else
                {
                    arguments.Append('"');
                    arguments.Append(escapeEscapeQuote.Replace(args[carg], m =>
                    m.Groups[1].Value + m.Groups[1].Value +
                    (m.Groups[2].Value == "\"" ? "\\\"" : "")
                    ));
                    arguments.Append('"');
                }
                if (carg + 1 < args.Length)
                    arguments.Append(' ');
            }
            return arguments.ToString();
        }

        public static Task<CommandResult> RunAsync(string command, params string[] arguments)
        {
            return RunAsync(command, arguments, null, default(CancellationToken));
        }

        public static Task<CommandResult> RunAsync(string command, string[] arguments, string workingDirectory = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = command,
                RedirectStandardOutput = true,
                Arguments = CommandHelper.EscapeArguments(arguments)
            };

            if (!string.IsNullOrEmpty(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }

            var tcs = new TaskCompletionSource<CommandResult>();
            var outputBuffer = new StringBuilder();
            var process = new Process();
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (sender, args) => {
                if (args.Data == null)
                {
                    Debug.Assert(process.HasExited);
                    tcs.TrySetResult(new CommandResult(outputBuffer.ToString(), process.ExitCode));
                }
                else
                {
                    outputBuffer.AppendLine(args.Data);
                }
            };
            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(() =>
                {
                    tcs.SetCanceled();
                    process.Kill();
                });
            }

            process.Start();
            process.BeginOutputReadLine();
            return tcs.Task;
        }
    }
}
