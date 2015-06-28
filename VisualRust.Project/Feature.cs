using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    public static class Feature
    {
        public static bool Gdb(IServiceProvider provider)
        {
            return IsVs14OrHigher(provider) && IsGdbEngineInstalled(provider);
        }

        private static bool IsVs14OrHigher(IServiceProvider provider)
        {
            var dte = provider.GetService(typeof(SDTE)) as EnvDTE.DTE;
            if(dte == null)
                return false;
            Version ver;
            if (!Version.TryParse(dte.Version, out ver))
                return false;
            return ver.Major >= 14;
        }

        private static bool IsGdbEngineInstalled(IServiceProvider provider)
        {
            var debugger = provider.GetService(typeof(SVsShellDebugger)) as IVsDebugger2;
            if(debugger == null)
                return false;
            Guid gdbEngine = Constants.GdbEngine;
            string _;
            return debugger.GetEngineName(ref gdbEngine, out _) == VSConstants.S_OK;
        }
    }
}
