using System;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Deployment.WindowsInstaller;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace VisualRust.Test.Integration
{
    [TestClass]
    public class Project
    {
        private class StringLogger : ConsoleLogger
        {
            private StringBuilder sb = new StringBuilder();
            public StringLogger()
            {
                this.WriteHandler = new WriteHandler(Write);
            }

            private void Write(string message)
            {
                sb.Append(message);
            }

            public override string ToString()
            {
                return sb.ToString();
            }
        }

        [ClassInitialize]
        public static void InstallRustLocally(TestContext testContext)
        {
            AssertNoRustInstalled();
            Installer.SetInternalUI(InstallUIOptions.Silent);
            Installer.InstallProduct(
                @"MSBuild\rust-1.0.0-beta.2-i686-pc-windows-gnu.msi",
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
            var proj = new Microsoft.Build.Evaluation.Project(@"MSBuild\Trivial\trivial.rsproj");
            AssertBuildsProject(proj, "Build");
        }

        private static void AssertBuildsProject(Microsoft.Build.Evaluation.Project proj, string target)
        {
            var logger = new StringLogger();
            if(!proj.Build(target, new StringLogger[] { logger }))
                Assert.Fail(logger.ToString());
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
