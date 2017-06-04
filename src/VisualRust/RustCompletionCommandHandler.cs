using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using Antlr4.Runtime;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using VSCommand = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace VisualRust
{
    internal sealed class RustCompletionCommandHandler : VSCommandTarget<VSCommand>
    {
        private ICompletionSession currentSession;

        public RustCompletionCommandHandler(IVsTextView viewAdapter, IWpfTextView textView, ICompletionBroker broker)
            : base(viewAdapter, textView)
        {
            currentSession = null;
            Broker = broker;
        }

        public ICompletionBroker Broker { get; private set; }

        private char GetTypeChar(IntPtr pvaIn)
        {
            return (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
        }

        private bool IsSessionStarted
        {
            get { return currentSession != null && currentSession.IsStarted && !currentSession.IsDismissed; }
        }

        protected override bool Execute(VSCommand command, uint options, IntPtr pvaIn, IntPtr pvaOut)
        {
            bool handled = false;

            switch (command)
            {
                case VSConstants.VSStd2KCmdID.AUTOCOMPLETE:
                case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                    handled = StartSession();
                    break;
                case VSConstants.VSStd2KCmdID.RETURN:
                    handled = Complete(false);
                    break;
                case VSConstants.VSStd2KCmdID.TAB:
                    handled = Complete(true);
                    break;
                case VSConstants.VSStd2KCmdID.CANCEL:
                    handled = Cancel();
                    break;
            }

            bool hresult = true;

            if (!handled)
                hresult = ExecuteNext(command, options, pvaIn, pvaOut);

            if (hresult)
            {
                switch (command)
                {
                    case VSConstants.VSStd2KCmdID.TYPECHAR:
                        char ch = GetTypeChar(pvaIn);
                        var tokensAtCursor = Utils.GetTokensAtPosition(TextView.Caret.Position.BufferPosition);

                        RustTokenTypes? leftTokenType = tokensAtCursor.Item1 != null ? (RustTokenTypes?)Utils.LexerTokenToRustToken(tokensAtCursor.Item1.Text, tokensAtCursor.Item1.Type) : null;
                        RustTokenTypes? currentTokenType = tokensAtCursor.Item2 != null ? (RustTokenTypes?)Utils.LexerTokenToRustToken(tokensAtCursor.Item2.Text, tokensAtCursor.Item2.Type) : null;

                        RustTokenTypes[] cancelTokens = { RustTokenTypes.COMMENT, RustTokenTypes.STRING, RustTokenTypes.DOC_COMMENT, RustTokenTypes.WHITESPACE };

                        if (char.IsControl(ch)
                            || ch == ';'
                            || (leftTokenType.HasValue && cancelTokens.Contains(leftTokenType.Value))
                            || (currentTokenType.HasValue && cancelTokens.Contains(currentTokenType.Value)))
                        {
                            Cancel();
                        }
                        else if (leftTokenType == RustTokenTypes.STRUCTURAL)
                        {
                            Cancel();
                            StartSession();
                        }
                        else if (leftTokenType == RustTokenTypes.IDENT && currentTokenType == null)
                        {
                            StartSession();
                        }
                        else if (IsSessionStarted)
                        {
                            string applicableTo = currentSession.SelectedCompletionSet.ApplicableTo.GetText(currentSession.TextView.TextSnapshot);
                            if (string.IsNullOrEmpty(applicableTo))
                                RestartSession();
                            else
                                Filter();
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.BACKSPACE:
                    case VSConstants.VSStd2KCmdID.DELETE:
                    case VSConstants.VSStd2KCmdID.DELETEWORDLEFT:
                    case VSConstants.VSStd2KCmdID.DELETEWORDRIGHT:
                        Filter();
                        break;
                }
            }

            return hresult;
        }

        private void RestartSession()
        {
            if (IsSessionStarted)
            {
                currentSession.Dismiss();
            }
            StartSession();
        }

        private void Filter()
        {
            if (!IsSessionStarted)
                return;

            currentSession.SelectedCompletionSet.Filter();
            if (currentSession.SelectedCompletionSet.Completions.Count == 0)
                Cancel();
            else
                currentSession.SelectedCompletionSet.SelectBestMatch();
        }

        private bool Cancel()
        {
            if (currentSession == null)
                return false;

            currentSession.Dismiss();

            return true;
        }

        private bool Complete(bool force)
        {
            if (currentSession == null)
                return false;

            if (!currentSession.SelectedCompletionSet.SelectionStatus.IsSelected && !force)
            {
                currentSession.Dismiss();
                return false;
            }
            else
            {
                currentSession.Commit();
                return true;
            }
        }

        private bool StartSession()
        {
            if (currentSession != null)
            {
                Filter();
                return false;
            }

            SnapshotPoint caret = TextView.Caret.Position.BufferPosition;
            ITextSnapshot snapshot = caret.Snapshot;

            if (!Broker.IsCompletionActive(TextView))
            {
                currentSession = Broker.CreateCompletionSession(TextView, snapshot.CreateTrackingPoint(caret, PointTrackingMode.Positive), true);
            }
            else
            {
                currentSession = Broker.GetSessions(TextView)[0];
            }

            currentSession.Dismissed += (sender, args) => currentSession = null;

            currentSession.Start();
            Filter();
            return true;
        }

        protected override IEnumerable<VSCommand> SupportedCommands
        {
            get
            {
                yield return VSCommand.CANCEL;

                // start / filter / complete commands
                yield return VSCommand.TYPECHAR;
                yield return VSCommand.BACKSPACE;
                yield return VSCommand.DELETE;
                yield return VSCommand.DELETEWORDLEFT;
                yield return VSCommand.DELETEWORDRIGHT;

                // completion commands
                yield return VSCommand.RETURN;
                yield return VSCommand.TAB;

                // ctrl + space commands
                yield return VSCommand.COMPLETEWORD;
                yield return VSCommand.AUTOCOMPLETE;

                // ctrl + j commands
                yield return VSCommand.SHOWMEMBERLIST;
            }
        }

        protected override VSCommand ConvertFromCommandId(uint id)
        {
            return (VSCommand)id;
        }

        protected override uint ConvertFromCommand(VSCommand command)
        {
            return (uint)command;
        }
    }
}