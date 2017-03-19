// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.ProjectSystem;
using VisualRust.Core;
using VisualRust.ProjectSystem.FileSystemMirroring.IO;
using VisualRust.ProjectSystem.FileSystemMirroring.Logging;
using VisualRust.ProjectSystem.FileSystemMirroring.MsBuild;
using Microsoft.Common.Core;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace VisualRust.ProjectSystem.FileSystemMirroring.Project {
    public class FileSystemMirroringProject : IFileSystemMirroringProjectTemporaryItems {
        private readonly static XProjDocument EmptyProject;

        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly IProjectLockService _projectLockService;
        private readonly MsBuildFileSystemWatcher _fileSystemWatcher;
        private readonly IActionLog _log;
        private readonly CancellationToken _unloadCancellationToken;
        private readonly string _projectDirectory;
        private readonly string _inMemoryImportFullPath;
        private readonly Dictionary<string, ProjectItemElement> _fileItems;
        private readonly Dictionary<string, ProjectItemElement> _directoryItems;
        private readonly IProjectItemDependencyProvider _dependencyProvider;

        private ProjectRootElement _inMemoryImport;
        private ProjectItemGroupElement _filesItemGroup;
        private ProjectItemGroupElement _directoriesItemGroup;
        private ProjectItemGroupElement _temporaryAddedItemGroup;

        static FileSystemMirroringProject() {
            EmptyProject = new XProjDocument(new XProject());
        }

        public FileSystemMirroringProject(UnconfiguredProject unconfiguredProject, IProjectLockService projectLockService, 
                                          MsBuildFileSystemWatcher fileSystemWatcher, IProjectItemDependencyProvider dependencyProvider, 
                                          IActionLog log) {
            _unconfiguredProject = unconfiguredProject;
            _projectLockService = projectLockService;
            _fileSystemWatcher = fileSystemWatcher;
            _dependencyProvider = dependencyProvider;

            _log = log;
            _unloadCancellationToken = _unconfiguredProject.Services.ProjectAsynchronousTasks.UnloadCancellationToken;
            _projectDirectory = _unconfiguredProject.GetProjectDirectory();
            _inMemoryImportFullPath = _unconfiguredProject.GetInMemoryTargetsFileFullPath();
            _fileItems = new Dictionary<string, ProjectItemElement>(StringComparer.OrdinalIgnoreCase);
            _directoryItems = new Dictionary<string, ProjectItemElement>(StringComparer.OrdinalIgnoreCase);

            var changesHandler = new Func<MsBuildFileSystemWatcher.Changeset, Task>(FileSystemChanged);
            _fileSystemWatcher.SourceBlock.LinkTo(new ActionBlock<MsBuildFileSystemWatcher.Changeset>(changesHandler));
        }

        public async Task CreateInMemoryImport() {
            if (_unloadCancellationToken.IsCancellationRequested) {
                return;
            }

            using (var access = await _projectLockService.WriteLockAsync(_unloadCancellationToken)) {
                // A bit odd but we have to "check it out" prior to creating it to avoid some of the validations in chk CPS
                await access.CheckoutAsync(_inMemoryImportFullPath);

                // Now either open or create the in-memory file. Normally Create will happen, but in
                // a scenario where your project had previously failed to load for some reason, need to TryOpen
                // to avoid a new reason for a project load failure
                _inMemoryImport = ProjectRootElement.TryOpen(_inMemoryImportFullPath, access.ProjectCollection);
                if (_inMemoryImport != null) {
                    // The file already exists. Scrape it out so we don’t add duplicate items.
                    _inMemoryImport.RemoveAllChildren();
                } else {
                    // The project didn’t already exist, so create it, and then mark the evaluated project dirty
                    // so that MSBuild will notice. This step isn’t necessary if the project was already in memory.
                    _inMemoryImport = CreateEmptyMsBuildProject(_inMemoryImportFullPath, access.ProjectCollection);

                    // Note that we actually need to mark every project evaluation dirty that is already loaded.
                    await ReevaluateLoadedConfiguredProjects(_unloadCancellationToken, access);
                }

                _filesItemGroup = _inMemoryImport.AddItemGroup();
                _directoriesItemGroup = _inMemoryImport.AddItemGroup();
                _temporaryAddedItemGroup = _inMemoryImport.AddItemGroup();
            }
        }

        public Task<IReadOnlyCollection<string>> AddTemporaryFiles(ConfiguredProject configuredProject, IEnumerable<string> filesToAdd) {
            return AddTemporaryItems(configuredProject, "Content", filesToAdd);
        }

        public Task<IReadOnlyCollection<string>> AddTemporaryDirectories(ConfiguredProject configuredProject, IEnumerable<string> directoriesToAdd) {
            return AddTemporaryItems(configuredProject, "Folder", directoriesToAdd);
        }

        private async Task<IReadOnlyCollection<string>> AddTemporaryItems(ConfiguredProject configuredProject, string itemType, IEnumerable<string> itemPathsToAdd) {
            var unhandled = new List<string>();
            var relativePathToAdd = new List<string>();

            foreach (var path in itemPathsToAdd) {
                if (PathHelper.IsOutsideProjectDirectory(_projectDirectory, _unconfiguredProject.MakeRooted(path))) {
                    unhandled.Add(path);
                } else {
                    relativePathToAdd.Add(_unconfiguredProject.MakeRelative(path));
                }
            }

            if (relativePathToAdd.Count == 0) {
                return unhandled.AsReadOnly();
            }

            using (var access = await _projectLockService.WriteLockAsync(_unloadCancellationToken)) {
                await access.CheckoutAsync(_inMemoryImportFullPath);

                foreach (var path in relativePathToAdd) {
                    _temporaryAddedItemGroup.AddItem(itemType, path, Enumerable.Empty<KeyValuePair<string, string>>());
                }

                var project = await access.GetProjectAsync(configuredProject, _unloadCancellationToken);
                project.ReevaluateIfNecessary();
            }

            return unhandled.AsReadOnly();
        }

        private async Task ReevaluateLoadedConfiguredProjects(CancellationToken cancellationToken, ProjectWriteLockReleaser access) {
            foreach (var configuredProject in _unconfiguredProject.LoadedConfiguredProjects) {
                try {
                    var jsproj = await access.GetProjectAsync(configuredProject, cancellationToken);
                    jsproj.ReevaluateIfNecessary();
                } catch (Exception ex) {
                    System.Diagnostics.Debug.Fail("We were unable to mark a configuration as dirty" + ex.Message, ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Helper used to create the empty project file.
        /// Note that we need to set the IsExplicitlyLoaded property on the ProjectRootElement to true to make sure
        /// it is not cleared from the ProjectRootElementCache. Unfortuantely, the constructure which creates a new
        /// empty project does not set this flag. However, the one which can be created from an XmlReader does. So we use
        /// that one to create the project file in memory and then set the path to make sure it is added correctly to the cache.
        /// </summary>
        private ProjectRootElement CreateEmptyMsBuildProject(string projectFilePath, ProjectCollection collection) {
            using (XmlReader reader = EmptyProject.CreateReader()) {
#if VS14
                ProjectRootElement importFile = ProjectRootElement.Create(reader, collection);
#else
                ProjectRootElement importFile = ProjectRootElement.Create(reader, collection, preserveFormatting: false);
#endif
                importFile.FullPath = projectFilePath;
                return importFile;
            }
        }

        private async Task FileSystemChanged(MsBuildFileSystemWatcher.Changeset changeset) {
            _log.ApplyProjectChangesStarted();

            if (_unloadCancellationToken.IsCancellationRequested) {
                return;
            }

            try {
                using (var access = await _projectLockService.WriteLockAsync(_unloadCancellationToken)) {
                    await access.CheckoutAsync(_inMemoryImportFullPath);

                    _temporaryAddedItemGroup.RemoveAllChildren();

                    await RemoveFiles(changeset.RemovedFiles, access);
                    await RemoveDirectories(changeset.RemovedDirectories, access);

                    await RenameFiles(changeset.RenamedFiles, access);
                    await RenameDirectories(changeset.RenamedDirectories, access);

                    AddDirectories(changeset.AddedDirectories);
                    AddFiles(changeset.AddedFiles);

                    _log.MsBuildAfterChangesApplied(_inMemoryImport);

                    foreach (var configuredProject in _unconfiguredProject.LoadedConfiguredProjects) {
                        try {
                            var project =
                                await access.GetProjectAsync(configuredProject, _unloadCancellationToken);
                            project.ReevaluateIfNecessary();
                        } catch (Exception ex) {
                            Trace.Fail("Unable to mark a configuration as dirty" + ex.Message, ex.StackTrace);
                        }
                    }
                }
            } catch (Exception ex) {
                Trace.Fail("Unable to handle file system change:" + ex.Message, ex.StackTrace);
            }

            _log.ApplyProjectChangesFinished();
        }

        private Task RemoveFiles(HashSet<string> filesToRemove, ProjectWriteLockReleaser access) {
            return RemoveItems(_filesItemGroup, _fileItems, filesToRemove, access);
        }

        private async Task RemoveDirectories(IReadOnlyCollection<string> directoriesToRemove, ProjectWriteLockReleaser access) {
            foreach (var directoryName in directoriesToRemove) {
                await RemoveItems(_filesItemGroup, _fileItems, directoryName, access);
                await RemoveItems(_directoriesItemGroup, _directoryItems, directoryName, access);
            }
        }

        private Task RemoveItems(ProjectItemGroupElement parent, Dictionary<string, ProjectItemElement> items, string directoryName, ProjectWriteLockReleaser access) {
            return RemoveItems(parent, items, items.Keys.Where(f => f.StartsWithIgnoreCase(directoryName)).ToList(), access);
        }

        private async Task RemoveItems(ProjectItemGroupElement parent, Dictionary<string, ProjectItemElement> items, IReadOnlyCollection<string> itemsToRemove, ProjectWriteLockReleaser access) {
            await access.CheckoutAsync(itemsToRemove);
            foreach (var path in itemsToRemove) {
                RemoveItem(parent, items, path);
            }
        }

        private void RemoveItem(ProjectItemGroupElement parent, Dictionary<string, ProjectItemElement> items, string path) {
            ProjectItemElement item;
            if (!items.TryGetValue(path, out item)) {
                return;
            }

            parent.RemoveChild(item);
            items.Remove(path);
        }

        private Task RenameFiles(IReadOnlyDictionary<string, string> filesToRename, ProjectWriteLockReleaser access) {
            return RenameItems(_fileItems, filesToRename, access);
        }

        private async Task RenameDirectories(IReadOnlyDictionary<string, string> directoriesToRename, ProjectWriteLockReleaser access) {
            foreach (var kvp in directoriesToRename) {
                await RenameItems(_fileItems, kvp.Key, kvp.Value, access);
                await RenameItems(_directoryItems, kvp.Key, kvp.Value, access);
            }
        }

        private Task RenameItems(Dictionary<string, ProjectItemElement> items, string oldDirectoryName, string newDirectoryName, ProjectWriteLockReleaser access) {
            var itemsToRename = items.Keys
                .Where(f => f.StartsWithIgnoreCase(oldDirectoryName))
                .ToDictionary(f => f, f => newDirectoryName + f.Substring(oldDirectoryName.Length));

            return RenameItems(items, itemsToRename, access);
        }

        private async Task RenameItems(Dictionary<string, ProjectItemElement> items, IReadOnlyDictionary<string, string> itemsToRename, ProjectWriteLockReleaser access) {
            await access.CheckoutAsync(itemsToRename.Keys);
            foreach (var kvp in itemsToRename) {
                ProjectItemElement item;
                if (items.TryGetValue(kvp.Key, out item)) {
                    items.Remove(kvp.Key);
                    item.Include = kvp.Value;
                    items[kvp.Value] = item;
                }
            }
        }

        private void AddDirectories(IReadOnlyCollection<string> directoriesToAdd) {
            foreach (string path in directoriesToAdd) {
                RemoveItem(_directoriesItemGroup, _directoryItems, path);
                ProjectItemElement item = _directoriesItemGroup.AddItem("Folder", path, Enumerable.Empty<KeyValuePair<string, string>>());
                _directoryItems.Add(path, item);
            }
        }

        private void AddFiles(IReadOnlyCollection<string> filesToAdd) {
            // await InMemoryProjectSourceItemProviderExtension.CallListeners(this.SourceItemsAddingListeners, contexts, false);

            foreach (string path in filesToAdd) {
                RemoveItem(_filesItemGroup, _fileItems, path);

                var metadata = Enumerable.Empty<KeyValuePair<string, string>>();
                // TODO: consider getting this via a provider
                var masterFilePath = _dependencyProvider?.GetMasterFile(path);
                if (!string.IsNullOrEmpty(masterFilePath)) {
                    var dict = new Dictionary<string, string>();
                    dict["DependentUpon"] = masterFilePath;
                    metadata = dict;
                }
                var item = _filesItemGroup.AddItem("Content", path, metadata);
                _fileItems.Add(path, item);
            }

            // await InMemoryProjectSourceItemProviderExtension.CallListeners(this.SourceItemsAddedListeners, contexts, false);
        }
    }

    /// <summary>
    /// When files or folders are added from VS, CPS requires new items to be added to project at an early stage
    /// But since for the FSMP the only source of truth is file system, these items removed from project during next update
    /// (which may restore them as added from file system)
    /// </summary>
    public interface IFileSystemMirroringProjectTemporaryItems {
        Task<IReadOnlyCollection<string>> AddTemporaryFiles(ConfiguredProject configuredProject, IEnumerable<string> filesToAdd);
        Task<IReadOnlyCollection<string>> AddTemporaryDirectories(ConfiguredProject configuredProject, IEnumerable<string> directoriesToAdd);
    }
}
