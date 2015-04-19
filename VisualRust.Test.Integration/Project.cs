using System;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Deployment.WindowsInstaller;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Build.Evaluation;

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
            Installer.ConfigureProduct("{35295818-DAA6-43A9-997B-11EB194EFB2F}", 0, InstallState.Absent, "");
        }

        [TestCategory("Explicit")]
        [TestMethod]
        public void BuildTrivialProject()
        {
            var proj = new ProjectCollection().LoadProject(@"MSBuild\Trivial\trivial.rsproj");
            AssertBuildsProject(proj, "Build");
            Assert.IsTrue(File.Exists(@"MSBuild\Trivial\target\Debug\trivial.exe"));
            Directory.Delete(@"MSBuild\Trivial\obj", true);
            Directory.Delete(@"MSBuild\Trivial\target", true);
        }

        [TestCategory("Explicit")]
        [TestMethod]
        public void CleanWithoutBuild()
        {
            AssertBuildEnviromentIsClean(@"MSBuild\Trivial");
            var proj = new ProjectCollection().LoadProject(@"MSBuild\Trivial\trivial.rsproj");
            AssertBuildsProject(proj, "Clean");
            AssertBuildEnviromentIsClean(@"MSBuild\Trivial");
        }

        [TestCategory("Explicit")]
        [TestMethod]
        public void CleanAfterBuild()
        {
            var proj = new ProjectCollection().LoadProject(@"MSBuild\Trivial\trivial.rsproj");
            AssertBuildsProject(proj, "Build");
            AssertBuildsProject(proj, "Clean");
            AssertBuildEnviromentIsClean(@"MSBuild\Trivial");
        }

        private static void AssertBuildEnviromentIsClean(string path)
        {
            if(Directory.Exists(Path.Combine(path, "obj")))
                AssertDirectoryIsEmpty(Path.Combine(path, "obj"));
            if(Directory.Exists(Path.Combine(path, "target")))
                AssertDirectoryIsEmpty(Path.Combine(path, "target"));
        }


        private static void AssertDirectoryIsEmpty(string path)
        {
            var files = Directory.EnumerateFiles(path).ToArray();
            if(files.Length > 0)
            {
                Assert.Fail(
                    "Directory {0} not empty. Files: {1}",
                    path,
                    String.Join(", ", files.Select(f => f.Substring(path.Length + 1))));
            }
            else
            {
                foreach(var dir in Directory.GetDirectories(path))
                AssertDirectoryIsEmpty(dir);
            }
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
