// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Logging {
    /// <summary>
    /// Log that writes data into a string.
    /// </summary>
    public class StringLog : IActionLog {
        private readonly IActionLogWriter _logWriter;
        private readonly StringBuilder _sb = new StringBuilder();

        public StringLog(IActionLogWriter logWriter) {
            _logWriter = logWriter ?? NullLogWriter.Instance;
        }

        public Task WriteAsync(MessageCategory category, string message) {
            _sb.Append(message);
            return _logWriter.WriteAsync(category, message);
        }

        public Task WriteFormatAsync(MessageCategory category, string format, params object[] arguments) {
            string message = string.Format(CultureInfo.InvariantCulture, format, arguments);
            return WriteAsync(category, message);
        }

        public Task WriteLineAsync(MessageCategory category, string message) {
            return WriteAsync(category, message + "\r\n");
        }

        public virtual string Content => _sb.ToString();
    }
}
