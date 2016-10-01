// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif
using static System.FormattableString;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public sealed partial class MsBuildFileSystemWatcher {
        private class FileRenamed : IFileSystemChange {
            private readonly MsBuildFileSystemWatcherEntries _entries;
            private readonly string _rootDirectory;
            private readonly IFileSystem _fileSystem;
            private readonly IMsBuildFileSystemFilter _fileSystemFilter;
            private readonly string _oldFullPath;
            private readonly string _fullPath;

            public FileRenamed(MsBuildFileSystemWatcherEntries entries, string rootDirectory, IFileSystem fileSystem, IMsBuildFileSystemFilter fileSystemFilter, string oldFullPath, string fullPath) {
                _entries = entries;
                _rootDirectory = rootDirectory;
                _fileSystem = fileSystem;
                _fileSystemFilter = fileSystemFilter;
                _oldFullPath = oldFullPath;
                _fullPath = fullPath;
            }

            public void Apply() {
                string newRelativePath;
                string newShortRelativePath;
                if (!_oldFullPath.StartsWithIgnoreCase(_rootDirectory)) {
                    if (IsFileAllowed(_rootDirectory, _fullPath, _fileSystem, _fileSystemFilter, out newRelativePath, out newShortRelativePath)) {
                        _entries.AddFile(newRelativePath, newShortRelativePath);
                    }

                    return;
                }

                var oldRelativePath = PathHelper.MakeRelative(_rootDirectory, _oldFullPath);
                if (IsFileAllowed(_rootDirectory, _fullPath, _fileSystem, _fileSystemFilter, out newRelativePath, out newShortRelativePath)) {
                    _entries.RenameFile(oldRelativePath, newRelativePath, newShortRelativePath);
                } else {
                    _entries.DeleteFile(oldRelativePath);
                }
            }

            public override string ToString() {
                return Invariant($"File renamed: {_oldFullPath} -> {_fullPath}");
            }
        }
    }
}