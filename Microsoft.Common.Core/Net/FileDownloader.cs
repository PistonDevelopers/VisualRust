// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Threading;

namespace Microsoft.Common.Core.Net {
    public sealed class FileDownloader : IFileDownloader {
        public string Download(string url, string dstPath, CancellationToken ct) {
            try {
                using (var client = new WebClient()) {
                    client.DownloadFileAsync(new Uri(url, UriKind.Absolute), dstPath);
                    while (client.IsBusy && !ct.IsCancellationRequested) {
                        Thread.Sleep(200);
                    }
                }
            } catch (WebException ex) {
                return ex.Message;
            }
            return null;
        }
    }
}
