using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using VisualRust.Core;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace VisualRust.ProjectSystem
{
    class OutputPaneLogger : IActionLog
    {
        const string Key = "1B6CF02F-29CA-417B-A07E-122EF7C98B5A";
        IVsOutputWindowPane pane;

        public OutputPaneLogger(IServiceProvider serviceProvider)
        {
            IVsOutputWindow window = (IVsOutputWindow)serviceProvider.GetService(typeof(SVsOutputWindow));
            var key = new Guid(Key);
            Verify.HResult(window.CreatePane(ref key, "Visual Rust", 1, 0));
            Verify.HResult(window.GetPane(ref key, out pane));

        }

        public void Error(string msg)
        {
            pane.OutputStringThreadSafe(msg);
        }

        public void Trace(string msg)
        {
            pane.OutputStringThreadSafe(msg);
        }
    }
}