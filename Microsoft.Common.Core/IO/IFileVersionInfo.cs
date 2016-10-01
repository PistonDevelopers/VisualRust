// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.IO {
    public interface IFileVersionInfo {
        int FileMajorPart { get; }
        int FileMinorPart { get; }
    }
}
