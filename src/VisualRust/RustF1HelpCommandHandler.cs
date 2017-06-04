using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;
using Antlr4.Runtime;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace VisualRust
{
    class RustF1HelpCommandHandler : VSCommandTarget<VSConstants.VSStd97CmdID>
    {
        readonly ITextBuffer Buffer;

        public RustF1HelpCommandHandler(IVsTextView vsTextView, IWpfTextView textView)
            : base(vsTextView, textView)
        {
            Buffer = textView.TextBuffer;
        }

        protected override bool Execute(VSConstants.VSStd97CmdID command, uint options, IntPtr pvaIn, IntPtr pvaOut)
        {
            // TODO: This could consider more context than just the current lexer token,
            //       especially for IDENTs we probably want the whole path.
            //       Ideally we would even use the path after resolution (incorporating imported modules, etc)
            var snapshot = Buffer.CurrentSnapshot;
            var tokens = Utils.GetTokensAtPosition(TextView.Caret.Position.BufferPosition);
            var leftToken = tokens.Item1;
            var currentToken = tokens.Item2;

            var searchToken = currentToken ?? leftToken;
            if (searchToken == null)
            {
                // TODO: By returning false we fall back to the default MSDN page if we don't have a valid token/context.
                //       Is this what we want?
                return false;
            }

            var type = Utils.LexerTokenToRustToken(searchToken.Text, searchToken.Type);
            var text = searchToken.Text;
            String helpUrl;

            // TODO: This "translation" should be done by a web service, as is the case with MSDN,
            //       in order to decouple this plugin from the structure of the Rust documentation.
            //       Furthermore, it is not clear whether referring to The Book is useful here.
            switch (type)
            {
                case RustTokenTypes.IDENT:
                    helpUrl = "https://doc.rust-lang.org/std/?search=" + text;
                    break;
                case RustTokenTypes.LIFETIME:
                    helpUrl = "https://doc.rust-lang.org/book/lifetimes.html";
                    break;
                case RustTokenTypes.STRING:
                    helpUrl = "https://doc.rust-lang.org/book/strings.html";
                    break;
                case RustTokenTypes.KEYWORD:
                default:
                    // TODO?
                    return false;
            }

            // "run" the URL in the default browser
            System.Diagnostics.Process.Start(helpUrl);
            return true;
        }

        protected override IEnumerable<VSConstants.VSStd97CmdID> SupportedCommands
        {
            get
            {
                yield return VSConstants.VSStd97CmdID.F1Help;
            }
        }

        protected override VSConstants.VSStd97CmdID ConvertFromCommandId(uint id)
        {
            return (VSConstants.VSStd97CmdID)id;
        }

        protected override uint ConvertFromCommand(VSConstants.VSStd97CmdID command)
        {
            return (uint)command;
        }
    }
}
