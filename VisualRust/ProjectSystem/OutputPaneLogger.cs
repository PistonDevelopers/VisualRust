using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using VisualRust.Core;
using Microsoft.VisualStudio.Threading;
#if VS14
using IThreadHandling = Microsoft.VisualStudio.ProjectSystem.IThreadHandling;
using Microsoft.VisualStudio.ProjectSystem.VS;
#else
using IThreadHandling = Microsoft.VisualStudio.ProjectSystem.IProjectThreadingService;
using Microsoft.VisualStudio.ProjectSystem.VS.Build;
#endif

namespace VisualRust.ProjectSystem
{
    using System.IO;
    using System.Threading.Tasks;

    [Export(typeof(IActionLog))]
    class OutputPaneLogger : IActionLog
    {
        const string Key = "1B6CF02F-29CA-417B-A07E-122EF7C98B5A";
        private IVsOutputWindowPane pane;
        private JoinableTaskFactory taskFactory;

        [ImportingConstructor]
        public OutputPaneLogger(
            [Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider,
            IThreadHandling threadHandling,
            IVsOutputWindowTextWriterFactory outputWriterFactory)
        {
#if VS14
            Initialize(serviceProvider, threadHandling.AsyncPump, outputWriterFactory);
#else
            Initialize(serviceProvider, threadHandling.JoinableTaskFactory, outputWriterFactory);
#endif
        }

        private void Initialize(IServiceProvider serviceProvider, JoinableTaskFactory taskFactory, IVsOutputWindowTextWriterFactory outputWriterFactory)
        {
            taskFactory.Run(async delegate
            {
                await taskFactory.SwitchToMainThreadAsync();
                //IVsOutputWindow window = (IVsOutputWindow)Package.GetGlobalService(typeof(SVsOutputWindow));
                IVsOutputWindow window = (IVsOutputWindow)serviceProvider.GetService(typeof(SVsOutputWindow));
                var key = new Guid(Key);
                Verify.HResult(window.CreatePane(ref key, "Visual Rust", 1, 0));
                IVsOutputWindowPane pane;
                Verify.HResult(window.GetPane(ref key, out pane));
                this.pane = pane;
            });
            this.taskFactory = taskFactory;
        }

        public void Error(string msg)
        {
            // Can't use Run(...) here, because sometimes we are called from inside the project lock
            this.taskFactory.RunAsync(async delegate
            {
                await taskFactory.SwitchToMainThreadAsync();
                Verify.HResult(pane.OutputString($"{msg}\n"));
            });
        }

        public void Trace(string msg)
        {
            Error(msg);
        }
    }
}