// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static System.FormattableString;

namespace Microsoft.Common.Core.Logging {
    public sealed class FileLogWriter : IActionLogWriter {
        private const int _maxTimeout = 5000;
        private const int _maxBufferSize = 1024;
        private static readonly ConcurrentDictionary<string, FileLogWriter> _writers = new ConcurrentDictionary<string, FileLogWriter>();
        private readonly char[] _lineBreaks = { '\n' };
        private readonly string _filePath;
        private readonly ActionBlock<string> _messages;
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private FileLogWriter(string filePath) {
            _filePath = filePath;
            _messages = new ActionBlock<string>(new Func<string, Task>(WriteToFile));

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private async Task WriteToFile(string message) {
            try {
                await WriteBuffer(message, flush: false);
            } catch (UnauthorizedAccessException ex) {
                Trace.Fail(ex.ToString());
            } catch (PathTooLongException ex) {
                Trace.Fail(ex.ToString());
            } catch (DirectoryNotFoundException ex) {
                Trace.Fail(ex.ToString());
            } catch (NotSupportedException ex) {
                Trace.Fail(ex.ToString());
            } catch (IOException ex) {
                Trace.Fail(ex.ToString());
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            WriteBuffer(string.Empty, flush: true).Wait(_maxTimeout);
        }

        private void OnProcessExit(object sender, EventArgs e) {
            WriteBuffer(string.Empty, flush: true).Wait(_maxTimeout);
        }

        private async Task WriteBuffer(string message, bool flush) {
            await _semaphore.WaitAsync();
            try {
                _sb.Append(message);
                if (_sb.Length > _maxBufferSize || flush) {
                    using (var stream = File.AppendText(_filePath)) {
                        await stream.WriteAsync(_sb.ToString());
                    }
                    _sb.Clear();
                }
            } finally {
                _semaphore.Release();
            }
        }

        public Task WriteAsync(MessageCategory category, string message) {
            return _messages.SendAsync(GetStringToWrite(category, message));
        }

        public void Flush() {
            WriteBuffer(string.Empty, flush: true).Wait(_maxTimeout);
        }


        private string GetStringToWrite(MessageCategory category, string message) {
            var categoryString = GetCategoryString(category);
            var prefix = Invariant($"[{DateTime.Now:yy-M-dd_HH-mm-ss}]{categoryString}:");
            if (!message.Take(message.Length - 1).Contains('\n')) {
                return prefix + message;
            }

            var emptyPrefix = new string(' ', prefix.Length);
            var lines = message.Split(_lineBreaks, StringSplitOptions.RemoveEmptyEntries)
                .Select((line, i) => i == 0 ? prefix + line + "\n" : emptyPrefix + line + "\n");
            return string.Concat(lines);
        }

        public static FileLogWriter InTempFolder(string fileName) {
            return _writers.GetOrAdd(fileName, _ => {
                var path = Path.Combine(Path.GetTempPath(), Invariant($@"{fileName}_{DateTime.Now:yyyyMdd_HHmmss}_pid{Process.GetCurrentProcess().Id}.log"));
                return new FileLogWriter(path);
            });
        }

        private static string GetCategoryString(MessageCategory category) {
            switch (category) {
                case MessageCategory.Error:
                    return "[ERROR]";
                case MessageCategory.Warning:
                    return "[WARNING]";
                default:
                    return string.Empty;
            }
        }
    }
}