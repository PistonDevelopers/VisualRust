using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Process = System.Diagnostics.Process;

namespace VisualRust.GPG
{
    /// <summary>
    /// Wrapper for the native gpg2.exe binary
    /// </summary>
    public class Gpg
    {
        private string homedir;
        private string gpgPath;
        private const string BundledGpgExecutable = "gpg2.exe";
        private const int TimeoutMillis = 3000;
        private static Gpg instance;

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static Gpg Instance
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
                instance = new Gpg();
        }

        private void ReinitializeGpgPaths()
        {
            DTE env = (DTE)VisualRustPackage.GetGlobalService(typeof(DTE));
            if (Utils.GetVisualRustProperty<bool>(env, "UseCustomGpg"))
                gpgPath = Utils.GetVisualRustProperty<string>(env, "CustomGpgPath");
            else
                gpgPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "gpg", BundledGpgExecutable);

            if (Utils.GetVisualRustProperty<bool>(env, "UseCustomGpgHomedir"))
                homedir = Utils.GetVisualRustProperty<string>(env, "CustomGpgHomedir");
            else
                homedir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "gpg");
        }

        private Gpg()
        {
            ReinitializeGpgPaths();
        }

        private Tuple<int, string> Exec(string args, Stream stdin)
        {
            try
            {
                using (new WindowsErrorMode(3))
                using (Process process = new Process())
                {

                    process.StartInfo.FileName = gpgPath;
                    process.StartInfo.Arguments = args;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    
                    process.Start();

                    stdin.CopyTo(process.StandardInput.BaseStream);
                    process.StandardInput.Close();
                    string result = process.StandardOutput.ReadToEnd();
                    result += process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    return Tuple.Create(process.ExitCode, result);
                }
            }
            catch (Exception ex)
            {
                Utils.DebugPrintToOutput("Error executing gpg2.exe: {0}", ex);
                return null;
            }
        }

        /// <summary>
        /// Verify that data is validly signed.
        /// </summary>
        /// <param name="data_to_verify">Stream to verify signature of.</param>
        /// <returns>true if the stream is signed with a key in the keyring, false otherwise.</returns>
        public Tuple<bool, string> Verify(Stream data_to_verify, Stream signature)
        {
            // We could avoid hitting the filesystem, and ideally we'd be using GPGME instead of shelling out, but this is... far easier.
            using (var tmp = new TemporaryFile(""))
            {
                using (var f = new FileStream(tmp.Path, FileMode.OpenOrCreate))
                {
                    signature.CopyTo(f);
                    f.Flush();
                }
                var res = Exec("--verify --no-permission-warning --keyring \"" + homedir + "\\rust-key.gpg\" " + tmp.Path + " -",
                        data_to_verify);
                return Tuple.Create(res.Item1 == 0, res.Item2);
            }
        }
    }
}
