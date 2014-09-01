using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace VisualRust.Build
{
    // TODO: Properly log errors and warnings
    public class Rustc : Microsoft.Build.Utilities.Task
    {
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
                this.Log.LogErrorFromException(ex, true, true, null);
                return false;
            }
        }

        private bool ExecuteInner()
        {
            StringBuilder sb = new StringBuilder();
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
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            try
            {
                Process process = Process.Start(psi);
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                this.Log.LogMessagesFromStream(process.StandardOutput, MessageImportance.Normal);
                this.Log.LogError(process.StandardError.ReadToEnd());
                if (process.ExitCode != 0)
                {
                    this.Log.LogError(process.StandardError.ReadToEnd());
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }
    }
}
