// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.IO {
    internal class FileVersionInfo : IFileVersionInfo {
        public FileVersionInfo(int major, int minor) {
            FileMajorPart = major;
            FileMinorPart = minor;
        }
        public int FileMajorPart { get; }

        public int FileMinorPart { get; }
    }
}
