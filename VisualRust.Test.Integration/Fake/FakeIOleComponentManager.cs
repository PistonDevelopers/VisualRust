using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Test.Integration.Fake
{
    class FakeIOleComponentManager : IOleComponentManager
    {
        public int FContinueIdle()
        {
            throw new NotImplementedException();
        }

        public int FCreateSubComponentManager(object piunkOuter, object piunkServProv, ref Guid riid, out IntPtr ppvObj)
        {
            throw new NotImplementedException();
        }

        public int FGetActiveComponent(uint dwgac, out IOleComponent ppic, OLECRINFO[] pcrinfo, uint dwReserved)
        {
            throw new NotImplementedException();
        }

        public int FGetParentComponentManager(out IOleComponentManager ppicm)
        {
            throw new NotImplementedException();
        }

        public int FInState(uint uStateID, IntPtr pvoid)
        {
            throw new NotImplementedException();
        }

        public int FOnComponentActivate(uint dwComponentID)
        {
            throw new NotImplementedException();
        }

        public int FOnComponentExitState(uint dwComponentID, uint uStateID, uint uContext, uint cpicmExclude, IOleComponentManager[] rgpicmExclude)
        {
            throw new NotImplementedException();
        }

        public int FPushMessageLoop(uint dwComponentID, uint uReason, IntPtr pvLoopData)
        {
            throw new NotImplementedException();
        }

        public int FRegisterComponent(IOleComponent piComponent, OLECRINFO[] pcrinfo, out uint pdwComponentID)
        {
            pdwComponentID = 0;
            return VSConstants.S_OK;
        }

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        {
            throw new NotImplementedException();
        }

        public int FRevokeComponent(uint dwComponentID)
        {
            throw new NotImplementedException();
        }

        public int FSetTrackingComponent(uint dwComponentID, int fTrack)
        {
            throw new NotImplementedException();
        }

        public int FUpdateComponentRegistration(uint dwComponentID, OLECRINFO[] pcrinfo)
        {
            throw new NotImplementedException();
        }

        public void OnComponentEnterState(uint dwComponentID, uint uStateID, uint uContext, uint cpicmExclude, IOleComponentManager[] rgpicmExclude, uint dwReserved)
        {
            throw new NotImplementedException();
        }

        public void QueryService(ref Guid guidService, ref Guid iid, out IntPtr ppvObj)
        {
            throw new NotImplementedException();
        }
    }
}
