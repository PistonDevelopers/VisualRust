//using Microsoft.VisualStudio;
//using Microsoft.VisualStudio.Shell.Interop;
//using Microsoft.VisualStudioTools.Project.Automation;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using VisualRust.Project;

//namespace VisualRust
//{
//    sealed class RunningDocTableEventsListener : IVsRunningDocTableEvents, IDisposable
//    {
//        private uint cookie;
//        private IVsRunningDocumentTable rdt;

//        public RunningDocTableEventsListener(IVsRunningDocumentTable rdt)
//        {
//            this.rdt = rdt;
//            rdt.AdviseRunningDocTableEvents(this, out cookie);
//        }

//        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
//        {
//            if (rdt != null && grfAttribs != (uint)__VSRDTATTRIB.RDTA_DocDataIsNotDirty && grfAttribs != (uint)__VSRDTATTRIB.RDTA_DocDataReloaded)
//                return VSConstants.S_OK;
//            uint docFlags;
//            uint readLocks;
//            uint writeLocks;
//            string docPath;
//            IVsHierarchy hier;
//            uint nodeId;
//            IntPtr docData;
//            ErrorHandler.ThrowOnFailure(rdt.GetDocumentInfo(docCookie, out docFlags, out readLocks, out writeLocks, out docPath, out hier, out nodeId, out docData));
//            object projectAO;
//            ErrorHandler.ThrowOnFailure(hier.GetProperty((uint)VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out projectAO));
//            OAProject project = projectAO as OAProject;
//            if (project == null)
//                return VSConstants.S_OK;
//            ((RustProjectNode)project.Project).OnNodeDirty(nodeId);
//            return VSConstants.S_OK;
//        }

//        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
//        {
//            return VSConstants.S_OK;
//        }

//        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
//        {
//            return VSConstants.S_OK;
//        }

//        public int OnAfterSave(uint docCookie)
//        {
//            return VSConstants.S_OK;
//        }

//        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
//        {
//            return VSConstants.S_OK;
//        }

//        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
//        {
//            return VSConstants.S_OK;
//        }

//        public void Dispose()
//        {
//            if (rdt != null)
//            {
//                rdt.UnadviseRunningDocTableEvents(cookie);
//                rdt = null;
//                GC.SuppressFinalize(this);
//            }
//        }

//        ~RunningDocTableEventsListener()
//        {
//            Dispose();
//        }
//    }

//}
