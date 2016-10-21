// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Common.Core.IO {
    public interface IDirectoryInfo : IFileSystemInfo {
        IDirectoryInfo Parent { get; }
        IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos();
    }
}