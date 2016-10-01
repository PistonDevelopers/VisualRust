// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public sealed partial class MsBuildFileSystemWatcher {
        private class AttributesChanged : IFileSystemChange {
            private readonly MsBuildFileSystemWatcherEntries _entries;
            private readonly string _name;
            private readonly string _fullPath;

            public AttributesChanged(MsBuildFileSystemWatcherEntries entries, string name, string fullPath) {
                _entries = entries;
                _name = name;
                _fullPath = fullPath;
            }

            public void Apply() { }
        }
    }
}