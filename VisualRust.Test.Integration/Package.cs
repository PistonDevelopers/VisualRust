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
    }
}
