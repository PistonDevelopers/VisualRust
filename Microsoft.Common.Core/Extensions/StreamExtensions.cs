// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Common.Core {
    public static class StreamExtensions {
        public static async Task CopyAndFlushAsync(this Stream destination, Stream source) {
                await source.CopyToAsync(destination);
                await destination.FlushAsync();
        }
    }
}
