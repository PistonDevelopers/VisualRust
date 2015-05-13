using System;
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

namespace VisualRust
{
    internal sealed class RustCompletionCommandHandler : IOleCommandTarget
    {
        private readonly IOleCommandTarget next;
        private ICompletionSession currentSession;

        public RustCompletionCommandHandler(IVsTextView viewAdapter, ITextView textView, ICompletionBroker broker)
        {
            currentSession = null;
            TextView = textView;
            Broker = broker;
            viewAdapter.AddCommandFilter(this, out next);
        }

        public ITextView TextView { get; private set; }

        public ICompletionBroker Broker { get; private set; }

        private char GetTypeChar(IntPtr pvaIn)
        {
            return (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
        }

        private bool IsSessionStarted
        {
            get { return currentSession != null && currentSession.IsStarted && !currentSession.IsDismissed; }
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            bool handled = false;
            int hresult = VSConstants.S_OK;

            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                switch ((VSConstants.VSStd2KCmdID)nCmdID)
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
            }

            if (!handled)
                hresult = next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            if (ErrorHandler.Succeeded(hresult))
            {
                if (pguidCmdGroup == VSConstants.VSStd2K)
                {
                    switch ((VSConstants.VSStd2KCmdID)nCmdID)
                    {
                        case VSConstants.VSStd2KCmdID.TYPECHAR:
                            char ch = GetTypeChar(pvaIn);
                            var tokensAtCursor = GetTokensAtCursor();

                            var leftTokenType = tokensAtCursor.Item1;
                            var currentTokenType = tokensAtCursor.Item2;

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
            }

            return hresult;
        }

        private Tuple<RustTokenTypes?, RustTokenTypes?> GetTokensAtCursor()
        {
            SnapshotPoint caret = TextView.Caret.Position.BufferPosition;
            var line = caret.GetContainingLine();
            var tokens = Utils.LexString(line.GetText()).ToList();

            if (tokens.Count == 0)
                return Tuple.Create<RustTokenTypes?, RustTokenTypes?>(null, null);

            int col = caret.Position - line.Start.Position;

            IToken leftToken;
            IToken currentToken = tokens.FirstOrDefault(t => col > t.StartIndex && col <= t.StopIndex);

            if (currentToken != null)
            {
                if (currentToken == tokens.First())
                    leftToken = null;
                else
                    leftToken = tokens[tokens.IndexOf(currentToken) - 1];
            }
            else
            {
                leftToken = tokens.Last();
            }

            RustTokenTypes? leftTokenType = leftToken != null ? (RustTokenTypes?)Utils.LexerTokenToRustToken(leftToken.Text, leftToken.Type) : null;
            RustTokenTypes? currentTokenType = currentToken != null ? (RustTokenTypes?)Utils.LexerTokenToRustToken(currentToken.Text, currentToken.Type) : null;

            return Tuple.Create(leftTokenType, currentTokenType);
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

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                switch ((VSConstants.VSStd2KCmdID)prgCmds[0].cmdID)
                {
                    case VSConstants.VSStd2KCmdID.AUTOCOMPLETE:
                    case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                        prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_ENABLED | (uint)OLECMDF.OLECMDF_SUPPORTED;
                        return VSConstants.S_OK;
                }
            }
            return next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }


    }
}