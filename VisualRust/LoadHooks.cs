// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
/*using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings.Definitions;*/
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
/*using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.SurveyNews;
using Microsoft.VisualStudio.R.Packages.R;*/
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Common.Core.Logging;
//#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using IThreadHandling = Microsoft.VisualStudio.ProjectSystem.IThreadHandling;
using Microsoft.VisualStudio;
//#endif
#if VS15
using Microsoft.VisualStudio.ProjectSystem.VS;
using IThreadHandling = Microsoft.VisualStudio.ProjectSystem.IProjectThreadingService;
#endif


namespace VisualRust
{
	internal sealed class LoadHooks
	{
		//private const string DefaultRDataName = ".RData";
		//private const string DefaultRHistoryName = ".RHistory";

		[Export(typeof(IFileSystemMirroringProjectTemporaryItems))]
		private FileSystemMirroringProject Project { get; }

		private readonly MsBuildFileSystemWatcher _fileWatcher;
		private readonly string _projectDirectory;
		//private readonly IRToolsSettings _toolsSettings;
		private readonly IFileSystem _fileSystem = new FileSystem();
		private readonly IThreadHandling _threadHandling;
		private readonly UnconfiguredProject _unconfiguredProject;
		private readonly IEnumerable<Lazy<IVsProject>> _cpsIVsProjects;
		//private readonly IRInteractiveWorkflowProvider _workflowProvider;
		//private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;
		private readonly IProjectItemDependencyProvider _dependencyProvider;

		//private IRInteractiveWorkflow _workflow;
		//private IRSession _session;
		//private IRHistory _history;
		//private ISurveyNewsService _surveyNews;

		/// <summary>
		/// Backing field for the similarly named property.
		/// </summary>
		[ImportingConstructor]
		public LoadHooks(UnconfiguredProject unconfiguredProject
			, [ImportMany("Microsoft.VisualStudio.ProjectSystem.Microsoft.VisualStudio.Shell.Interop.IVsProject")] IEnumerable<Lazy<IVsProject>> cpsIVsProjects
			, IProjectLockService projectLockService
			//, IRInteractiveWorkflowProvider workflowProvider
			//, IInteractiveWindowComponentContainerFactory componentContainerFactory
			//, IRToolsSettings toolsSettings
			, IThreadHandling threadHandling
			//, ISurveyNewsService surveyNews
			, [Import(AllowDefault = true)] IProjectItemDependencyProvider dependencyProvider)
		{

			_unconfiguredProject = unconfiguredProject;
			_cpsIVsProjects = cpsIVsProjects;
			//_workflowProvider = workflowProvider;
			//_componentContainerFactory = componentContainerFactory;

			//_toolsSettings = toolsSettings;
			_threadHandling = threadHandling;
			//_surveyNews = surveyNews;
			_dependencyProvider = dependencyProvider;

			_projectDirectory = unconfiguredProject.GetProjectDirectory();

			unconfiguredProject.ProjectUnloading += ProjectUnloading;
			_fileWatcher = new MsBuildFileSystemWatcher(_projectDirectory, "*", 25, 1000, _fileSystem, new MsBuildFileSystemFilter());
			_fileWatcher.Error += FileWatcherError;
			Project = new FileSystemMirroringProject(unconfiguredProject, projectLockService, _fileWatcher, _dependencyProvider);
		}

		public static IVsPackage EnsurePackageLoaded(Guid guidPackage)
		{
			var shell = (IVsShell)VisualRustPackage.GetGlobalService(typeof(IVsShell));
			var guid = guidPackage;
			IVsPackage package;
			int hr = ErrorHandler.ThrowOnFailure(shell.IsPackageLoaded(ref guid, out package), VSConstants.E_FAIL);
			guid = guidPackage;
			if (hr != VSConstants.S_OK)
			{
				ErrorHandler.ThrowOnFailure(shell.LoadPackage(ref guid, out package), VSConstants.E_FAIL);
			}

			return package;
		}

		[AppliesTo(VisualRustPackage.UniqueCapability)]
//#if VS14
		[UnconfiguredProjectAutoLoad2(completeBy: UnconfiguredProjectLoadCheckpoint.CapabilitiesEstablished)]
//#else
//		[ProjectAutoLoad(startAfter: ProjectLoadCheckpoint.UnconfiguredProjectLocalCapabilitiesEstablished,
//						 completeBy: ProjectLoadCheckpoint.BeforeLoadInitialConfiguration,
//						 RequiresUIThread = false)]
//#endif
		public async Task InitializeProjectFromDiskAsync()
		{
			await Project.CreateInMemoryImport();
			_fileWatcher.Start();

			await _threadHandling.SwitchToUIThread();
			// Make sure package is loaded
			EnsurePackageLoaded(GuidList.VisualRustPkgGuid);

			// Verify project is not on a network share and give warning if it is
			//CheckRemoteDrive(_projectDirectory);

			//_workflow = _workflowProvider.GetOrCreate();
			//_session = _workflow.RSession;
			//_history = _workflow.History;

			//if (_workflow.ActiveWindow == null)
			//{
			//	var window = await _workflow.GetOrCreateVisualComponent(_componentContainerFactory);
			//	window.Container.Show(focus: true, immediate: false);
			//}

			//try
			//{
			//	await _session.HostStarted;
			//}
			//catch (Exception)
			//{
			//	return;
			//}

			// TODO: need to compute the proper paths for remote, but they might not even exist if the project hasn't been deployed.
			// https://github.com/Microsoft/RTVS/issues/2223
			//if (!_session.IsRemote)
			//{
			//	var rdataPath = Path.Combine(_projectDirectory, DefaultRDataName);
			//	bool loadDefaultWorkspace = _fileSystem.FileExists(rdataPath) && await GetLoadDefaultWorkspace(rdataPath);
			//	using (var evaluation = await _session.BeginEvaluationAsync())
			//	{
			//		if (loadDefaultWorkspace)
			//		{
			//			await evaluation.LoadWorkspaceAsync(rdataPath);
			//		}

			//		await evaluation.SetWorkingDirectoryAsync(_projectDirectory);
			//	}

			//	_toolsSettings.WorkingDirectory = _projectDirectory;
			//}

			//_history.TryLoadFromFile(Path.Combine(_projectDirectory, DefaultRHistoryName));

			//CheckSurveyNews();
		}

