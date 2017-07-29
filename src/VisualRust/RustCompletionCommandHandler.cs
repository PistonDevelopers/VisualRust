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
using System.Diagnostics;

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
                        var tokens      = Utils.GetLineTokensUpTo(TextView.Caret.Position.BufferPosition);
                        var tokenTypes  = tokens.Select(token => Utils.LexerTokenToRustToken(token.Text, token.Type)).ToList();
                        int tokensCount = tokenTypes.Count;
                        RustTokenTypes? currentTokenType  = tokensCount >= 1 ? (RustTokenTypes?)tokenTypes[tokensCount-1] : null; // the token we just helped type
                        Trace("TYPECHAR: ch={0}, currentToken={1}", ch, currentTokenType);

                        RustTokenTypes[] cancelTokens = { RustTokenTypes.COMMENT, RustTokenTypes.STRING, RustTokenTypes.DOC_COMMENT, RustTokenTypes.WHITESPACE };
                        if (char.IsControl(ch) || (currentTokenType.HasValue && cancelTokens.Contains(currentTokenType.Value)))
                        {
                            Cancel();
                        }
                        else if (currentTokenType == RustTokenTypes.STRUCTURAL)
                        {
                            var subtype = tokens.Last().Type;
                            if (subtype == RustLexer.RustLexer.DOT || subtype == RustLexer.RustLexer.MOD_SEP)
                            {
                                RestartSession();
                            }
                            else
                            {
                                Cancel();
                            }
                        }
                        else if (currentTokenType == RustTokenTypes.IDENT)
                        {
                            Func<string, bool> isPreceededBy = (prefixStr) =>
                            {
                                var prefix = prefixStr.Split(' ');
                                if (tokensCount-1 < 2*prefix.Length)
                                    return false; // not enough tokens to be preceeded by all that
                                for (int i=0; i<prefix.Length; ++i)
                                {
                                    if (tokens[tokensCount - 2*prefix.Length + 2*i - 1].Text != prefix[i]) return false; // mismatch
                                    if (tokens[tokensCount - 2*prefix.Length + 2*i - 0].Type != RustLexer.RustLexer.WHITESPACE) return false; // TODO: Allow e.g. mixture of comments/whitespace
                                }
                                return true;
                            };

                            // Don't start a session when naming new things...
                            if (isPreceededBy("fn")) break;
                            // Note that "if let" and "while let" can do pattern matching which could benifit from intellisense
                            if (isPreceededBy("let") && !isPreceededBy("if let") && !isPreceededBy("while let")) break;
                            if (isPreceededBy("let mut") && !isPreceededBy("if let mut") && !isPreceededBy("while let mut")) break;
                            if (isPreceededBy("const") || isPreceededBy("static") || isPreceededBy("static mut")) break; // shouldn't cull 'static lifetimes, only static vars
                            if (isPreceededBy("struct")) break;
                            if (isPreceededBy("trait")) break;
                            if (isPreceededBy("enum")) break;
                            if (isPreceededBy("type")) break;

                            // Things we still allow to complete:
                            // closure var names - need a full AST to properly disambugate those from identifiers after binary |s
                            // impl s - these reference existing types, so this is by design
                            // mod s - not always new modules, so this is also by design?

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
            Trace("RestartSession()");
            if (IsSessionStarted)
            {
                currentSession.Dismiss();
            }
            StartSession();
        }

        private void Filter()
        {
            Trace("Filter()");
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
            Trace("Cancel()");
            if (currentSession == null)
                return false;

            currentSession.Dismiss();

            return true;
        }

        private bool Complete(bool force)
        {
            Trace("Complete(force={0})", force);
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
            Trace("StartSession()");
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

        [Conditional("DISABLED")]
        private void Trace(string message, params object[] args)
        {
            SnapshotPoint caret = TextView.Caret.Position.BufferPosition;
            ITextSnapshotLine line = caret.GetContainingLine();

            var fmtMsg = string.Format(message, args);
            var fmtSess = currentSession == null ? "null" : currentSession.IsDismissed ? "dismissed" : currentSession.IsStarted ? "started" : "...ready?";
            var fmtText = line.GetText().Replace('\t',' ').Insert(caret.Position-line.Start, "@");
            Utils.DebugPrintToOutput("RustCompletionCommandHandler: {0} session={1} text=[{2}]", fmtMsg.PadRight(25, ' '), fmtSess.PadRight("dismissed".Length, ' '), fmtText);
        }
    }
}