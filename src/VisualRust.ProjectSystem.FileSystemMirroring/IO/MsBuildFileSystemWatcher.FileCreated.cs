// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using VisualRust.Core;
using VisualRust.ProjectSystem.FileSystemMirroring.Utilities;
using static System.FormattableString;

namespace VisualRust.ProjectSystem.FileSystemMirroring.IO {
    public sealed partial class MsBuildFileSystemWatcher {
        private class FileCreated : IFileSystemChange {
            private readonly MsBuildFileSystemWatcherEntries _entries;
            private readonly string _rootDirectory;
            private readonly IFileSystem _fileSystem;
            private readonly IMsBuildFileSystemFilter _fileSystemFilter;
            private readonly string _fullPath;

            public FileCreated(MsBuildFileSystemWatcherEntries entries, string rootDirectory, IFileSystem fileSystem, IMsBuildFileSystemFilter fileSystemFilter, string fullPath) {
                _entries = entries;
                _rootDirectory = rootDirectory;
                _fileSystem = fileSystem;
                _fileSystemFilter = fileSystemFilter;
                _fullPath = fullPath;
            }

            public void Apply() {
                string relativePath;
                string shortRelativePath;
                if (IsFileAllowed(_rootDirectory, _fullPath, _fileSystem, _fileSystemFilter, out relativePath, out shortRelativePath)) {
                    _entries.AddFile(relativePath, shortRelativePath);
                }
            }

            public override string ToString() {
                return Invariant($"File created: {_fullPath}");
            }
        }
    }
}