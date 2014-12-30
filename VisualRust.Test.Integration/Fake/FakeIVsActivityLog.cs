using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Test.Integration.Fake
{
    class FakeIVsActivityLog : IVsActivityLog
    {
        public int LogEntry(uint actType, string pszSource, string pszDescription)
        {
            return VSConstants.S_OK;
        }

        public int LogEntryGuid(uint actType, string pszSource, string pszDescription, Guid guid)
        {
            return VSConstants.S_OK;
        }

        public int LogEntryGuidHr(uint actType, string pszSource, string pszDescription, Guid guid, int hr)
        {
            return VSConstants.S_OK;
        }

        public int LogEntryGuidHrPath(uint actType, string pszSource, string pszDescription, Guid guid, int hr, string pszPath)
        {
            return VSConstants.S_OK;
        }

        public int LogEntryGuidPath(uint actType, string pszSource, string pszDescription, Guid guid, string pszPath)
        {
            return VSConstants.S_OK;
        }

        public int LogEntryHr(uint actType, string pszSource, string pszDescription, int hr)
        {
            return VSConstants.S_OK;
        }

        public int LogEntryHrPath(uint actType, string pszSource, string pszDescription, int hr, string pszPath)
        {
            return VSConstants.S_OK;
        }

        public int LogEntryPath(uint actType, string pszSource, string pszDescription, string pszPath)
        {
            return VSConstants.S_OK;
        }
    }
}
