using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VsSDK.UnitTestLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Test.Integration.Fake;

namespace VisualRust.Test.Integration
{
    [TestClass]
    public class SanityCheck
    {
        [TestMethod]
        public void SetSite()
        {
            IVsPackage package = new VisualRustPackage() as IVsPackage;
            OleServiceProvider serviceProvider = OleServiceProvider.CreateOleServiceProviderWithBasicServices();
#if VS12
            serviceProvider.AddService(typeof(SVsActivityLog), new FakeIVsActivityLog(), true);
#endif
            serviceProvider.AddService(typeof(SVsSolution), new FakeIVsSolution(), true);
            serviceProvider.AddService(typeof(SVsRegisterProjectTypes), new FakeIVsRegisterProjectTypes(), true);
            serviceProvider.AddService(typeof(SOleComponentManager), new FakeIOleComponentManager(), true);
            serviceProvider.AddService(typeof(SVsRunningDocumentTable), new FakeIVsRunningDocumentTable(), true);
            Assert.AreEqual(0, package.SetSite(serviceProvider));
            Assert.AreEqual(0, package.SetSite(null));
        }

    }
}
