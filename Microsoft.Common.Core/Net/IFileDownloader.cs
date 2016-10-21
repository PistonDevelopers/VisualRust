// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;

namespace Microsoft.Common.Core.Net {
    public interface IFileDownloader {
        string Download(string url, string dstPath, CancellationToken ct);
    }
}
