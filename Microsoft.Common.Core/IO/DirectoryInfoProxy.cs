// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Common.Core.IO {
    internal sealed class DirectoryInfoProxy : IDirectoryInfo {
        private readonly DirectoryInfo _directoryInfo;

        public DirectoryInfoProxy(string directoryPath) {
            _directoryInfo = new DirectoryInfo(directoryPath);
        }

        public DirectoryInfoProxy(DirectoryInfo directoryInfo) {
            _directoryInfo = directoryInfo;
        }

        public bool Exists => _directoryInfo.Exists;
        public string FullName => _directoryInfo.FullName;
        public FileAttributes Attributes => _directoryInfo.Attributes;
        public IDirectoryInfo Parent => _directoryInfo.Parent != null ? new DirectoryInfoProxy(_directoryInfo.Parent) : null;

        public void Delete() {
            _directoryInfo.Delete();
        }

        public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos() {
            return _directoryInfo
                .EnumerateFileSystemInfos()
                .Select(CreateFileSystemInfoProxy);
        }

        private static IFileSystemInfo CreateFileSystemInfoProxy(FileSystemInfo fileSystemInfo) {
            var directoryInfo = fileSystemInfo as DirectoryInfo;
            return directoryInfo != null ? (IFileSystemInfo)new DirectoryInfoProxy(directoryInfo) : new FileInfoProxy((FileInfo)fileSystemInfo);
        }
    }
}