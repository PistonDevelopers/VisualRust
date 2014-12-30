using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Test.Integration.Fake
{
    class FakeIVsRegisterProjectTypes : IVsRegisterProjectTypes
    {
        public int RegisterProjectType(ref Guid rguidProjType, IVsProjectFactory pVsPF, out uint pdwCookie)
        {
            rguidProjType = Guid.Empty;
            pdwCookie = 0;
            return VSConstants.S_OK;
        }

        public int UnregisterProjectType(uint dwCookie)
        {
            throw new NotImplementedException();
        }
    }
}
