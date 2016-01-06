using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using VisualRust.Text;

namespace VisualRust
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType("rust")]
    public class VisualRustSmartIndentProvider : ISmartIndentProvider
    {
        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return new VisualRustSmartIndent(textView);
        }
    }

    // TODO: this indenter should take comments into consideration
    // TODO: this indenter should take tabs into consideration
    class VisualRustSmartIndent : ISmartIndent
    {
        private readonly ITextView textView;
        private readonly DocumentState state;

        internal VisualRustSmartIndent(ITextView textView)
        {
            this.textView = textView;
            textView.TextBuffer.Properties.TryGetProperty(DocumentState.Key, out state);
        }

        int? ISmartIndent.GetDesiredIndentation(ITextSnapshotLine currentSnapshotLine)
        {
            var textSnapshot = textView.TextSnapshot;
            var position = textView.Caret.Position.BufferPosition.Position;

            var indentStep = textView.Options.GetIndentSize();

            var caretLine = textSnapshot.GetLineFromPosition(position);
            var lineRemainder = new Span(position, caretLine.End - position);

            var tokens = state.GetTokens(new Span(0, position));

            var indentStepsCount = 0;
            foreach (var token in tokens)
            {
                // "{"
                if (token.Type == RustLexer.RustLexer.LBRACE)
                {
                    indentStepsCount++;
                }

                // "}"
                if (token.Type == RustLexer.RustLexer.RBRACE && indentStepsCount > 0)
                {
                    indentStepsCount--;
                }

                // "("
                if (token.Type == RustLexer.RustLexer.LPAREN)
                {
                    indentStepsCount++;
                }

                // ")"
                if (token.Type == RustLexer.RustLexer.RPAREN && indentStepsCount > 0)
                {
                    indentStepsCount--;
                }
            }

            var closeBraceAfterCaret = false;
            foreach (var ch in textSnapshot.GetText(lineRemainder))
            {
                if (ch == '}' || ch == ')')
                {
                    closeBraceAfterCaret = true;
                    break;
                }

                if (ch != ' ')
                {
                    break;
                }
            }

            var indention = indentStepsCount * indentStep;
            if (closeBraceAfterCaret)
            {
                indention -= indentStep;
            }


            return indention;
        }

        public void Dispose() { }
    }
}
