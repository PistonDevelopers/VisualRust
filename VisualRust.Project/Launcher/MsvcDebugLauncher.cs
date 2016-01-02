using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project.Launcher
{
    class MsvcDebugLauncher : IRustProjectLauncher
    {
        readonly LauncherEnvironment env;

        public MsvcDebugLauncher(LauncherEnvironment env)
        {
            this.env = env;
        }

        public void Launch(string path, string args, string workingDir)
        {
            VsDebugTargetInfo4 target = new VsDebugTargetInfo4
            {
                dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess,
                guidLaunchDebugEngine = Constants.NativeOnlyEngine,
                bstrExe = path,
                bstrArg = args,
                bstrCurDir = workingDir
            };
            env.LaunchVsDebugger(target);
        }
    }
}
