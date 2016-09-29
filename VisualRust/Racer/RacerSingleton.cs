﻿using EnvDTE;
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
    public class RacerSingleton
    {
        private const string SystemRacerExecutable = "racer.exe";
        private const string BundledRacerExecutable = "racer-120e98b.exe";
        private const string RacerSourceEnviromentVariable = "RUST_SRC_PATH";
        private const int TimeoutMillis = 3000;

        private string racerPathForExecution;
        private string racerSourcesLocation;

        private static RacerSingleton instance;

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static RacerSingleton Instance
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
                instance = new RacerSingleton();
        }

        private RacerSingleton()
        {
        }
        
        private void ReinitializeRacerPaths()
        {
            DTE env = (DTE)VisualRustPackage.GetGlobalService(typeof(DTE));
            // If path to racer.exe is specifed, use it
            if(Utils.GetVisualRustProperty<bool>(env, "UseCustomRacer"))
                racerPathForExecution = Utils.GetVisualRustProperty<string>(env, "CustomRacerPath");
            else
                racerPathForExecution = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Racer", BundledRacerExecutable);
            // Same for custom RUST_SRC_PATH
            if(Utils.GetVisualRustProperty<bool>(env, "UseCustomSources"))
                racerSourcesLocation = Utils.GetVisualRustProperty<string>(env, "CustomSourcesPath");
            else
                racerSourcesLocation = null;
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
            ReinitializeRacerPaths();
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
                    if(this.racerSourcesLocation != null)
                        process.StartInfo.EnvironmentVariables[RacerSourceEnviromentVariable] = racerSourcesLocation;

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
    }
}
