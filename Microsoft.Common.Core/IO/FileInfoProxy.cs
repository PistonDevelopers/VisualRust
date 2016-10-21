// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;

namespace Microsoft.Common.Core.IO {
    internal sealed class FileInfoProxy : IFileInfo {
        private readonly FileInfo _fileInfo;

        public FileInfoProxy(FileInfo fileInfo) {
            _fileInfo = fileInfo;
        }

        public bool Exists => _fileInfo.Exists;
        public string FullName => _fileInfo.FullName;
        public FileAttributes Attributes => _fileInfo.Attributes;
        public IDirectoryInfo Directory => _fileInfo.Directory != null ? new DirectoryInfoProxy(_fileInfo.Directory) : null;

        public StreamWriter CreateText() {
            return _fileInfo.CreateText();
        }

        public void Delete() {
            _fileInfo.Delete();
        }
    }
}