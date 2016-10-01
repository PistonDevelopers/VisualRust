// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif
using static System.FormattableString;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public sealed partial class MsBuildFileSystemWatcher {
        private class DirectoryDeleted : IFileSystemChange {
            private readonly MsBuildFileSystemWatcherEntries _entries;
            private readonly string _rootDirectory;
            private readonly string _fullPath;

            public DirectoryDeleted(MsBuildFileSystemWatcherEntries entries, string rootDirectory, string fullPath) {
                _entries = entries;
                _rootDirectory = rootDirectory;
                _fullPath = fullPath;
            }

            public void Apply() {
                if (!_fullPath.StartsWithIgnoreCase(_rootDirectory)) {
                    return;
                }

                var relativePath = PathHelper.MakeRelative(_rootDirectory, _fullPath);
                _entries.DeleteDirectory(relativePath);
            }

            public override string ToString() {
                return Invariant($"Directory deleted: {_fullPath}");
            }
        }
    }
}