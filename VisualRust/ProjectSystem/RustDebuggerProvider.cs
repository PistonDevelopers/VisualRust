using Microsoft.VisualStudio.ProjectSystem.Debuggers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.DebuggerProviders;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.VS.Debuggers;
using System.IO;
using Microsoft.VisualStudio.Threading;
using VisualRust.Shared;

namespace VisualRust.ProjectSystem
{
    [ExportDebugger(RustDebugger.SchemaName)]
    [AppliesTo(VisualRustPackage.UniqueCapability)]
    public class RustDebuggerProvider : DebugLaunchProviderBase
    {
        [Import]
        private ProjectProperties Properties { get; set; }

        [ImportingConstructor]
        public RustDebuggerProvider(ConfiguredProject configuredProject) : base(configuredProject)
        {
        }

        public override Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions)
        {
            return TplExtensions.TrueTask;
        }

        public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
        {
            var targets = new List<IDebugLaunchSettings>();
            var debuggerProperties = await Properties.GetRustDebuggerPropertiesAsync();

            // TODO: this should initialized once when the project is loaded and then be updated using a file watcher on Cargo.toml
            var cargo = Cargo.FindSupportedInstallation();
            // TODO: use correct ManifestPath
            cargo.WorkingDirectory = Path.GetDirectoryName(ConfiguredProject.UnconfiguredProject.FullPath);
            // can't use `cargo rustc -- --print file-names`, because it would build all dependencies first
            var metadata = await cargo.ReadMetadataAsync(false);
            var pkgTarget = metadata.packages[0].targets[0];
            if (pkgTarget.kind.Contains("bin")) // TODO: show a real error when the project is not a binary
            {
                var targetConfiguration = ConfiguredProject.ProjectConfiguration.Dimensions["Configuration"].ToLowerInvariant();
                string executablePath = ConfiguredProject.UnconfiguredProject.MakeRooted(Path.Combine("target", targetConfiguration, pkgTarget.name + ".exe"));
                string arguments = (string)await debuggerProperties.StartArguments.GetValueAsync();
                string workingDirectory = await debuggerProperties.StartWorkingDirectory.GetValueAsPathAsync(false, false);
                if (string.IsNullOrWhiteSpace(workingDirectory))
                {
                    workingDirectory = Path.GetDirectoryName(ConfiguredProject.UnconfiguredProject.FullPath);
                }

                var target = new DebugLaunchSettings(launchOptions)
                {
                    LaunchOperation = DebugLaunchOperation.CreateProcess,
                    LaunchDebugEngineGuid = DebuggerEngines.NativeOnlyEngine,
                    Executable = executablePath,
                    Arguments = arguments,
                    CurrentDirectory = workingDirectory
                };

                targets.Add(target);
            }

            return targets;
        }
    }
}
