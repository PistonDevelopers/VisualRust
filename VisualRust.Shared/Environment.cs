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
        private const string MozillaPath = @"Software\Mozilla Foundation";
        private const string install = "InstallLocation";

        // I'm really torn between "default", "local", "native", "unspecified" and "any"
        public const string DefaultTarget = "default";

        /* 
         * If the target is "default" just return the first location with "bin\rustc.exe"
         * Otherwise also require "bin\rustlib\<target>"
         */
        public static string FindInstallPath(string target)
        {
            return GetAllInstallPaths()
                .Select(p => Path.Combine(p, "bin"))
                .Where(p => File.Exists(Path.Combine(p, "rustc.exe")))
                .FirstOrDefault(p => CanActuallyBuildTarget(p, target));
        }

        public static IEnumerable<string> FindInstalledTargets()
        {
            return GetAllInstallPaths().SelectMany(SniffTargets);
        }

        public static IEnumerable<string> FindCurrentUserInstallPaths()
        {
            if(System.Environment.Is64BitOperatingSystem)
            {
                return GetInstallRoots(RegistryHive.CurrentUser, RegistryView.Registry64)
                    .Union(GetInstallRoots(RegistryHive.CurrentUser, RegistryView.Registry32));
            }
            else
            {
                return GetInstallRoots(RegistryHive.CurrentUser, RegistryView.Registry32);
            }
        }

        private static IEnumerable<string> GetAllInstallPaths()
        {
            if(System.Environment.Is64BitOperatingSystem)
            {
                return GetInstallRoots(RegistryHive.CurrentUser, RegistryView.Registry64)
                    .Union(GetInstallRoots(RegistryHive.CurrentUser, RegistryView.Registry32))
                    .Union(GetInstallRoots(RegistryHive.LocalMachine, RegistryView.Registry64))
                    .Union(GetInstallRoots(RegistryHive.LocalMachine, RegistryView.Registry32))
                    .Union(GetInnoInstallRoot());
            }
            else
            {
                return GetInstallRoots(RegistryHive.CurrentUser, RegistryView.Registry32)
                    .Union(GetInstallRoots(RegistryHive.LocalMachine, RegistryView.Registry32))
                    .Union(GetInnoInstallRoot());
            }
        }

        private static string[] GetInnoInstallRoot()
        {
            RegistryKey innoKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(InnoPath);
            if(innoKey == null)
                return new string[0];
            string installPath = innoKey.GetValue("InstallLocation") as string;
            if(installPath == null)
                return new string[0];
            return new [] { installPath };
        }

        private static IEnumerable<string> GetInstallRoots(RegistryHive hive, RegistryView view)
        {
            RegistryKey mozillaKey = RegistryKey.OpenBaseKey(hive, view).OpenSubKey(MozillaPath);
            if (mozillaKey == null)
                return new string[0];
            return mozillaKey
                .GetSubKeyNames()
                .Where(n => n.StartsWith("Rust", StringComparison.OrdinalIgnoreCase))
                .SelectMany(n => AllSubKeys(mozillaKey.OpenSubKey(n)))
                .Select(k => k.GetValue("InstallDir") as string)
                .Where(x => x != null);
        }

        private static IEnumerable<RegistryKey> AllSubKeys(RegistryKey key)
        {
            return key.GetSubKeyNames().Select(n => key.OpenSubKey(n));
        }

        private static bool CanActuallyBuildTarget(string binPath, string target)
        {
            if(String.Equals(DefaultTarget, target, StringComparison.OrdinalIgnoreCase))
                return true;
            return Directory.Exists(Path.Combine(binPath, "rustlib", target));
        }

        private static IEnumerable<string> SniffTargets(string installPath)
        {
            try
            {
                string root = Path.Combine(installPath, "bin", "rustlib");
                return Directory.GetDirectories(root, "*-*-*").Select(p => p.Substring(root.Length + 1).ToLowerInvariant());
            }
            catch(IOException)
            {
                return new string[0];
            }
        }
    }
}
