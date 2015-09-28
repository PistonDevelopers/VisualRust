﻿//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms.Design;
using System.Windows.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.Win32;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System.IO;

namespace Microsoft.VisualStudioTools.Project {
    /// <summary>
    /// This class implements an MSBuild logger that output events to VS outputwindow and tasklist.
    /// </summary>
    internal class IDEBuildLogger : Logger, IDisposable {
        #region fields

        private const string GeneralCollection = @"General";
        private const string BuildVerbosityProperty = "MSBuildLoggerVerbosity";
        private const string StdMacrosToken = "<std macros>";

        private int currentIndent;
        private IVsOutputWindowPane outputWindowPane;
        private string errorString = SR.GetString(SR.Error);
        private string warningString = SR.GetString(SR.Warning);
        private TaskProvider taskProvider;
        private IVsHierarchy hierarchy;
        private IServiceProvider serviceProvider;
        private Dispatcher dispatcher;
        private bool haveCachedVerbosity = false;

        private BuildErrorEventArgs lastMacroError;

        // Queues to manage Tasks and Error output plus message logging
        private ConcurrentQueue<Func<ErrorTask>> taskQueue;
        private ConcurrentQueue<OutputQueueEntry> outputQueue;

        #endregion

        #region properties

        public IServiceProvider ServiceProvider {
            get { return this.serviceProvider; }
        }

        public string WarningString {
            get { return this.warningString; }
            set { this.warningString = value; }
        }

        public string ErrorString {
            get { return this.errorString; }
            set { this.errorString = value; }
        }

        /// <summary>
        /// When the build is not a "design time" (background or secondary) build this is True
        /// </summary>
        /// <remarks>
        /// The only known way to detect an interactive build is to check this.outputWindowPane for null.
        /// </remarks>
        protected bool InteractiveBuild {
            get { return this.outputWindowPane != null; }
        }

        /// <summary>
        /// Set to null to avoid writing to the output window
        /// </summary>
        internal IVsOutputWindowPane OutputWindowPane {
            get { return this.outputWindowPane; }
            set { this.outputWindowPane = value; }
        }

        #endregion

        #region ctors

        /// <summary>
        /// Constructor.  Inititialize member data.
        /// </summary>
        public IDEBuildLogger(IVsOutputWindowPane output, TaskProvider taskProvider, IVsHierarchy hierarchy) {
            UIThread.MustBeCalledFromUIThread();

            Utilities.ArgumentNotNull("taskProvider", taskProvider);
            Utilities.ArgumentNotNull("hierarchy", hierarchy);

            Trace.WriteLineIf(Thread.CurrentThread.GetApartmentState() != ApartmentState.STA, "WARNING: IDEBuildLogger constructor running on the wrong thread.");

            IOleServiceProvider site;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hierarchy.GetSite(out site));

            this.taskProvider = taskProvider;
            this.outputWindowPane = output;
            this.hierarchy = hierarchy;
            this.serviceProvider = new ServiceProvider(site);
            this.dispatcher = Dispatcher.CurrentDispatcher;
        }

        #endregion

