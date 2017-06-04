using System.Threading.Tasks;
using VisualRust.Shared;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Debuggers;
using Microsoft.VisualStudio.ProjectSystem.Utilities.DebuggerProviders;
using Microsoft.VisualStudio.ProjectSystem.VS.Debuggers;
#else
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
#endif

namespace VisualRust.ProjectSystem
{
    internal class MsvcDebugLaunchSettingsProvider : IDebugLaunchSettingsProvider
    {
        public Task<DebugLaunchSettings> GetLaunchSettingsAsync(string executable, string arguments, string workingDirectory,
           DebugLaunchOptions options, Cargo cargo, TargetTriple triple)
        {
            var target = new DebugLaunchSettings(options)
            {
                LaunchOperation = DebugLaunchOperation.CreateProcess,
                LaunchDebugEngineGuid = DebuggerEngines.NativeOnlyEngine,
                Executable = executable,
                Arguments = arguments,
                CurrentDirectory = workingDirectory
            };
            return Task.FromResult(target);
        }
    }
}
