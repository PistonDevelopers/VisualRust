using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Build.Framework;
using System.IO;
using VisualRust.Shared;

namespace VisualRust.Build
{
    public class Rustc : Microsoft.Build.Utilities.Task
    {
        private static readonly Regex defectRegex = new Regex(@"^([^\n:]+):(\d+):(\d+):\s+(\d+):(\d+)\s+(.*)$", RegexOptions.Multiline | RegexOptions.CultureInvariant);

        private static readonly Regex errorCodeRegex = new Regex(@"\[([A-Z]\d\d\d\d)\]$", RegexOptions.CultureInvariant);

        private string[] configFlags = new string[0];
        /// <summary>
        /// Sets --cfg option.
        /// </summary>
        public string[] ConfigFlags
        {
            get { return configFlags; }
            set { configFlags = value; }
        }

        private string[] libPaths = new string[0];
        /// <summary>
        /// Sets -L option.
        /// </summary>
        public string[] AdditionalLibPaths
        {
            get { return libPaths; }
            set { libPaths = value; }
        }

        private string[] crateType = new string[0];
        /// <summary>
        /// Sets --crate-type option.
        /// </summary>
        public string[] CrateType 
        { 
            get { return crateType; }
            set { crateType = value; }
        }

        private string[] emit = new string[0];
        /// <summary>
        /// Sets --emit option.
        /// </summary>
        public string[] Emit
        {
            get { return emit; }
            set { emit = value; }
        }

        /// <summary>
        /// Sets --crate-name option.
        /// </summary>
        public string CrateName { get; set; }

        private bool? debugInfo;
        /// <summary>
        /// Sets -g option.
        /// </summary>
        public bool DebugInfo 
        {
            get { return debugInfo.HasValue && debugInfo.Value; }
            set { debugInfo = value; }
        }

        /// <summary>
        /// Sets -o option.
        /// </summary>
        public string OutputFile { get; set; }

        private int? optimizationLevel;
        /// <summary>
        /// Sets --opt-level option. Default value is 0.
        /// </summary>
        public int OptimizationLevel
        {
            get { return optimizationLevel.HasValue ? optimizationLevel.Value : 0; }
            set { optimizationLevel = value; }
        }

        /// <summary>
        /// Sets --out-dir option.
        /// </summary>
        public string OutputDirectory { get; set; }

        private bool? test;
        /// <summary>
        /// Sets --test option. Default value is false.
        /// </summary>
        public bool Test
        {
            get { return test.HasValue ? test.Value : false; }
            set { test = value; }
        }

        /// <summary>
        /// Sets --target option.
        /// </summary>
        public string TargetTriple { get; set; }


        private string[] lintsAsWarning = new string[0];
        /// <summary>
        /// Sets -W option.
        /// </summary>
        public string[] LintsAsWarnings
        { 
            get { return lintsAsWarning; }
            set { lintsAsWarning = value; }
        }

        private string[] lintsAsAllowed = new string[0];
        /// <summary>
        /// Sets -A option.
        /// </summary>
        public string[] LintsAsAllowed
        { 
            get { return lintsAsAllowed; }
            set { lintsAsAllowed = value; }
        }

        private string[] lintsAsDenied = new string[0];
        /// <summary>
        /// Sets -D option.
        /// </summary>
        public string[] LintsAsDenied
        { 
            get { return lintsAsDenied; }
            set { lintsAsDenied = value; }
        }

        private string[] lintsAsForbidden = new string[0];
        /// <summary>
        /// Sets -F option.
        /// </summary>
        public string[] LintsAsForbidden
        { 
            get { return lintsAsForbidden; }
            set { lintsAsForbidden = value; }
        }

        /// <summary>
        /// Sets -C option.
        /// </summary>
        public string CodegenOptions { get; set; }

        private bool? lto;
        /// <summary>
        /// Sets -C lto option. Default value is false.
        /// </summary>
        public bool LTO
        {
            get { return lto.HasValue ? lto.Value : false; }
            set { lto = value; }
        }

        [Required]
        public string WorkingDirectory { get; set; }

        [Required]
        public string Input { get; set; }

