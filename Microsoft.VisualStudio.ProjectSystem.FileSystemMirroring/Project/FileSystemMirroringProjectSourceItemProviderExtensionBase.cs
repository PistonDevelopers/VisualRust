// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.Threading;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Items;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif
using ItemData = System.Tuple<string, string, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>>;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project {
    public abstract class FileSystemMirroringProjectSourceItemProviderExtensionBase 
        : IProjectSourceItemProviderExtension, IProjectFolderItemProviderExtension {

        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly ConfiguredProject _configuredProject;
        private readonly IProjectLockService _projectLockService;
        private readonly IFileSystemMirroringProjectTemporaryItems _temporaryItems;

        protected FileSystemMirroringProjectSourceItemProviderExtensionBase(UnconfiguredProject unconfiguredProject, ConfiguredProject configuredProject, IProjectLockService projectLockService, IFileSystemMirroringProjectTemporaryItems temporaryItems) {
            _unconfiguredProject = unconfiguredProject;
            _configuredProject = configuredProject;
            _projectLockService = projectLockService;
            _temporaryItems = temporaryItems;
        }

#region IProjectSourceItemProviderExtension implementation

        public Task<bool> CheckSourceItemOwnershipAsync(string itemType, string evaluatedInclude) {
            return this.CheckFolderItemOwnershipAsync(evaluatedInclude);
        }

        public Task<bool> CheckProjectFileOwnershipAsync(string projectFilePath) {
            return this.CheckProjectFileOwnership(projectFilePath)
                ? TplExtensions.TrueTask
                : TplExtensions.FalseTask;
        }

        public async Task<IReadOnlyCollection<ItemData>> AddOwnedSourceItemsAsync(IReadOnlyCollection<ItemData> items) {
            var unhandledItems = await _temporaryItems.AddTemporaryFiles(_configuredProject, items.Select(i => i.Item2));
            return items.Where(i => unhandledItems.Contains(i.Item2)).ToImmutableArray();
        }

        public async Task<bool> TryAddSourceItemsToOwnedProjectFileAsync(IReadOnlyCollection<ItemData> items, string projectFilePath) {
            if (!CheckProjectFileOwnership(projectFilePath)) {
                return false;
            }

            var unhandledItems = await _temporaryItems.AddTemporaryFiles(_configuredProject, items.Select(i => i.Item2));
            return unhandledItems.Count < items.Count;
        }

        public Task<IReadOnlyCollection<IProjectSourceItem>> RemoveOwnedSourceItemsAsync(
            IReadOnlyCollection<IProjectSourceItem> projectItems, DeleteOptions deleteOptions) {
            var projectDirectory = _unconfiguredProject.GetProjectDirectory();
            List<IProjectSourceItem> itemsInProjectFolder = projectItems
                .Where(item => !PathHelper.IsOutsideProjectDirectory(projectDirectory, item.EvaluatedIncludeAsFullPath))
                .ToList();

            return
                Task.FromResult(itemsInProjectFolder.Count == 0
                    ? projectItems
                    : projectItems.Except(itemsInProjectFolder).ToImmutableArray());
        }

        public Task<ProjectItem> RenameOwnedSourceItemAsync(IProjectItem projectItem, string newValue) {
            return GetMsBuildItemByProjectItem(projectItem);
        }

        public Task<ProjectItem> SetItemTypeOfOwnedSourceItemAsync(IProjectItem projectItem, string newItemType) {
            return GetMsBuildItemByProjectItem(projectItem);
        }

#endregion

#region IProjectFolderItemProviderExtension implementation

        public Task<bool> CheckFolderItemOwnershipAsync(string evaluatedInclude) {
            return _unconfiguredProject.IsOutsideProjectDirectory(_unconfiguredProject.MakeRooted(evaluatedInclude))
                ? TplExtensions.FalseTask
                : TplExtensions.TrueTask;
        }

        public async Task<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>>> AddOwnedFolderItemsAsync(IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>> items) {
            var unhandledItems = await _temporaryItems.AddTemporaryDirectories(_configuredProject, items.Keys);
            return items.Where(i => unhandledItems.Contains(i.Key)).ToImmutableDictionary();
        }

        public Task<IReadOnlyCollection<IProjectItem>> RemoveOwnedFolderItemsAsync(
            IReadOnlyCollection<IProjectItem> projectItems, DeleteOptions deleteOptions) {
            List<IProjectItem> itemsInProjectFolder = projectItems
                .Where(item => !_unconfiguredProject.IsOutsideProjectDirectory(item.EvaluatedIncludeAsFullPath))
                .ToList();

            return
                Task.FromResult(itemsInProjectFolder.Count == 0
                    ? projectItems
                    : projectItems.Except(itemsInProjectFolder).ToImmutableArray());
        }

        public Task<ProjectItem> RenameOwnedFolderItemAsync(IProjectItem projectItem, string newValue) {
            return GetMsBuildItemByProjectItem(projectItem);
        }

#endregion

        private bool CheckProjectFileOwnership(string projectFilePath) {
            return _unconfiguredProject.GetInMemoryTargetsFileFullPath().EqualsIgnoreCase(projectFilePath);
        }

        private async Task<ProjectItem> GetMsBuildItemByProjectItem(IProjectItem projectItem) {
            using (var access = await _projectLockService.ReadLockAsync()) {
                var project = await access.GetProjectAsync(_configuredProject);
                var item =  project.GetItemsByEvaluatedInclude(projectItem.EvaluatedInclude)
                    .FirstOrDefault(pi => StringComparer.OrdinalIgnoreCase.Equals(pi.ItemType, projectItem.ItemType));
                return item;
            }
        }
    }
}
