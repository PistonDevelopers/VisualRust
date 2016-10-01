// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;

namespace Microsoft.Common.Core.IO {
    public interface IFileInfo : IFileSystemInfo {
        IDirectoryInfo Directory { get; }
        StreamWriter CreateText();
    }
}