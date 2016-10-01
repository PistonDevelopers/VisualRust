// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public interface IMsBuildFileSystemFilter {
        bool IsFileAllowed(string relativePath, FileAttributes attributes);
        bool IsDirectoryAllowed(string relativePath, FileAttributes attributes);
        void Seal();
    }
}