        public override bool Execute()
        {
            try
            {
                return ExecuteInner();
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }
        }

        private bool ExecuteInner()
        {
            StringBuilder sb = new StringBuilder();
            if (ConfigFlags.Length > 0)
                sb.AppendFormat(" --cfg {0}", String.Join(",", ConfigFlags));
            if (AdditionalLibPaths.Length > 0)
                sb.AppendFormat(" -L {0}", String.Join(",", AdditionalLibPaths));
            if(CrateType.Length > 0)
                sb.AppendFormat(" --crate-type {0}", String.Join(",",CrateType));
            if(Emit.Length > 0)
                sb.AppendFormat(" --emit {0}", String.Join(",", Emit));
            if(!String.IsNullOrWhiteSpace(CrateName))
                sb.AppendFormat(" --crate-name {0}", CrateName);
            if(DebugInfo)
                sb.AppendFormat(" -g");
            if(OutputFile != null)
                sb.AppendFormat(" -o {0}", OutputFile);
            if (optimizationLevel.HasValue)
                sb.AppendFormat(" -C opt-level={0}", Shared.OptimizationLevelExtension.Parse(OptimizationLevel.ToString()).ToBuildString());
            if (OutputDirectory != null)
                sb.AppendFormat(" --out-dir {0}", OutputDirectory);
            if (test.HasValue && test.Value)
                sb.Append(" --test");
            if (TargetTriple != null && !String.Equals(TargetTriple, Shared.Environment.DefaultTarget, StringComparison.OrdinalIgnoreCase))
                sb.AppendFormat(" --target {0}", TargetTriple);
            if(LintsAsWarnings.Length > 0)
                sb.AppendFormat(" -W {0}", String.Join(",", LintsAsWarnings));
            if(LintsAsAllowed.Length > 0)
                sb.AppendFormat(" -A {0}", String.Join(",", LintsAsAllowed));
            if(LintsAsDenied.Length > 0)
                sb.AppendFormat(" -D {0}", String.Join(",", LintsAsDenied));
            if(LintsAsForbidden.Length > 0)
                sb.AppendFormat(" -F {0}", String.Join(",", LintsAsForbidden));
            if (lto.HasValue && lto.Value)
                sb.AppendFormat(" -C lto");
            if (CodegenOptions != null)
                sb.AppendFormat(" -C {0}", CodegenOptions);
            sb.AppendFormat(" {0}", Input);
            string target = TargetTriple ?? Shared.Environment.DefaultTarget;
            string installPath = Shared.Environment.FindInstallPath(target);
            if(installPath == null)
            {
                if(String.Equals(target, Shared.Environment.DefaultTarget, StringComparison.OrdinalIgnoreCase))
                    Log.LogError("No Rust installation detected. You can download official Rust installer from https://www.rust-lang.org/downloads.html");
                else
                    Log.LogError("Could not find a Rust installation that can compile target {0}.", target);
                return false;
            }
            var psi = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                FileName =  Path.Combine(installPath, "rustc.exe"),
                UseShellExecute = false,
                WorkingDirectory = WorkingDirectory,
                Arguments = sb.ToString(),
                RedirectStandardError = true
            };
            Log.LogCommandLine(String.Join(" ", psi.FileName, psi.Arguments));
            try
            {
                Process process = new Process();
                process.StartInfo = psi;
                StringBuilder error = new StringBuilder();

                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    errorWaitHandle.WaitOne();
                }

