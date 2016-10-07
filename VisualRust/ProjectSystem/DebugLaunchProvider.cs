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
	// ExportDebugger must match rule name in VisualRust.Build\Rules\Debugger.xaml.
	[ExportDebugger(RustDebuggerProvider.Name)]
	[AppliesTo(VisualRustPackage.UniqueCapability)]
	public class RustDebuggerProvider : DebugLaunchProviderBase
	{
		public const string Name = "RustDebugger"; // must match rule name in VisualRust.Build\Rules\Debugger.xaml.

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

		public override Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
		{
			var targets = new List<IDebugLaunchSettings>();

			//var executablePath = ConfiguredProject.UnconfiguredProject.MakeRooted("target\\debug\\....exe");
			//var arguments = ... TODO: read from property
			//var workingDirectory = Path.GetDirectoryName(ConfiguredProject.UnconfiguredProject.FullPath) // TODO: read from property and fall back to project directory if empty

			//var target = new DebugLaunchSettings(launchOptions)
			//{
			//	LaunchOperation = DebugLaunchOperation.CreateProcess,
			//	LaunchDebugEngineGuid = DebuggerEngines.NativeOnlyEngine,
			//	Executable = executablePath,
			//	Arguments = arguments,
			//	CurrentDirectory = workingDirectory
			//};

			//targets.Add(target);

			return Task.FromResult((IReadOnlyList<IDebugLaunchSettings>)targets);
		}
	}
}
