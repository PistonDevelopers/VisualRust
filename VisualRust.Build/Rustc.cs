using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace VisualRust.Build
{
    public class Rustc : Microsoft.Build.Utilities.Task
    {
        private static readonly Regex defectRegex = new Regex(@"^([^:^]+):(\d+):(\d+):\s+(\d+):(\d+)\s+(.*)$", RegexOptions.Multiline | RegexOptions.CultureInvariant);
        private static readonly Regex errorCodeRegex = new Regex(@"\[[A-Z]\d\d\d\d\]$", RegexOptions.CultureInvariant);

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

        private int? debugInfo;
        /// <summary>
        /// Sets --debuginfo option. Default value is 0.
        /// </summary>
        public int DebugInfo 
        {
            get { return debugInfo.HasValue ? debugInfo.Value : 0; }
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

        /// <summary>
        /// Sets --sysroot option.
        /// </summary>
        public string SystemRoot { get; set; }

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
                sb.AppendFormat(" --crate-type {0}", String.Join(",", CrateType));
            if(Emit.Length > 0)
                sb.AppendFormat(" --emit {0}", String.Join(",", Emit));
            if(!String.IsNullOrWhiteSpace(CrateName))
                sb.AppendFormat(" --crate-name {0}", CrateName);
            if(debugInfo.HasValue)
                sb.AppendFormat(" --debuginfo {0}", DebugInfo);
            if(OutputFile != null)
                sb.AppendFormat(" -o {0}", OutputFile);
            if (optimizationLevel.HasValue)
                sb.AppendFormat(" --opt-level {0}", OptimizationLevel);
            if (OutputDirectory != null)
                sb.AppendFormat(" --out-dir {0}", OutputDirectory);
            if (SystemRoot != null)
                sb.AppendFormat(" --sysroot {0}", SystemRoot);
            if (test.HasValue && test.Value)
                sb.Append(" --test");
            if (TargetTriple != null)
                sb.AppendFormat(" --target {0}", TargetTriple);
            if(LintsAsWarnings.Length > 0)
                sb.AppendFormat(" -W {0}", String.Join(",", LintsAsWarnings));
            if(LintsAsAllowed.Length > 0)
                sb.AppendFormat(" -A {0}", String.Join(",", LintsAsAllowed));
            if(LintsAsDenied.Length > 0)
                sb.AppendFormat(" -D {0}", String.Join(",", LintsAsDenied));
            if(LintsAsForbidden.Length > 0)
                sb.AppendFormat(" -F {0}", String.Join(",", LintsAsForbidden));
            if (CodegenOptions != null)
                sb.AppendFormat(" -C {0}", CodegenOptions);
            sb.AppendFormat(" {0}", Input);
            // Currently we hope that rustc is in the path
            var psi = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                FileName = "rustc.exe",
                UseShellExecute = false,
                WorkingDirectory = WorkingDirectory,
                Arguments = sb.ToString(),
                RedirectStandardError = true
            };
            try
            {
                Process process = Process.Start(psi);
                process.WaitForExit();
                string errorOutput = process.StandardError.ReadToEnd();
                MatchCollection errorMatches = defectRegex.Matches(errorOutput);
                // We found some warning or errors in the output, print them out
                foreach(Match match in errorMatches)
                {
                    LogParsedDefect(match);
                }
                // rustc failed but we couldn't sniff anything from stderr, probabaly internal error
                if (process.ExitCode != 0 && errorMatches.Count == 0)
                {
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

        private void LogParsedDefect(Match match)
        {
            Match errorMatch = errorCodeRegex.Match(match.Groups[6].Value);
            string errorCode = errorMatch.Success ? errorMatch.Groups[0].Value : null;
            if (match.Groups[6].Value.StartsWith("warning: "))
            {
                string msg = match.Groups[6].Value.Substring(9, match.Groups[6].Value.Length - 9 - (errorCode != null ? 8 : 0));
                this.Log.LogWarning(
                    null,
                    errorCode,
                    null,
                    match.Groups[1].Value,
                    Int32.Parse(match.Groups[2].Value, System.Globalization.NumberStyles.None),
                    Int32.Parse(match.Groups[3].Value, System.Globalization.NumberStyles.None),
                    Int32.Parse(match.Groups[4].Value, System.Globalization.NumberStyles.None),
                    Int32.Parse(match.Groups[5].Value, System.Globalization.NumberStyles.None),
                    msg);
            }
            else
            {
                bool startsWithError = match.Groups[6].Value.StartsWith("error: ");
                string msg = match.Groups[6].Value.Substring((startsWithError ? 7 : 0), match.Groups[6].Value.Length - (startsWithError ? 7 : 0) - (errorCode != null ? 8 : 0));
                this.Log.LogError(
                    null,
                    errorCode,
                    null,
                    match.Groups[1].Value,
                    Int32.Parse(match.Groups[2].Value, System.Globalization.NumberStyles.None),
                    Int32.Parse(match.Groups[3].Value, System.Globalization.NumberStyles.None),
                    Int32.Parse(match.Groups[4].Value, System.Globalization.NumberStyles.None),
                    Int32.Parse(match.Groups[5].Value, System.Globalization.NumberStyles.None),
                    msg);
            }
        }
    }
}
