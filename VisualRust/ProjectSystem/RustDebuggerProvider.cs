using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debuggers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.DebuggerProviders;
using Microsoft.VisualStudio.ProjectSystem.VS.Debuggers;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using VisualRust.Shared;

namespace VisualRust.ProjectSystem
{
    [ExportDebugger(RustDebugger.SchemaName)]
    [AppliesTo(VisualRustPackage.UniqueCapability)]
    public class RustDebuggerProvider : DebugLaunchProviderBase
    {
        [Import]
        private ProjectProperties Properties { get; set; }

        private Cargo cargo;

        [ImportingConstructor]
        public RustDebuggerProvider(ConfiguredProject configuredProject) : base(configuredProject)
        {
            // TODO: this should initialized once when the project is loaded and then be updated using a file watcher on Cargo.toml
            cargo = Cargo.FindSupportedInstallation();
        }

        public override Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions)
        {
            return TplExtensions.TrueTask;
        }

        public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
        {
            var targets = new List<IDebugLaunchSettings>();

            if (cargo == null)
            {
                return targets;
            }

            var props = await Properties.GetConfigurationGeneralPropertiesAsync();
            var manifestPath = await props.ManifestPath.GetEvaluatedValueAsync();
            if (String.IsNullOrWhiteSpace(manifestPath))
            {
                manifestPath = "Cargo.toml";
            }
            cargo.WorkingDirectory = Path.GetDirectoryName(Path.Combine(Path.GetDirectoryName(ConfiguredProject.UnconfiguredProject.FullPath), manifestPath));
            // TODO: this should initialized once when the project is loaded and then be updated using a file watcher on Cargo.toml
            // can't use `cargo rustc -- --print file-names`, because it would build all dependencies first
            var metadata = await cargo.ReadMetadataAsync(false);
            var pkgTarget = metadata.packages[0].targets[0];
            if (pkgTarget.kind.Contains("bin")) // TODO: show a real error when the project is not a binary
            {
                var debuggerProperties = await Properties.GetRustDebuggerPropertiesAsync();
                var targetConfiguration = ConfiguredProject.ProjectConfiguration.Dimensions["Configuration"].ToLowerInvariant();
                string executablePath = ConfiguredProject.UnconfiguredProject.MakeRooted(Path.Combine("target", targetConfiguration, pkgTarget.name + ".exe"));
                string arguments = await debuggerProperties.StartArguments.GetEvaluatedValueAsync();
                string workingDirectory = await debuggerProperties.StartWorkingDirectory.GetValueAsPathAsync(false, false);
                if (string.IsNullOrWhiteSpace(workingDirectory))
                {
                    workingDirectory = Path.GetDirectoryName(ConfiguredProject.UnconfiguredProject.FullPath);
                }

                // TODO: support --target flag (the host triple is not necessarily correct)
                var targetTriple = await cargo.GetHostTargetTripleAsync();
                IDebugLaunchSettingsProvider provider = null;
                if (targetTriple.Abi == "gnu")
                    provider = new GnuDebugLaunchSettingsProvider();
                else if (targetTriple.Abi == "msvc")
                    provider = new MsvcDebugLaunchSettingsProvider();
                
                var target = await provider.GetLaunchSettingsAsync(executablePath, arguments, workingDirectory,
                    launchOptions, cargo, targetTriple);

                targets.Add(target);
            }

            return targets;
        }

        public override async Task LaunchAsync(DebugLaunchOptions launchOptions)
        {
            await base.LaunchAsync(launchOptions);
            IVsCommandWindow commandWnd = (IVsCommandWindow)ServiceProvider.GetService(typeof(SVsCommandWindow));
            commandWnd.ExecuteCommand("Tools.Alias gdb Debug.VRDebugExec");
        }
    }
}
