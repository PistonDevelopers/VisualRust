using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Process = System.Diagnostics.Process;

namespace VisualRust.Racer
{
    /// <summary>
    /// Wrapper for the native racer.exe binary. 
    /// </summary>
    /// <remarks>
    /// racer.exe compiled (x86) as rustc -O -o racer.exe src\main.rs    
    /// </remarks>
    public class AutoCompleter
    {
        private const string SystemRacerExecutable = "racer.exe";
        private const string BundledRacerExecutable = "racer-bf2373e.exe";
        private const int TimeoutMillis = 3000;

        /// <summary>
        /// System environment variable with path to racer.exe
        /// </summary>
        public const string EnvVarRacerPath = "RUST_RACER_PATH";

        /// <summary>
        /// System environment variable with path to rust sources folder
        /// </summary>
        private const string EnvVarRustSrcPath = "RUST_SRC_PATH";

        private string racerPathForExecution;

        private static AutoCompleter instance;

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static AutoCompleter Instance
        {
            get
            {
                if (instance == null)
                    Init();
                return instance;
            }
        }

        /// <summary>
        /// Initializes the environment for the racer autocompleter. 
        /// Can be called from package/command init to init ahead of first use.
        /// </summary>
        public static void Init()
        {
            if (instance == null)
                instance = new AutoCompleter();
        }

        private AutoCompleter()
        {
            // Check for the source dir env var required by racer.
            CheckRustSrcPath(RustSrcPath);
            
            ReinitializeRacerPath();
        }

        private static bool CheckRustSrcPath(string rustSrcPath)
        {
            if (string.IsNullOrEmpty(rustSrcPath) || !Directory.Exists(rustSrcPath))
            {
                Utils.PrintToOutput(@"Environment variable RUST_SRC_PATH must exist and point to a rust source dir for autocompletion to work, e.g. 'C:\Rust\src'");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Path to rust source folder
        /// </summary>
        public string RustSrcPath
        {
            get
            {
                var result = Environment.GetEnvironmentVariable(EnvVarRustSrcPath, EnvironmentVariableTarget.User) ??
                             Environment.GetEnvironmentVariable(EnvVarRustSrcPath, EnvironmentVariableTarget.Machine);
                return result;
            }
            set
            {
                if (CheckRustSrcPath(value))
                {
                    Environment.SetEnvironmentVariable(EnvVarRustSrcPath, value, EnvironmentVariableTarget.User);
                }                            
            }
        }

        public string RacerPath
        {
            get
            {
                var result = Environment.GetEnvironmentVariable(EnvVarRacerPath, EnvironmentVariableTarget.User) ??
                             Environment.GetEnvironmentVariable(EnvVarRacerPath, EnvironmentVariableTarget.Machine);
                return result;                
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Environment.SetEnvironmentVariable(EnvVarRacerPath, value, EnvironmentVariableTarget.User);
                }

                ReinitializeRacerPath();                
            }
        }

        private void ReinitializeRacerPath()
        {
            // If path to racer.exe specifed in options and this file is exists
            var racerPathFromOptions = RacerPath;
            if (!string.IsNullOrWhiteSpace(racerPathFromOptions) && File.Exists(racerPathFromOptions))
            {
                racerPathForExecution = racerPathFromOptions;
            }
            else
            {
                // If a racer.exe is found on the path, it is used instead of the bundled racer from $extdir\Racer.            
                if (RacerExistsOnPath())
                {
                    racerPathForExecution = SystemRacerExecutable;
                    Utils.PrintToOutput("Using racer.exe found in PATH");
                }
                else
                {
                    racerPathForExecution = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Racer", BundledRacerExecutable);
                }
            }
        }

        public static string Run(string args)
        {
            return Instance.Exec(args);
        }

        private bool RacerExistsOnPath()
        {
            try
            {
                using (new WindowsErrorMode(3))
                using (var ps = new Process())
                {
                    // Note: no attempt is made here to see if it is actually the racer.exe we want.
                    ps.StartInfo.FileName = SystemRacerExecutable;
                    ps.StartInfo.UseShellExecute = false;
                    ps.StartInfo.CreateNoWindow = true;
                    ps.Start();
                    ps.WaitForExit(1000);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private string Exec(string args)
        {
            try
            {
                using (new WindowsErrorMode(3))
                using (Process process = new Process())
                {

                    process.StartInfo.FileName = racerPathForExecution;
                    process.StartInfo.Arguments = args;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.EnvironmentVariables[EnvVarRustSrcPath] = RustSrcPath;

                    process.Start();

                    string result = process.StandardOutput.ReadToEnd();

                    // Don't want to hang waiting for the results.
                    if (!process.WaitForExit(TimeoutMillis))
                    {
                        Utils.DebugPrintToOutput("Autocomplete timed out");
                        return "";
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Utils.DebugPrintToOutput("Error executing racer.exe: {0}", ex);
                return "";
            }
        }

        class WindowsErrorMode : IDisposable
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern int SetErrorMode(int wMode);

            private readonly int oldErrorMode;

            /// <summary>
            ///     Creates a new error mode context.
            /// </summary>
            /// <param name="mode">Error mode to use. 3 is a useful value.</param>
            public WindowsErrorMode(int mode)
            {
                oldErrorMode = SetErrorMode(mode);
            }

            public void Dispose()
            {
                SetErrorMode(oldErrorMode);
            }
        }
    }
}
