using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Antlr4.Runtime;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace VisualRust
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType("rust")]
    public class VisualRustSmartIndentProvider : ISmartIndentProvider
    {
        [Import]
        public IEditorOperationsFactoryService OperationsFactory = null;

        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return new VisualRustSmartIndent(textView, this.OperationsFactory.GetEditorOperations(textView));
        }
    }

    // TODO: this indenter should take comments into consideration
    // TODO: this indenter should take tabs into consideration
    class VisualRustSmartIndent : ISmartIndent
    {
        private IEditorOperations ed;

        private ITextView _textView;
        internal VisualRustSmartIndent(ITextView textView, IEditorOperations operations)
        {
            _textView = textView;
            ed = operations;
        }

        // Check if the last non-whitespace token is left brace
        private static bool EndsWidthLBrace(List<IToken> tokens)
        {
            for(int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i].Type == RustLexer.RustLexer.WHITESPACE)
                    continue;
                else if (tokens[i].Type == RustLexer.RustLexer.LBRACE)
                    return true;
                else
                    return false;
            }
            throw new InvalidOperationException();
        }

        int? ISmartIndent.GetDesiredIndentation(ITextSnapshotLine line)
        {
            ITextSnapshot snap = _textView.TextSnapshot;
            int indentSize = _textView.Options.GetIndentSize();
            // get all of the previous lines
            IEnumerable<ITextSnapshotLine> lines = snap.Lines.Reverse().Skip(snap.LineCount - line.LineNumber);
            foreach (ITextSnapshotLine prevLine in lines)
            {
                var text = prevLine.GetText();
                if (String.IsNullOrWhiteSpace(text))
                {
                    continue;
                }
                List<IToken> toks = Utils.LexString(text).ToList();
                // The line before ends with {, we add tab
                if (EndsWidthLBrace(toks))
                {
                    return prevLine.GetText().TakeWhile(c2 => c2 == ' ').Count() + indentSize;
                }
                // The line before contains }, we dedent it
                else if (toks.Any(tok => tok.Type == RustLexer.RustLexer.RBRACE))
                {
                    ed.MoveLineUp(false);
                    ed.DecreaseLineIndent();
                    ed.MoveLineDown(false);
                    return Math.Max(0, prevLine.GetText().TakeWhile(c2 => c2 == ' ').Count() - indentSize);
                }
            }
            // otherwise, there are no lines ending in braces before us.
            return null;
        }

        public void Dispose() { }
    }
}
