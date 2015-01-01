using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio;

namespace VisualRust.Test.Integration
{
    using EnvDTE;
    using Microsoft.VisualStudioTools.Project;
    using Microsoft.VisualStudioTools.Project.Automation;

    [TestClass]
    public class Package
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [HostType("VS IDE")]
        [TestProperty("VsHiveName", "12.0Exp")]
        public void PackageLoad()
        {
            UIThreadInvoker.Invoke((Action)delegate() 
            {
                IVsShell shellService = VsIdeTestHostContext.ServiceProvider.GetService(typeof(SVsShell)) as IVsShell;
                Assert.IsNotNull(shellService);
                IVsPackage pkg;
                // TODO: refactor guid in the VisualRust project
                Guid visualRust = new Guid("40c1d2b5-528b-4966-a7b1-1974e3568abe");
                Assert.AreEqual(VSConstants.S_OK, shellService.LoadPackage(ref visualRust, out pkg));
                Assert.IsNotNull(pkg);
            });
        }

        [TestMethod]
        [HostType("VS IDE")]
        [TestProperty("VsHiveName", "12.0Exp")]
        public void CreateRustLibraryProject()
        {
            UIThreadInvoker.Invoke((Action)delegate()
            {
                string projName = Utils.GetCallingFunction();
                Utils.CreateEmptySolution(TestContext.TestDir, projName);
                Utils.CreateProjectFromTemplate(projName, "Rust Library", "Rust", false);
                ProjectNode root = Utils.GetProject(projName);
                Assert.IsNotNull(root);
                new HierarchyCheck(root)
                    .Child<FolderNode>("src")
                        .Child<FileNode>("lib.rs")
                .Run();
            });
        }

        [TestMethod]
        [HostType("VS IDE")]
        [TestProperty("VsHiveName", "12.0Exp")]
        public void CreateRustApplicationProject()
        {
            UIThreadInvoker.Invoke((Action)delegate()
            {
                string projName = Utils.GetCallingFunction();
                Utils.CreateEmptySolution(TestContext.TestDir, projName);
                Utils.CreateProjectFromTemplate(projName, "Rust Application", "Rust", false);
                ProjectNode root = Utils.GetProject(projName);
                Assert.IsNotNull(root);
                new HierarchyCheck(root)
                    .Child<FolderNode>("src")
                        .Child<FileNode>("main.rs")
                .Run();
            });
        }
    }
}