                string errorOutput = error.ToString();
                // We found some warning or errors in the output, print them out
                IEnumerable<RustcParsedMessage> messages = ParseOutput(errorOutput);
                // We found some warning or errors in the output, print them out
                foreach (RustcParsedMessage msg in messages)
                {
                    LogRustcMessage(msg);
                }
                // rustc failed but we couldn't sniff anything from stderr
                // this could be an internal compiler error or a missing main() function (there are probably more errors without spans)
                if (process.ExitCode != 0 && !messages.Any())
                {
                    // FIXME: This automatically sets the file to VisualRust.Rust.targets. Is there a way to set no file instead?
                    this.Log.LogError(errorOutput);
                    return false;
                }
                return process.ExitCode == 0;
            }
            catch(Exception ex)
            {
                Log.LogErrorFromException(ex, true);
                return false;
            }
        }
        
        private IEnumerable<RustcParsedMessage> ParseOutput(string output)
        {
            MatchCollection errorMatches = defectRegex.Matches(output);

            RustcParsedMessage previous = null;
            foreach (Match match in errorMatches)
            {
                string remainingMsg = match.Groups[6].Value.Trim();
                Match errorMatch = errorCodeRegex.Match(remainingMsg);
                string errorCode = errorMatch.Success ? errorMatch.Groups[1].Value : null;
                int line = Int32.Parse(match.Groups[2].Value, System.Globalization.NumberStyles.None);
                int col = Int32.Parse(match.Groups[3].Value, System.Globalization.NumberStyles.None);
                int endLine = Int32.Parse(match.Groups[4].Value, System.Globalization.NumberStyles.None);
                int endCol = Int32.Parse(match.Groups[5].Value, System.Globalization.NumberStyles.None);

                if (remainingMsg.StartsWith("warning: "))
                {
                    string msg = match.Groups[6].Value.Substring(9, match.Groups[6].Value.Length - 9 - (errorCode != null ? 8 : 0));
                    if (previous != null) yield return previous;
                    previous = new RustcParsedMessage(RustcParsedMessageType.Warning, msg, errorCode, match.Groups[1].Value,
                        line, col, endLine, endCol);
                }
                else if (remainingMsg.StartsWith("note: ") || remainingMsg.StartsWith("help: "))
                {
                    if (remainingMsg.StartsWith("help: pass `--explain ") && previous != null)
                    {
                        previous.CanExplain = true;
                        continue;
                    }

                    // NOTE: "note: " and "help: " are both 6 characters long (though hardcoding this is probably still not a very good idea)
                    string msg = remainingMsg.Substring(6, remainingMsg.Length - 6 - (errorCode != null ? 8 : 0));
                    var type = remainingMsg.StartsWith("note: ") ? RustcParsedMessageType.Note : RustcParsedMessageType.Help;
                    RustcParsedMessage note = new RustcParsedMessage(type, msg, errorCode, match.Groups[1].Value,
                        line, col, endLine, endCol);

                    if (previous != null)
                    {
                        // try to merge notes and help messages with a previous message (warning or error where it belongs to), if the span is the same
                        if (previous.TryMergeWithFollowing(note))
                        {
                            continue; // skip setting new previous, because we successfully merged the new note into the previous message
                        }
                        else
                        {
                            yield return previous;
                        }
                    }
                    previous = note;
                }
                else
                {
                    bool startsWithError = remainingMsg.StartsWith("error: ");
                    string msg = remainingMsg.Substring((startsWithError ? 7 : 0), remainingMsg.Length - (startsWithError ? 7 : 0) - (errorCode != null ? 8 : 0));
                    if (previous != null) yield return previous;
                    previous = new RustcParsedMessage(RustcParsedMessageType.Error, msg, errorCode, match.Groups[1].Value,
                        line, col, endLine, endCol);
                }
            }

            if (previous != null) yield return previous;
        }

        private void LogRustcMessage(RustcParsedMessage msg)
        {
            if (msg.Type == RustcParsedMessageType.Warning)
            {
                this.Log.LogWarning(null, msg.ErrorCode, null, msg.File, msg.LineNumber, msg.ColumnNumber, msg.EndLineNumber, msg.EndColumnNumber, msg.Message);
            }
            else if (msg.Type == RustcParsedMessageType.Note)
            {
                this.Log.LogWarning(null, msg.ErrorCode, null, msg.File, msg.LineNumber, msg.ColumnNumber, msg.EndLineNumber, msg.EndColumnNumber, "note: " + msg.Message);
            }
            else
            {
                this.Log.LogError(null, msg.ErrorCode, null, msg.File, msg.LineNumber, msg.ColumnNumber, msg.EndLineNumber, msg.EndColumnNumber, msg.Message);
            }
        }
    }
}