		//private async void CheckSurveyNews()
		//{
		//	// Don't return a task, the caller doesn't want to await on this
		//	// and hold up loading of the project.
		//	// We do it this way instead of calling DoNotWait extension in order
		//	// to handle any non critical exceptions.
		//	try
		//	{
		//		await _surveyNews.CheckSurveyNewsAsync(false);
		//	}
		//	catch (Exception ex) when (!ex.IsCriticalException())
		//	{
		//		GeneralLog.Write(ex);
		//	}
		//}

		private void FileWatcherError(object sender, EventArgs args)
		{
			_fileWatcher.Error -= FileWatcherError;
			//VsAppShell.Current.DispatchOnUIThread(() => {
			//	foreach (var iVsProjectLazy in _cpsIVsProjects)
			//	{
			//		IVsProject iVsProject;
			//		try
			//		{
			//			iVsProject = iVsProjectLazy.Value;
			//		}
			//		catch (Exception)
			//		{
			//			continue;
			//		}

			//		if (iVsProject.AsUnconfiguredProject() != _unconfiguredProject)
			//		{
			//			continue;
			//		}

			//		var solution = VsAppShell.Current.GetGlobalService<IVsSolution>(typeof(SVsSolution));
			//		solution.CloseSolutionElement((uint)__VSSLNCLOSEOPTIONS.SLNCLOSEOPT_UnloadProject, (IVsHierarchy)iVsProject, 0);
			//		return;
			//	}
			//});
		}

		private async Task ProjectUnloading(object sender, EventArgs args)
		{
			//VsAppShell.Current.AssertIsOnMainThread();

			_unconfiguredProject.ProjectUnloading -= ProjectUnloading;
			_fileWatcher.Dispose();

			if (!_fileSystem.DirectoryExists(_projectDirectory))
			{
				return;
			}

			//if (_toolsSettings.AlwaysSaveHistory)
			//{
			//	_history.TrySaveToFile(Path.Combine(_projectDirectory, DefaultRHistoryName));
			//}

			//var rdataPath = Path.Combine(_projectDirectory, DefaultRDataName);
			//var saveDefaultWorkspace = await GetSaveDefaultWorkspace(rdataPath);
			//if (!_session.IsHostRunning)
			//{
			//	return;
			//}

			//Task.Run(async () => {
			//	using (var evaluation = await _session.BeginEvaluationAsync())
			//	{
			//		if (saveDefaultWorkspace)
			//		{
			//			await evaluation.SaveWorkspaceAsync(rdataPath);
			//		}
			//		await evaluation.SetDefaultWorkingDirectoryAsync();
			//	}
			//}).SilenceException<RException>().DoNotWait();
		}

		//private async Task<bool> GetLoadDefaultWorkspace(string rdataPath)
		//{
		//	switch (_toolsSettings.LoadRDataOnProjectLoad)
		//	{
		//		case YesNoAsk.Yes:
		//			return true;
		//		case YesNoAsk.Ask:
		//			await _threadHandling.SwitchToUIThread();
		//			return VsAppShell.Current.ShowMessage(
		//				string.Format(CultureInfo.CurrentCulture, Resources.LoadWorkspaceIntoGlobalEnvironment, rdataPath),
		//				MessageButtons.YesNo) == MessageButtons.Yes;
		//		default:
		//			return false;
		//	}
		//}

		//private async Task<bool> GetSaveDefaultWorkspace(string rdataPath)
		//{
		//	switch (_toolsSettings.SaveRDataOnProjectUnload)
		//	{
		//		case YesNoAsk.Yes:
		//			return true;
		//		case YesNoAsk.Ask:
		//			await _threadHandling.SwitchToUIThread();
		//			return VsAppShell.Current.ShowMessage(
		//				string.Format(CultureInfo.CurrentCulture, Resources.SaveWorkspaceOnProjectUnload, rdataPath),
		//				MessageButtons.YesNo) == MessageButtons.Yes;
		//		default:
		//			return false;
		//	}
		//}

		//private void CheckRemoteDrive(string path)
		//{
		//	bool remoteDrive = NativeMethods.PathIsUNC(path);
		//	if (!remoteDrive)
		//	{
		//		var pathRoot = Path.GetPathRoot(path);
		//		var driveType = (NativeMethods.DriveType)NativeMethods.GetDriveType(pathRoot);
		//		remoteDrive = driveType == NativeMethods.DriveType.Remote;
		//	}
		//	if (remoteDrive)
		//	{
		//		VsAppShell.Current.ShowMessage(Resources.Warning_UncPath, MessageButtons.OK);
		//	}
		//}
	}
}