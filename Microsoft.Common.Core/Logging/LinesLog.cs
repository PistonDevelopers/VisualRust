// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Common.Core.Logging {
    /// <summary>
    /// Implementation of a text log that has multiple text lines.
    /// </summary>
    public class LinesLog : StringLog, IActionLinesLog {
        private readonly char[] _lineBreaks = { '\n' };
        private List<string> _lines;

        public IReadOnlyList<string> Lines {
            get {
                if (_lines == null) {
                    _lines = new List<string>();
                    _lines.AddRange(Content.Replace("\r", string.Empty).Split(_lineBreaks));
                }

                return _lines;
            }
        }

        public LinesLog(IActionLogWriter logWriter) : base(logWriter) { }
    }
}
