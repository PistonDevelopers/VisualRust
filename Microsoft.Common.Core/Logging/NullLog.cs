// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Logging {
    /// <summary>
    /// Logger that does nothing
    /// </summary>
    public sealed class NullLog : IActionLinesLog {
        public static IActionLinesLog Instance { get; } = new NullLog();

        public Task WriteAsync(MessageCategory category, string message) {
            return Task.CompletedTask;
        }

        public Task WriteFormatAsync(MessageCategory category, string format, params object[] arguments) {
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(MessageCategory category, string message) {
            return Task.CompletedTask;
        }

        public string Content => string.Empty;

        public IReadOnlyList<string> Lines => new string[0];
    }
}
