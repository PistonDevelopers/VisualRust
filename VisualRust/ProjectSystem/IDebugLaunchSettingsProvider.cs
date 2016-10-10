using Microsoft.VisualStudio.ProjectSystem.Debuggers;
using Microsoft.VisualStudio.ProjectSystem.Utilities.DebuggerProviders;
using System;
using System.Threading.Tasks;
using VisualRust.Shared;

namespace VisualRust.ProjectSystem
{
    internal interface IDebugLaunchSettingsProvider
    {
        Task<DebugLaunchSettings> GetLaunchSettingsAsync(string executable, string arguments, string workingDirectory,
            DebugLaunchOptions options, RustDebugger debuggerProperties, Cargo cargo, TargetTriple triple);
    }
}
