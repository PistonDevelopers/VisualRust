// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Common.Core {
    public static class ArraySegmentExtensions {
        public static async Task<byte[]> ToByteArrayAsync(this ArraySegment<byte> source, int count) {
            byte[] data = null;
            using (MemoryStream ms = new MemoryStream()) {
                await ms.WriteAsync(source.Array, source.Offset, count);
                await ms.FlushAsync();
                data = ms.ToArray();
            }
            return data;
        }

        public static async Task<byte[]> ToByteArrayAsync(this ArraySegment<byte> source) {
            byte[] data = null;
            using (MemoryStream ms = new MemoryStream()) {
                await ms.WriteAsync(source.Array, source.Offset, source.Count);
                await ms.FlushAsync();
                data = ms.ToArray();
            }
            return data;
        }
    }
}
