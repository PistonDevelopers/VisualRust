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


			string executablePath = ConfiguredProject.UnconfiguredProject.MakeRooted("target\\debug\\rust_application_cargo.exe");
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

			return targets;
		}
	}
}
