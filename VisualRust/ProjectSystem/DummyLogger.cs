using Microsoft.Common.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.ProjectSystem
{
    internal class DummyLogger : IActionLog
    {
        public LogVerbosity LogVerbosity => LogVerbosity.None;

        public void Flush()
        {
        }

        public Task WriteAsync(LogVerbosity verbosity, MessageCategory category, string message)
        {
            return Task.CompletedTask;
        }

        public Task WriteFormatAsync(LogVerbosity verbosity, MessageCategory category, string format, params object[] arguments)
        {
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(LogVerbosity verbosity, MessageCategory category, string message)
        {
            return Task.CompletedTask;
        }
    }
}
