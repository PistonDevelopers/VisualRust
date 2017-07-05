using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Build.Framework;
using System.IO;
using VisualRust.Shared.Message;
using VisualRust.Shared;
using Microsoft.Build.Utilities;

namespace VisualRust.Build
{
    // TODO: It is unclear whether we should continue to support/provide this task
    public class Rustc : Microsoft.Build.Utilities.Task
    {
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

        private String installPath;
        private ToolVersion rustcVersion;

        public override bool Execute()
        {
            try
            {
                if (!FindRustc())
                    return false;

                var version = GetVersion();
                if (!version.HasValue)
                    return false;
                rustcVersion = version.Value;

                return ExecuteInner();
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex, true);
                return false;
            }
        }

        private bool FindRustc()
        {
            string target = TargetTriple ?? Shared.Environment.DefaultTarget;
            installPath = Shared.Environment.FindInstallPath(target);
            if (installPath == null)
            {
                if (String.Equals(target, Shared.Environment.DefaultTarget, StringComparison.OrdinalIgnoreCase))
                    Log.LogError("No Rust installation detected. You can download official Rust installer from https://www.rust-lang.org/downloads.html");
                else
                    Log.LogError("Could not find a Rust installation that can compile target {0}.", target);
                return false;
            }

            return true;
        }

        private Process CreateProcess(String argumets)
        {
            var psi = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                FileName = Path.Combine(installPath, "rustc.exe"),
                UseShellExecute = false,
                WorkingDirectory = WorkingDirectory,
                Arguments = argumets,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            var process = new Process();
            process.StartInfo = psi;

            return process;
        }

        private ToolVersion? GetVersion()
        {
            var process = CreateProcess(" --version");
            process.Start();
            process.WaitForExit();
            return ToolVersion.Parse(process.StandardOutput.ReadToEnd());
        }

        private bool ExecuteInner()
        {
            var useJsonErrorFormat = rustcVersion.Major >= 1 && rustcVersion.Minor >= 12;

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
            if (useJsonErrorFormat)
                sb.Append(" --error-format=json");
            sb.AppendFormat(" {0}", Input);

            var process = CreateProcess(sb.ToString());
            Log.LogCommandLine(String.Join(" ", process.StartInfo.FileName, process.StartInfo.Arguments));
            try
            {

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
                IEnumerable<RustcMessageHuman> messagesHuman = null;
                IEnumerable<RustcMessageJson> messageJson = null;
                bool haveAnyMessages = false;

                if (useJsonErrorFormat)
                {
                    messageJson = RustcMessageJsonParser.Parse(errorOutput);
                    foreach (var msg in messageJson)
                    {
                        LogRustcMessage(msg, WorkingDirectory, Log);
                        haveAnyMessages = true;
                    }
                }
                else
                {
                    messagesHuman = RustcMessageHumanParser.Parse(errorOutput);
                    foreach (var msg in messagesHuman)
                    {
                        LogRustcMessage(msg);
                        haveAnyMessages = true;
                    }
                }

                // rustc failed but we couldn't sniff anything from stderr
                // this could be an internal compiler error or a missing main() function (there are probably more errors without spans)
                if (process.ExitCode != 0 && !haveAnyMessages)
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
        
        
        private void LogRustcMessage(RustcMessageHuman msg)
        {
            if (msg.Type == RustcMessageType.Warning)
            {
                this.Log.LogWarning(null, msg.ErrorCode, null, msg.File, msg.LineNumber, msg.ColumnNumber, msg.EndLineNumber, msg.EndColumnNumber, msg.Message);
            }
            else if (msg.Type == RustcMessageType.Note)
            {
                this.Log.LogWarning(null, msg.ErrorCode, null, msg.File, msg.LineNumber, msg.ColumnNumber, msg.EndLineNumber, msg.EndColumnNumber, "note: " + msg.Message);
            }
            else
            {
                this.Log.LogError(null, msg.ErrorCode, null, msg.File, msg.LineNumber, msg.ColumnNumber, msg.EndLineNumber, msg.EndColumnNumber, msg.Message);
            }
        }

        public static void LogRustcMessage(RustcMessageJson msg, string rootPath, TaskLoggingHelper log)
        {
            if (msg == null) return;
            // todo multi span
            // todo all other fields
            // todo mb help key word is code.explanation

            var type = msg.GetLevelAsEnum();
            var primarySpan = msg.GetPrimarySpan();
            var code = msg.GetErrorCodeAsString();

            // suppress message "aborting due to previous error"
            if (String.IsNullOrEmpty(code) && primarySpan == null && msg.message.Contains("aborting due to"))
                return;

            // primarySpan.file_name might not be legal (e.g. "file_name":"<println macros>" is common)
            string logFile = null;
            var logSpan = primarySpan;
            for (;;)
            {
                try
                {
                    logFile = Path.Combine(rootPath, logSpan.file_name); // maybe checking for ".rs" extension is saner than trying this and seeing if it throws?
                    break;
                }
                catch (ArgumentException) // "Illegal characters in path."
                {
                    logSpan = logSpan.expansion?.span; // see if expanding helps us find a real file
                }
            }

            if (type == RustcMessageType.Error)
            {
                if (logSpan == null)
                    log.LogError(msg.message);
                else
                    log.LogError(null, code, null, logFile, logSpan.line_start, logSpan.column_start, logSpan.line_end, logSpan.column_end, msg.message);
            }
            else
            {
                if (logSpan == null)
                    log.LogWarning(msg.message);
                else
                    log.LogWarning(null, code, null, logFile, logSpan.line_start, logSpan.column_start, logSpan.line_end, logSpan.column_end, msg.message);
            }

        }
    }
}
