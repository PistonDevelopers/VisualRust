using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Antlr4.Runtime;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Formatting;

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

        int? ISmartIndent.GetDesiredIndentation(ITextSnapshotLine currentSnapshotLine)
        {
            var textView = _textView;
            var textSnapshot = textView.TextSnapshot;
            var caret = textView.Caret;
            var currentLine = caret.ContainingTextViewLine;
            var currentLineStartPosition = currentLine.Start.Position;
            var indentStep = _textView.Options.GetIndentSize();

            var tokens = Utils.LexString(textSnapshot.GetText()).ToArray();
            var currentTokenIndex = GetTokensArrayIndexForPosition(tokens, currentLineStartPosition);
            if (currentTokenIndex < 0)
            {
                return null;
            }

            var tokenForCaretPosition = tokens[currentTokenIndex];
            
            if (tokenForCaretPosition.Type == RustLexer.RustLexer.COMMENT || tokenForCaretPosition.Type == RustLexer.RustLexer.DOC_COMMENT)
            {
                // for comment return comment start position
                var startPositionInLine = GetStartTokenPositionInLine(tokenForCaretPosition, textSnapshot);

                return startPositionInLine;
            }

            var bracketChecker = new Stack<IToken>();

            for (int tokenIndex = currentTokenIndex; tokenIndex >= 0; tokenIndex--)
            {
                var token = tokens[tokenIndex];
                var firstTokenLetter = GetFirstTokenLetter(token);
                switch (firstTokenLetter)
                {
                    case '{':
                        if (bracketChecker.Any())
                        {
                            var lastBracket = bracketChecker.Pop();
                            if (lastBracket.Text[0] != '}')
                            {
                                return null;
                            }
                        }
                        else
                        {
                            var result = GetStartTokenPositionInLine(token, textSnapshot) + indentStep;
                            return result;
                        }

                        break;
                    case '(':
                        if (bracketChecker.Any())
                        {
                            var lastBracket = bracketChecker.Pop();
                            if (lastBracket.Text[0] != ')')
                            {
                                return null;
                            }
                        }
                        else
                        {
                            var result = GetStartTokenPositionInLine(token, textSnapshot) + indentStep;
                            return result;
                        }

                        break;
                    case '}':
                    case ')':
                        bracketChecker.Push(token);
                        break;
                }
            }

            if (tokens[0].Type == RustLexer.RustLexer.WHITESPACE && tokens.Length >= 2)
            {
                return GetStartTokenPositionInLine(tokens[1], textSnapshot);
            }

            return 0;
        }

        private static char GetFirstTokenLetter(IToken token)
        {
            var text = token.Text;
            if (text.Length >= 1)
            {
                return text[0];                
            }

            // returning '\n' becouse it doesn't matter in this context
            return '\n';
        }

        private static int GetStartTokenPositionInLine(IToken token, ITextSnapshot textSnapshot)
        {
            // lexer line number = textSnapshot line number + 1
            var startBlockLine = textSnapshot.Lines.ElementAt(token.Line - 1);
            var startPositionInLine = GetTextPositionInLine(startBlockLine.GetText());

            return startPositionInLine;
        }

        private static int GetTextPositionInLine(string lineText)
        {
            for (int index = 0; index < lineText.Length; index++)
            {
                var symbol = lineText[index];
                if (char.IsWhiteSpace(symbol) || char.IsControl(symbol))
                {
                    continue;
                }

                return index;
            }

            throw new Exception("I'm sorry. An internal error in indention calculation algorithm.");
        }

        /// <summary>
        /// Get index of token in array of tokens for specifying position in source text
        /// </summary>
        /// <returns>
        /// Index in tokens array or -1 if not found
        /// </returns>
        private static int GetTokensArrayIndexForPosition(IToken[] tokens, int caretPosition)
        {
            var searchingPosition = caretPosition - 1;

            for (int index = 0; index < tokens.Length; index++)
            {
                var token = tokens[index];
                if (token.StartIndex <= searchingPosition && searchingPosition <= token.StopIndex)
                {
                    if (token.Type == RustLexer.RustLexer.WHITESPACE)
                    {
                        return index - 1;
                    }

                    return index;
                }
            }

            return -1;
        }

        public void Dispose() { }
    }
}