        #region IDisposable

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                var sp = this.serviceProvider as ServiceProvider;
                this.serviceProvider = null;
                if (sp != null) {
                    sp.Dispose();
                }
            }
        }

        #endregion

        #region overridden methods

        /// <summary>
        /// Overridden from the Logger class.
        /// </summary>
        public override void Initialize(IEventSource eventSource) {
            Utilities.ArgumentNotNull("eventSource", eventSource);

            this.taskQueue = new ConcurrentQueue<Func<ErrorTask>>();
            this.outputQueue = new ConcurrentQueue<OutputQueueEntry>();

            eventSource.BuildStarted += new BuildStartedEventHandler(BuildStartedHandler);
            eventSource.BuildFinished += new BuildFinishedEventHandler(BuildFinishedHandler);
            eventSource.ProjectStarted += new ProjectStartedEventHandler(ProjectStartedHandler);
            eventSource.ProjectFinished += new ProjectFinishedEventHandler(ProjectFinishedHandler);
            eventSource.TargetStarted += new TargetStartedEventHandler(TargetStartedHandler);
            eventSource.TargetFinished += new TargetFinishedEventHandler(TargetFinishedHandler);
            eventSource.TaskStarted += new TaskStartedEventHandler(TaskStartedHandler);
            eventSource.TaskFinished += new TaskFinishedEventHandler(TaskFinishedHandler);
            eventSource.CustomEventRaised += new CustomBuildEventHandler(CustomHandler);
            eventSource.ErrorRaised += new BuildErrorEventHandler(ErrorRaisedHandler);
            eventSource.WarningRaised += new BuildWarningEventHandler(WarningHandler);
            eventSource.MessageRaised += new BuildMessageEventHandler(MessageHandler);
        }

        #endregion

        #region event delegates

        /// <summary>
        /// This is the delegate for BuildStartedHandler events.
        /// </summary>
        protected virtual void BuildStartedHandler(object sender, BuildStartedEventArgs buildEvent) {
            // NOTE: This may run on a background thread!
            ClearCachedVerbosity();
            ClearQueuedOutput();
            ClearQueuedTasks();

            QueueOutputEvent(MessageImportance.Low, buildEvent);
        }

        /// <summary>
        /// This is the delegate for BuildFinishedHandler events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="buildEvent"></param>
        protected virtual void BuildFinishedHandler(object sender, BuildFinishedEventArgs buildEvent) {
            // NOTE: This may run on a background thread!
            MessageImportance importance = buildEvent.Succeeded ? MessageImportance.Low : MessageImportance.High;
            QueueOutputText(importance, Environment.NewLine);
            QueueOutputEvent(importance, buildEvent);

            // flush output and error queues
            ReportQueuedOutput();
            ReportQueuedTasks();
        }

        /// <summary>
        /// This is the delegate for ProjectStartedHandler events.
        /// </summary>
        protected virtual void ProjectStartedHandler(object sender, ProjectStartedEventArgs buildEvent) {
            // NOTE: This may run on a background thread!
            QueueOutputEvent(MessageImportance.Low, buildEvent);
        }

        /// <summary>
        /// This is the delegate for ProjectFinishedHandler events.
        /// </summary>
        protected virtual void ProjectFinishedHandler(object sender, ProjectFinishedEventArgs buildEvent) {
            // NOTE: This may run on a background thread!
            QueueOutputEvent(buildEvent.Succeeded ? MessageImportance.Low : MessageImportance.High, buildEvent);
        }

        /// <summary>
        /// This is the delegate for TargetStartedHandler events.
        /// </summary>
        protected virtual void TargetStartedHandler(object sender, TargetStartedEventArgs buildEvent) {
            // NOTE: This may run on a background thread!
            QueueOutputEvent(MessageImportance.Low, buildEvent);
            IndentOutput();
        }

        /// <summary>
        /// This is the delegate for TargetFinishedHandler events.
        /// </summary>
        protected virtual void TargetFinishedHandler(object sender, TargetFinishedEventArgs buildEvent) {
            // NOTE: This may run on a background thread!
            UnindentOutput();
            QueueOutputEvent(MessageImportance.Low, buildEvent);
        }

        /// <summary>
        /// This is the delegate for TaskStartedHandler events.
        /// </summary>
        protected virtual void TaskStartedHandler(object sender, TaskStartedEventArgs buildEvent) {
            // NOTE: This may run on a background thread!
            QueueOutputEvent(MessageImportance.Low, buildEvent);
            IndentOutput();
        }

        /// <summary>
        /// This is the delegate for TaskFinishedHandler events.
        /// </summary>
        protected virtual void TaskFinishedHandler(object sender, TaskFinishedEventArgs buildEvent) {
            // NOTE: This may run on a background thread!
            UnindentOutput();
            QueueOutputEvent(MessageImportance.Low, buildEvent);
        }

        /// <summary>
        /// This is the delegate for CustomHandler events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="buildEvent"></param>
        protected virtual void CustomHandler(object sender, CustomBuildEventArgs buildEvent) {
            // NOTE: This may run on a background thread!
            QueueOutputEvent(MessageImportance.High, buildEvent);
        }

        /// <summary>
        /// This is the delegate for error events.
        /// </summary>
        protected virtual void ErrorRaisedHandler(object sender, BuildErrorEventArgs errorEvent) {
            // NOTE: This may run on a background thread!
            QueueOutputText(GetFormattedErrorMessage(errorEvent.File, errorEvent.LineNumber, errorEvent.ColumnNumber, false, errorEvent.Code, errorEvent.Message));

            if (errorEvent.File == StdMacrosToken)
            {
                HandleMacroErrorEvent(errorEvent);
            }
            else
            {
                QueueTaskEvent(errorEvent);
            }
        }

        /// <summary>
        /// This is the delegate for warning events.
        /// </summary>
        protected virtual void WarningHandler(object sender, BuildWarningEventArgs warningEvent) {
            // NOTE: This may run on a background thread!
            QueueOutputText(MessageImportance.High, GetFormattedErrorMessage(warningEvent.File, warningEvent.LineNumber, warningEvent.ColumnNumber, true, warningEvent.Code, warningEvent.Message));

            if (warningEvent.File == StdMacrosToken)
            {
                // Ignore these. See the comment in HandleMacroErrorEvent.
            }
            else
            {
                if (lastMacroError != null)
                {
                    TryQueueLastMacroError(warningEvent);
                }

                QueueTaskEvent(warningEvent);
            }
        }

        /// <summary>
        /// This is the delegate for Message event types
        /// </summary>
        protected virtual void MessageHandler(object sender, BuildMessageEventArgs messageEvent) {
            // NOTE: This may run on a background thread!

            // Special-case this event type. It's reported by tasks derived from ToolTask, and prints out the command line used
            // to invoke the tool.  It has high priority for some unclear reason, but we really don't want to be showing it for
            // verbosity below normal (https://nodejstools.codeplex.com/workitem/693). The check here is taken directly from the
            // standard MSBuild console logger, which does the same thing.
            if (messageEvent is TaskCommandLineEventArgs && !IsVerbosityAtLeast(LoggerVerbosity.Normal)) {
                return;
            }

            QueueOutputEvent(messageEvent.Importance, messageEvent);
        }

        #endregion

        #region output queue

        protected void QueueOutputEvent(MessageImportance importance, BuildEventArgs buildEvent) {
            // NOTE: This may run on a background thread!
            if (LogAtImportance(importance) && !string.IsNullOrEmpty(buildEvent.Message)) {
                StringBuilder message = new StringBuilder(this.currentIndent + buildEvent.Message.Length);
                if (this.currentIndent > 0) {
                    message.Append('\t', this.currentIndent);
                }
                message.AppendLine(buildEvent.Message);

                QueueOutputText(message.ToString());
            }
        }

        protected void QueueOutputText(MessageImportance importance, string text) {
            // NOTE: This may run on a background thread!
            if (LogAtImportance(importance)) {
                QueueOutputText(text);
            }
        }

        protected void QueueOutputText(string text) {
            // NOTE: This may run on a background thread!
            if (this.OutputWindowPane != null) {
                // Enqueue the output text
                this.outputQueue.Enqueue(new OutputQueueEntry(text, OutputWindowPane));

                // We want to interactively report the output. But we dont want to dispatch
                // more than one at a time, otherwise we might overflow the main thread's
                // message queue. So, we only report the output if the queue was empty.
                if (this.outputQueue.Count == 1) {
                    ReportQueuedOutput();
                }
            }
        }

        private void IndentOutput() {
            // NOTE: This may run on a background thread!
            this.currentIndent++;
        }

        private void UnindentOutput() {
            // NOTE: This may run on a background thread!
            this.currentIndent--;
        }

        private void ReportQueuedOutput() {
            // NOTE: This may run on a background thread!
            // We need to output this on the main thread. We must use BeginInvoke because the main thread may not be pumping events yet.
            BeginInvokeWithErrorMessage(this.serviceProvider, this.dispatcher, FlushBuildOutput);
        }

        internal void FlushBuildOutput() {
            OutputQueueEntry output;

            while (this.outputQueue.TryDequeue(out output)) {
                ErrorHandler.ThrowOnFailure(output.Pane.OutputString(output.Message));
            }
        }

        private void ClearQueuedOutput() {
            // NOTE: This may run on a background thread!
            this.outputQueue = new ConcurrentQueue<OutputQueueEntry>();
        }

        #endregion output queue

        #region task queue

        class NavigableErrorTask : ErrorTask {
            private readonly IServiceProvider _serviceProvider;

            public NavigableErrorTask(IServiceProvider serviceProvider) {
                _serviceProvider = serviceProvider;
            }

            protected override void OnNavigate(EventArgs e) {
                VsUtilities.NavigateTo(
                    _serviceProvider,
                    Document,
                    Guid.Empty,
                    Line,
                    Column - 1
                );
                base.OnNavigate(e);
            }
        }

        protected void QueueTaskEvent(BuildEventArgs errorEvent) {
            // This enqueues a function that will later be run on the main (UI) thread
            this.taskQueue.Enqueue(() =>
            {
                TextSpan span;
                string file;
                MARKERTYPE marker;
                TaskErrorCategory category;

                if (errorEvent is BuildErrorEventArgs)
                {
                    BuildErrorEventArgs errorArgs = (BuildErrorEventArgs)errorEvent;
                    span = new TextSpan();
                    // spans require zero-based indices
                    span.iStartLine = errorArgs.LineNumber - 1;
                    span.iEndLine = errorArgs.EndLineNumber - 1;
                    span.iStartIndex = errorArgs.ColumnNumber - 1;
                    span.iEndIndex = errorArgs.EndColumnNumber - 1;
                    file = GetRelativeOrProjectPath(errorArgs.ProjectFile, errorArgs.File);
                    marker = MARKERTYPE.MARKER_CODESENSE_ERROR; // red squiggles
                    category = TaskErrorCategory.Error;
                }
                else if (errorEvent is BuildWarningEventArgs)
                {
                    BuildWarningEventArgs warningArgs = (BuildWarningEventArgs)errorEvent;
                    span = new TextSpan();
                    // spans require zero-based indices
                    span.iStartLine = warningArgs.LineNumber - 1;
                    span.iEndLine = warningArgs.EndLineNumber - 1;
                    span.iStartIndex = warningArgs.ColumnNumber - 1;
                    span.iEndIndex = warningArgs.EndColumnNumber - 1;
                    file = GetRelativeOrProjectPath(warningArgs.ProjectFile, warningArgs.File);
                    marker = MARKERTYPE.MARKER_COMPILE_ERROR; // red squiggles
                    category = TaskErrorCategory.Warning;
                }
                else
                {
                    throw new NotImplementedException();
                }

                if (span.iEndLine == -1) span.iEndLine = span.iStartLine;
                if (span.iEndIndex == -1) span.iEndIndex = span.iStartIndex;

                IVsUIShellOpenDocument openDoc = serviceProvider.GetService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
                if (openDoc == null)
                    throw new NotImplementedException(); // TODO

                IVsWindowFrame frame;
                IOleServiceProvider sp;
                IVsUIHierarchy hier;
                uint itemid;
                Guid logicalView = VSConstants.LOGVIEWID_Code;

                IVsTextLines buffer = null;

                // Notes about acquiring the buffer:
                // If the file physically exists then this will open the document in the current project. It doesn't matter if the file is a member of the project.
                // Also, it doesn't matter if this is a Rust file. For example, an error in Microsoft.Common.targets will cause a file to be opened here.
                // However, opening the document does not mean it will be shown in VS. 
                if (!Microsoft.VisualStudio.ErrorHandler.Failed(openDoc.OpenDocumentViaProject(file, ref logicalView, out sp, out hier, out itemid, out frame)) && frame != null)
                {
                    object docData;
                    frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out docData);

                    // Get the text lines
                    buffer = docData as IVsTextLines;

                    if (buffer == null)
                    {
                        IVsTextBufferProvider bufferProvider = docData as IVsTextBufferProvider;
                        if (bufferProvider != null)
                        {
                            bufferProvider.GetTextBuffer(out buffer);
                        }
                    }
                }

                ErrorTask task = CreateErrorTask(errorEvent, span, file, marker, category, buffer);
                task.ErrorCategory = category;
                task.Document = file;
                task.Line = span.iStartLine;
                task.Column = span.iStartIndex;
                task.Priority = category == TaskErrorCategory.Error ? TaskPriority.High : TaskPriority.Normal;
                task.Text = errorEvent.Message;
                task.Category = TaskCategory.BuildCompile;
                task.HierarchyItem = hierarchy;
                return task;
            });

            // NOTE: Unlike output we dont want to interactively report the tasks. So we never queue
            // call ReportQueuedTasks here. We do this when the build finishes.
        }

        static string GetRelativeOrProjectPath(string projectFile, string file)
        {
            bool isInvalidFileName = file.IndexOfAny(Path.GetInvalidPathChars()) != -1;
            if(isInvalidFileName)
                return projectFile;
            return Path.Combine(Path.GetDirectoryName(projectFile), file);

        }

        ErrorTask CreateErrorTask(BuildEventArgs errorEvent, TextSpan span, string file, MARKERTYPE marker, TaskErrorCategory category, IVsTextLines buffer)
        {
            if(buffer != null)
                return new DocumentTask(serviceProvider, buffer, marker, span, file);
            else
                return new ErrorTask();
        }

        private void ReportQueuedTasks() {
            // NOTE: This may run on a background thread!
            // We need to output this on the main thread. We must use BeginInvoke because the main thread may not be pumping events yet.
            BeginInvokeWithErrorMessage(this.serviceProvider, this.dispatcher, () => {
                this.taskProvider.SuspendRefresh();
                try {
                    Func<ErrorTask> taskFunc;

                    while (this.taskQueue.TryDequeue(out taskFunc)) {
                        // Create the error task
                        ErrorTask task = taskFunc();

                        // Log the task
                        this.taskProvider.Tasks.Add(task);
                    }
                } finally {
                    this.taskProvider.ResumeRefresh();
                }
            });
        }

        private void ClearQueuedTasks() {
            // NOTE: This may run on a background thread!
            this.taskQueue = new ConcurrentQueue<Func<ErrorTask>>();

            if (this.InteractiveBuild) {
                // We need to clear this on the main thread. We must use BeginInvoke because the main thread may not be pumping events yet.
                BeginInvokeWithErrorMessage(this.serviceProvider, this.dispatcher, () => {
                    this.taskProvider.Tasks.Clear();
                });
            }
        }

        #endregion task queue

        #region helpers

        /// <summary>
        /// This method takes a MessageImportance and returns true if messages
        /// at importance i should be loggeed.  Otherwise return false.
        /// </summary>
        private bool LogAtImportance(MessageImportance importance) {
            // If importance is too low for current settings, ignore the event
            bool logIt = false;

            this.SetVerbosity();

            switch (this.Verbosity) {
                case LoggerVerbosity.Quiet:
                    logIt = false;
                    break;
                case LoggerVerbosity.Minimal:
                    logIt = (importance == MessageImportance.High);
                    break;
                case LoggerVerbosity.Normal:
                // Falling through...
                case LoggerVerbosity.Detailed:
                    logIt = (importance != MessageImportance.Low);
                    break;
                case LoggerVerbosity.Diagnostic:
                    logIt = true;
                    break;
                default:
                    Debug.Fail("Unknown Verbosity level. Ignoring will cause nothing to be logged");
                    break;
            }

            return logIt;
        }

        /// <summary>
        /// Format error messages for the task list
        /// </summary>
        private string GetFormattedErrorMessage(
            string fileName,
            int line,
            int column,
            bool isWarning,
            string errorNumber,
            string errorText) {
            string errorCode = isWarning ? this.WarningString : this.ErrorString;

            StringBuilder message = new StringBuilder();
            if (!string.IsNullOrEmpty(fileName)) {
                message.AppendFormat(CultureInfo.CurrentCulture, "{0}({1},{2}):", fileName, line, column);
            }
            message.AppendFormat(CultureInfo.CurrentCulture, " {0} {1}: {2}", errorCode, errorNumber, errorText);
            message.AppendLine();

            return message.ToString();
        }

        /// <summary>
        /// Sets the verbosity level.
        /// </summary>
        private void SetVerbosity() {
            if (!this.haveCachedVerbosity) {
                this.Verbosity = LoggerVerbosity.Normal;

                try {
                    var settings = new ShellSettingsManager(serviceProvider);
                    var store = settings.GetReadOnlySettingsStore(SettingsScope.UserSettings);
                    if (store.CollectionExists(GeneralCollection) && store.PropertyExists(GeneralCollection, BuildVerbosityProperty)) {
                        this.Verbosity = (LoggerVerbosity)store.GetInt32(GeneralCollection, BuildVerbosityProperty, (int)LoggerVerbosity.Normal);
                    }
                } catch (Exception ex) {
                    var message = string.Format(
                        "Unable to read verbosity option from the registry.{0}{1}",
                        Environment.NewLine,
                        ex.ToString()
                    );
                    this.QueueOutputText(MessageImportance.High, message);
                }

                this.haveCachedVerbosity = true;
            }
        }

        /// <summary>
        /// Clear the cached verbosity, so that it will be re-evaluated from the build verbosity registry key.
        /// </summary>
        private void ClearCachedVerbosity() {
            this.haveCachedVerbosity = false;
        }

        /// <summary>
        /// Handle error event from inside a macro expansion.
        /// </summary>
        /// <param name="errorEvent"></param>
        private void HandleMacroErrorEvent(BuildErrorEventArgs errorEvent)
        {
            // Errors reported from inside macros are a special case.
            // There are 2 problems with them:
            //    1. The errors themselves are reported for file "<std macros>", and therefore we
            //       can't call QueueTaskEvent for them, because the path is invalid.
            //    2. In some cases the only thing reported against the real "*.rs" file is a "warning",
            //       so if we just ignore all the errors with "<std macros>", the error window in VS
            //       doesn't pop up, because there are no errors, only warnings.
            // 
            // Let's remember the last macro error we saw, and report it the first time
            // we see a "warning" for macro expansion in a real Rust file.

            /* Sample code

                    use std::sync::atomic::AtomicUsize;
                    fn test2() {
                        let v = vec![AtomicUsize::new(0); 2];
                        println!("{:?}", vec![AtomicUsize::new(0); 2]);
                    }

                Sample output

                     <std macros>(1,37): error E0277: the trait `core::clone::Clone` is not implemented for the type `core::sync::atomic::AtomicUsize`
                     src\lib.rs(161,13): warning : note: in this expansion of vec! (defined in <std macros>)
                     <std macros>(1,37): error : run `rustc --explain E0277` to see a detailed explanation
                     note: required by `collections::vec::from_elem`

                     <std macros>(1,37): error E0277: the trait `core::clone::Clone` is not implemented for the type `core::sync::atomic::AtomicUsize`
                     src\lib.rs(162,22): warning : note: in this expansion of vec! (defined in <std macros>)
                     <std macros>(2,25): warning : note: in this expansion of format_args!
                     <std macros>(3,1): warning : note: in this expansion of print! (defined in <std macros>)
                     src\lib.rs(162,5): warning : note: in this expansion of println! (defined in <std macros>)
                     <std macros>(1,37): error : run `rustc --explain E0277` to see a detailed explanation
                     note: required by `collections::vec::from_elem`
            */

            lastMacroError = errorEvent;
        }

        /// <summary>
        /// Report last macro error using the information from the warning,
        /// if the warning is of a special form.
        /// </summary>
        private void TryQueueLastMacroError(BuildWarningEventArgs warningEvent)
        {
            if (lastMacroError != null
                && warningEvent.Message.Contains("note:")
                && warningEvent.Message.Contains(StdMacrosToken))
            {
                // Assume the last macro error is related to this "warning".
                // See the comment in HandleMacroErrorEvent.
                // This is fragile, but if we ignore this error completely
                // we will only display vague warning messages for macro errors.
                var adjustedMacrosError = new BuildErrorEventArgs
                    (
                        lastMacroError.Subcategory,
                        lastMacroError.Code,
                        warningEvent.File,
                        warningEvent.LineNumber,
                        warningEvent.ColumnNumber,
                        warningEvent.EndLineNumber,
                        warningEvent.EndColumnNumber,
                        lastMacroError.Message,
                        lastMacroError.HelpKeyword,
                        lastMacroError.SenderName,
                        lastMacroError.Timestamp
                    );

                adjustedMacrosError.ProjectFile = warningEvent.ProjectFile;

                lastMacroError = null;
                QueueTaskEvent(adjustedMacrosError);
            }
        }

        #endregion helpers

        #region exception handling helpers

        /// <summary>
        /// Call Dispatcher.BeginInvoke, showing an error message if there was a non-critical exception.
        /// </summary>
        /// <param name="serviceProvider">service provider</param>
        /// <param name="dispatcher">dispatcher</param>
        /// <param name="action">action to invoke</param>
        private static void BeginInvokeWithErrorMessage(IServiceProvider serviceProvider, Dispatcher dispatcher, Action action) {
            dispatcher.BeginInvoke(new Action(() => CallWithErrorMessage(serviceProvider, action)));
        }

        /// <summary>
        /// Show error message if exception is caught when invoking a method
        /// </summary>
        /// <param name="serviceProvider">service provider</param>
        /// <param name="action">action to invoke</param>
        private static void CallWithErrorMessage(IServiceProvider serviceProvider, Action action) {
            try {
                action();
            } catch (Exception ex) {
                if (ex.IsCriticalException()) {
                    throw;
                }

                ShowErrorMessage(serviceProvider, ex);
            }
        }

        /// <summary>
        /// Show error window about the exception
        /// </summary>
        /// <param name="serviceProvider">service provider</param>
        /// <param name="exception">exception</param>
        private static void ShowErrorMessage(IServiceProvider serviceProvider, Exception exception) {
            IUIService UIservice = (IUIService)serviceProvider.GetService(typeof(IUIService));
            if (UIservice != null && exception != null) {
                UIservice.ShowError(exception);
            }
        }

        #endregion exception handling helpers

        class OutputQueueEntry {
            public readonly string Message;
            public readonly IVsOutputWindowPane Pane;

            public OutputQueueEntry(string message, IVsOutputWindowPane pane) {
                Message = message;
                Pane = pane;
            }
        }
    }
}