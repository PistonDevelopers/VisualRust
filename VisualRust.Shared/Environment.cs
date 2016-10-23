using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VisualRust.Shared
{
    public static class Environment
    {
        private const string InnoPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Rust_is1";
        private static readonly string[] MozillaPaths = { @"Software\Mozilla Foundation", @"Software\The Rust Project Developers" };
        private const string install = "InstallLocation";

        public const string DefaultTarget = "default";

        public static string FindInstallPath(string target)
        {
            return GetAllInstallPaths()
                .Select(p => Path.Combine(p, "bin"))
                .FirstOrDefault(p => CanBuildTarget(Path.Combine(p, "rustc.exe"), target));
        }

        public static IEnumerable<TargetTriple> FindInstalledTargets()
        {
            return GetAllInstallPaths()
                   .Select(path => Path.Combine(path, "bin", "rustc.exe"))
                   .Select(GetTarget)
                   .Where(t => t != null);
        }

        public static IEnumerable<string> FindCurrentUserInstallPaths()
        {
            if (System.Environment.Is64BitOperatingSystem)
            {
                return GetInstallRoots(RegistryHive.CurrentUser, RegistryView.Registry64)
                    .Union(GetInstallRoots(RegistryHive.CurrentUser, RegistryView.Registry32));
            }
            else
            {
                return GetInstallRoots(RegistryHive.CurrentUser, RegistryView.Registry32);
            }
        }

        // TODO: this is wrong, because we're checking the host triple, not the availability of a target
        public static TargetTriple GetTarget(string exePath)
        {
            if (!File.Exists(exePath))
                return null;
            ProcessStartInfo psi = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = exePath,
                RedirectStandardOutput = true,
                Arguments = "-Vv",
                StandardOutputEncoding = Encoding.UTF8
            };
            string verboseVersion;
            try
            {
                Process proc = Process.Start(psi);
                verboseVersion = proc.StandardOutput.ReadToEnd();
            }
            catch (Win32Exception)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
            Match hostMatch = Regex.Match(verboseVersion, "^host:\\s*(.+)$", RegexOptions.Multiline);
            if (hostMatch.Groups.Count == 1)
                return null;
            return TargetTriple.Create(hostMatch.Groups[1].Value);
        }

        public static IEnumerable<string> GetAllInstallPaths()
        {
            if (System.Environment.Is64BitOperatingSystem)
            {
                return GetInstallRoots(RegistryHive.CurrentUser, RegistryView.Registry64)
                    .Union(GetInstallRoots(RegistryHive.CurrentUser, RegistryView.Registry32))
                    .Union(new string[] { GetMultirustInstallRoot(), GetRustupInstallRoot() })
                    .Union(GetInstallRoots(RegistryHive.LocalMachine, RegistryView.Registry64))
                    .Union(GetInstallRoots(RegistryHive.LocalMachine, RegistryView.Registry32))
                    .Union(GetInnoInstallRoot());
            }
            else
            {
                return GetInstallRoots(RegistryHive.CurrentUser, RegistryView.Registry32)
                    .Union(new string[] { GetMultirustInstallRoot(), GetRustupInstallRoot() })
                    .Union(GetInstallRoots(RegistryHive.LocalMachine, RegistryView.Registry32))
                    .Union(GetInnoInstallRoot());
            }
        }

        static string GetMultirustInstallRoot()
        {
            string multirustHome = System.Environment.GetEnvironmentVariable("MULTIRUST_HOME");
            if (String.IsNullOrEmpty(multirustHome))
            {
                string localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(localAppData, ".multirust");
            }
            return multirustHome;
        }

        static string GetRustupInstallRoot()
        {
            // rustup.rs installs into %CARGO_HOME%\bin\ and %CARGO_HOME% defaults to %USERPROFILE%\.cargo\
            string cargoHome = System.Environment.GetEnvironmentVariable("CARGO_HOME");
            if (String.IsNullOrEmpty(cargoHome))
            {
                string userprofile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
                return Path.Combine(userprofile, ".cargo");
            }
            return cargoHome;
        }

        private static string[] GetInnoInstallRoot()
        {
            RegistryKey innoKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(InnoPath);
            if (innoKey == null)
                return new string[0];
            string installPath = innoKey.GetValue("InstallLocation") as string;
            if (installPath == null)
                return new string[0];
            return new[] { installPath };
        }

        private static IEnumerable<string> GetInstallRoots(RegistryHive hive, RegistryView view)
        {
            foreach (string root in MozillaPaths)
            {
                RegistryKey mozillaKey = RegistryKey.OpenBaseKey(hive, view).OpenSubKey(root);
                if (mozillaKey == null)
                    continue;
                var paths = mozillaKey
                    .GetSubKeyNames()
                    .Where(n => n.StartsWith("Rust", StringComparison.OrdinalIgnoreCase))
                    .SelectMany(n => AllSubKeys(mozillaKey.OpenSubKey(n)))
                    .Select(k => k.GetValue("InstallDir") as string)
                    .Where(x => x != null);
                foreach(string path in paths)
                    yield return path;
            }
        }

        private static IEnumerable<RegistryKey> AllSubKeys(RegistryKey key)
        {
            return key.GetSubKeyNames().Select(n => key.OpenSubKey(n));
        }

        private static bool CanBuildTarget(string exePath, string target)
        {
            if (!File.Exists(exePath))
                return false;
            if (String.Equals(DefaultTarget, target, StringComparison.OrdinalIgnoreCase))
                return true;
            TargetTriple triple = GetTarget(exePath);
            return triple != null
                   && String.Equals(triple.ToString(), target, StringComparison.OrdinalIgnoreCase);
        }
    }
}
