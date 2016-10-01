// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using static System.FormattableString;

namespace Microsoft.Common.Core.Logging {
    public static class GeneralLog {
        private static FileLogWriter _fileLogWriter;
        public static IActionLogWriter LogWriter {
            get {
                if(_fileLogWriter == null) {
                    _fileLogWriter = FileLogWriter.InTempFolder("Microsoft.R.General");
                }
                return _fileLogWriter;
            }
        }

        public static void Write(string message) {
            LogWriter.WriteAsync(MessageCategory.General, message + Environment.NewLine);
        }

        public static void Write(Exception ex) {
            LogWriter.WriteAsync(MessageCategory.Error, Invariant($"Exception {ex.Message}") + Environment.NewLine);
            LogWriter.WriteAsync(MessageCategory.Error, Invariant($"Stack trace {ex.StackTrace}") + Environment.NewLine);
        }
    }
}
