// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Common.Core {
    public static class StreamExtensions {
        public static async Task CopyAndFlushAsync(this Stream source, Stream destination, IProgress<long> progress = null) {
                await source.CopyToAsync(destination, progress);
                await destination.FlushAsync();
        }

        public static async Task CopyToAsync(this Stream source, Stream destination, IProgress<long> progress) {
            byte[] buffer = new byte[81920];
            int bytesRead = 0;
            long bytesTotal = 0;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                await destination.WriteAsync(buffer, 0, bytesRead);
                bytesTotal += bytesRead;
                progress?.Report(bytesTotal);
            }
            await destination.FlushAsync();
        }
    }
}
