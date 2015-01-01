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
                Utils.CreateEmptySolution(TestContext.TestDir, Utils.GetCallingFunction());
                Assert.AreEqual<int>(0, Utils.ProjectCount());
                Utils.CreateProjectFromTemplate(Utils.GetCallingFunction(), "Rust Library", "Rust", false);
                Assert.AreEqual<int>(1, Utils.ProjectCount());
            });
        }

        [TestMethod]
        [HostType("VS IDE")]
        [TestProperty("VsHiveName", "12.0Exp")]
        public void CreateRustApplicationProject()
        {
            UIThreadInvoker.Invoke((Action)delegate()
            {
                Utils.CreateEmptySolution(TestContext.TestDir, Utils.GetCallingFunction());
                Assert.AreEqual<int>(0, Utils.ProjectCount());
                Utils.CreateProjectFromTemplate(Utils.GetCallingFunction(), "Rust Application", "Rust", false);
                Assert.AreEqual<int>(1, Utils.ProjectCount());
            });
        }
    }
}
