// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Common.Core.Logging {
    internal sealed class NullLogWriter : IActionLogWriter {
        public static IActionLogWriter Instance { get; } = new NullLogWriter();

        private NullLogWriter() { }
        public Task WriteAsync(MessageCategory category, string message) => Task.CompletedTask;
        public void Flush() { }
    }
}