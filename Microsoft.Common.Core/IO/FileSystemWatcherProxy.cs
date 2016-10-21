// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;

namespace Microsoft.Common.Core.IO {
    internal sealed class FileSystemWatcherProxy : IFileSystemWatcher {
        private readonly FileSystemWatcher _fileSystemWatcher;

        public FileSystemWatcherProxy(string path, string filter) {
            _fileSystemWatcher = new FileSystemWatcher(path, filter);
        }

        public void Dispose() {
            _fileSystemWatcher.Dispose();
        }

        public bool EnableRaisingEvents {
            get { return _fileSystemWatcher.EnableRaisingEvents; }
            set { _fileSystemWatcher.EnableRaisingEvents = value; }
        }

        public bool IncludeSubdirectories {
            get { return _fileSystemWatcher.IncludeSubdirectories; }
            set { _fileSystemWatcher.IncludeSubdirectories = value; }
        }

        public int InternalBufferSize {
            get { return _fileSystemWatcher.InternalBufferSize; }
            set { _fileSystemWatcher.InternalBufferSize = value; }
        }

        public NotifyFilters NotifyFilter {
            get { return _fileSystemWatcher.NotifyFilter; }
            set { _fileSystemWatcher.NotifyFilter = value; }
        }

        public event FileSystemEventHandler Changed {
            add { _fileSystemWatcher.Changed += value; }
            remove { _fileSystemWatcher.Changed -= value; }
        }

        public event FileSystemEventHandler Created {
            add { _fileSystemWatcher.Created += value; }
            remove { _fileSystemWatcher.Created -= value; }
        }

        public event FileSystemEventHandler Deleted {
            add { _fileSystemWatcher.Deleted += value; }
            remove { _fileSystemWatcher.Deleted -= value; }
        }

        public event RenamedEventHandler Renamed {
            add { _fileSystemWatcher.Renamed += value; }
            remove { _fileSystemWatcher.Renamed -= value; }
        }

        public event ErrorEventHandler Error {
            add { _fileSystemWatcher.Error += value; }
            remove { _fileSystemWatcher.Error -= value; }
        }
    }
}