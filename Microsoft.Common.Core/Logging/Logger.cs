// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Common.Core.Threading;

namespace Microsoft.Common.Core.Logging {
    /// <summary>
    /// Application event logger
    /// </summary>
    internal sealed class Logger : IActionLog, IDisposable {
        private IActionLogWriter[] _logs;
        private readonly ILoggingPermissions _permissions;
        private readonly string _appName;
        private readonly IActionLogWriter _writer;
        private readonly BinaryAsyncLock _initializationLock = new BinaryAsyncLock();

        public void Dispose() {
            if (_logs != null) {
                foreach (var log in _logs) {
                    (log as IDisposable)?.Dispose();
                }
            }
        }

        internal Logger(string appName, ILoggingPermissions permissions, IActionLogWriter writer) {
            _appName = appName;
            _permissions = permissions;
            _writer = writer;
        }

        private async Task EnsureCreatedAsync() {
            var token = await _initializationLock.WaitAsync();
            try {
                if (!token.IsSet) {
                    // Delay-create log since permission is established when settings are loaded
                    // which may happen after ctor is called.
                    _logs = new IActionLogWriter[Enum.GetValues(typeof(LogVerbosity)).Length];
                    _logs[(int)LogVerbosity.None] = NullLogWriter.Instance;

                    IActionLogWriter mainWriter = NullLogWriter.Instance;
                    if (_permissions.CurrentVerbosity >= LogVerbosity.Minimal) {
                        mainWriter = _writer ?? FileLogWriter.InTempFolder(_appName);
                    }

                    // Unfortunately, creation of event sources in OS logs requires local admin rights.
                    // http://www.christiano.ch/wordpress/2009/12/02/iis7-web-application-writing-to-event-log-generates-security-exception/
                    // So we can't use OS event logs as in Dev15 there is no MSI which could elevate..
                    // _maxLogLevel >= LogLevel.Minimal ? (_writer ?? new ApplicationLogWriter(_appName)) : NullLogWriter.Instance;
                    _logs[(int)LogVerbosity.Minimal] = mainWriter;
                    _logs[(int)LogVerbosity.Normal] = _permissions.CurrentVerbosity >= LogVerbosity.Normal ? mainWriter : NullLogWriter.Instance;

                    if (_permissions.CurrentVerbosity == LogVerbosity.Traffic) {
                        _logs[(int)LogVerbosity.Traffic] = _writer ?? FileLogWriter.InTempFolder(_appName + ".traffic");
                    } else {
                        _logs[(int)LogVerbosity.Traffic] = NullLogWriter.Instance;
                    }
                }
            } finally {
                token.Set();
            }
        }

        #region IActionLog
        public async Task WriteAsync(LogVerbosity verbosity, MessageCategory category, string message) {
            await EnsureCreatedAsync();
            await _logs[(int)verbosity].WriteAsync(category, message);
        }
        public async Task WriteFormatAsync(LogVerbosity verbosity, MessageCategory category, string format, params object[] arguments) {
            await EnsureCreatedAsync();
            string message = string.Format(CultureInfo.InvariantCulture, format, arguments);
            await _logs[(int)verbosity].WriteAsync(category, message);
        }
        public async Task WriteLineAsync(LogVerbosity verbosity, MessageCategory category, string message) {
            await EnsureCreatedAsync();
            await _logs[(int)verbosity].WriteAsync(category, message + Environment.NewLine);
        }

        public void Flush() {
            foreach (var l in _logs) {
                l?.Flush();
            }
        }

        public LogVerbosity LogVerbosity => _permissions.CurrentVerbosity;
        #endregion
    }
}
