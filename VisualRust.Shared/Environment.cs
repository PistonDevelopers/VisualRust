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

        public static IEnumerable<string> FindInstalledTargets()
        {
            foreach(string path in System.Environment.GetEnvironmentVariable("PATH").Split(System.IO.Path.PathSeparator))
            {
                if(File.Exists(Path.Combine(path, "rustc.exe")) 
                    && File.Exists(Path.Combine(path, "cargo.exe")))
                foreach(string targetPath in SniffTargets(path))
                {
                    yield return targetPath;
                }
            }
            RegistryKey installpath = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(InnoPath);
            if (installpath != null)
            {
                object fullInstallKey = installpath.GetValue(InnoKey);
                if(fullInstallKey != null)
                {
                    foreach(string targetPath in SniffTargets(Path.Combine(fullInstallKey.ToString(), "bin")))
                    {
                        yield return targetPath;
                    }
                }
            }
        }

        private static bool CanActuallyBuildTarget(string binPath, string target)
        {
            if(String.Equals(DefaultTarget, target, StringComparison.OrdinalIgnoreCase))
                return true;
            return Directory.Exists(Path.Combine(binPath, "rustlib", target));
        }

        private static IEnumerable<string> SniffTargets(string binPath)
        {
            try
            {
                string root = Path.Combine(binPath, "rustlib");
                return Directory.GetDirectories(root, "*-*-*").Select(p => p.Substring(root.Length + 1).ToLowerInvariant());
            }
            catch(IOException)
            {
                return new string[0];
            }
        }
    }
}
