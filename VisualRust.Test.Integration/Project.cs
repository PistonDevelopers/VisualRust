using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Deployment.WindowsInstaller;
using System.IO;

namespace VisualRust.Test.Integration
{
    [TestClass]
    public class Project
    {
        [ClassInitialize]
        public static void InstallRustLocally(TestContext testContext)
        {
            AssertNoRustInstalled();
            Installer.SetInternalUI(InstallUIOptions.Silent);
            Installer.InstallProduct(
                @"Build\rust-1.0.0-beta-i686-pc-windows-gnu.msi",
                String.Format("INSTALLDIR_USER=\"{0}\"", Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())));
        }

        [ClassCleanup]
        public static void UninstallRustLocally()
        {
            Installer.ConfigureProduct("{17A31558-A8C0-4C44-A679-09F1B8732999}", 0, InstallState.Absent, "");
        }

        [TestCategory("Explicit")]
        [TestMethod]
        public void BuildTrivialProject()
        {
            var proj = new Microsoft.Build.Evaluation.Project(@"Build\Trivial\trivial.rsproj");
            proj.Build("Build");
        }

        private static void AssertNoRustInstalled()
        {
            string[] userInstallPaths = Shared.Environment.FindCurrentUserInstallPaths().ToArray();
            if(userInstallPaths.Length > 0)
            {
                string tabbedPaths = String.Join("\n", userInstallPaths);
                Assert.Fail("Found existing user-specific rust installations:\n{0}", tabbedPaths);
            }
        }
    }
}
