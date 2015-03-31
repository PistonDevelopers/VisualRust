using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Shared
{
    public class Environment
    {
        private const string InnoPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Rust_is1";
        private const string InnoKey = "InstallLocation";

        // I'm really torn between "default", "local", "native", "unspecified" and "any"
        public const string DefaultTarget = "default";

        /* 
         * If the target is "default" just return first location
         * Otherwise check for bin\rustlib\<target>
         */
        public static string FindInstallPath(string target)
        {
            foreach(string path in System.Environment.GetEnvironmentVariable("PATH").Split(System.IO.Path.PathSeparator))
            {
                if(File.Exists(Path.Combine(path, "rustc.exe")) 
                    && File.Exists(Path.Combine(path, "cargo.exe"))
                    && CanActuallyBuildTarget(path, target))
                return path;
            }
            RegistryKey installpath = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(InnoPath);
            if (installpath == null)
                return null;
            object fullInstallKey = installpath.GetValue(InnoKey);
            return fullInstallKey != null ? fullInstallKey.ToString() : null;
        }

        private static bool CanActuallyBuildTarget(string path, string target)
        {
            if(String.Equals(DefaultTarget, target, StringComparison.OrdinalIgnoreCase))
                return true;
            return Directory.Exists(Path.Combine(path, "rustlib", target));
        }
    }
}
