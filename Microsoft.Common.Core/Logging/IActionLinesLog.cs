// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Common.Core.Logging {
    /// <summary>
    /// Log that can be read as text lines
    /// </summary>
    public interface IActionLinesLog : IActionLog {
        IReadOnlyList<string> Lines { get; }
    }
}
