using System;
using System.Threading.Tasks;
using VisualRust.Shared;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Debuggers;
using Microsoft.VisualStudio.ProjectSystem.Utilities.DebuggerProviders;
#else
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
#endif

namespace VisualRust.ProjectSystem
{
    internal interface IDebugLaunchSettingsProvider
    {
        Task<DebugLaunchSettings> GetLaunchSettingsAsync(string executable, string arguments, string workingDirectory,
            DebugLaunchOptions options, Cargo cargo, TargetTriple triple);
    }
}
