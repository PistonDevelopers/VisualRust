using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust
{
    using Antlr4.Runtime;
    using RustLexer;
    sealed class RustClassifier : ITagger<ClassificationTag>
    {
        static Dictionary<RustTokenTypes, IClassificationType> _rustTypes;

        void InitializeClassifierDictionary(IStandardClassificationService typeService)
        {
            if(_rustTypes != null)
                return;
            _rustTypes = new Dictionary<RustTokenTypes, IClassificationType>();
            _rustTypes[RustTokenTypes.COMMENT] = typeService.Comment;
            _rustTypes[RustTokenTypes.DOC_COMMENT] = typeService.Comment;
            _rustTypes[RustTokenTypes.CHAR] = typeService.CharacterLiteral;
            _rustTypes[RustTokenTypes.IDENT] = typeService.Identifier;
            _rustTypes[RustTokenTypes.LIFETIME] = typeService.Identifier;
            _rustTypes[RustTokenTypes.NUMBER] = typeService.NumberLiteral;
            _rustTypes[RustTokenTypes.OP] = typeService.Operator;
            _rustTypes[RustTokenTypes.STRING] = typeService.StringLiteral;
            _rustTypes[RustTokenTypes.STRUCTURAL] = typeService.FormalLanguage;
            _rustTypes[RustTokenTypes.WHITESPACE] = typeService.WhiteSpace;
            _rustTypes[RustTokenTypes.KEYWORD] = typeService.Keyword;
            _rustTypes[RustTokenTypes.PRIMITIVE_TYPE] = typeService.Keyword;
            _rustTypes[RustTokenTypes.TYPE] = typeService.SymbolDefinition;
        }

        ITextBuffer _buffer;
        /*
         * Vs calls our tagger line-by-line. This breaks multiline tokens.
         * As a work-around for this we keep a dictionary of lines
         * with that problem and fix it up when called by GetTags.
         */
        private Dictionary<int, int> multilineTokens = new Dictionary<int, int>();

        public RustClassifier(ITextBuffer buffer, IStandardClassificationService standardClassificationService)
        {
            InitializeClassifierDictionary(standardClassificationService);
            _buffer = buffer;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private void OnTagsChanged(SnapshotSpan sp)
        {
            var temp = TagsChanged;
            if(temp != null)
                temp(this, new SnapshotSpanEventArgs(sp));
        }

        private RustLexer InitLexer(SnapshotSpan span, out int prefixSize)
        {
            ITextSnapshotLine containingLine = span.Start.GetContainingLine();
            int continuingToken;
            string lineText;
            if(multilineTokens.TryGetValue(containingLine.LineNumber - 1, out continuingToken))
            {
                if(continuingToken == RustLexer.BLOCK_COMMENT)
                    lineText = "/*" + containingLine.GetText();
                else if(continuingToken == RustLexer.LIT_STR)
                    lineText = "\" " + containingLine.GetText();
                else
                    throw new InvalidOperationException();
                prefixSize = 2;
            }
            else
            {
                continuingToken = RustLexer.Eof;
                lineText = containingLine.GetText();
                prefixSize = 0;
            }
            return new RustLexer(lineText);
        }

        private void UpdateMultilineDict(SnapshotSpan curSpan, int lastType, int newType)
        {
            int currentLineNumber =  curSpan.Snapshot.GetLineNumberFromPosition(curSpan.Start);
            int multilineEnding;
            if (!multilineTokens.TryGetValue(currentLineNumber, out multilineEnding) || multilineEnding != lastType)
            {
                multilineTokens[currentLineNumber] = newType;
                if (curSpan.End.Position < curSpan.Snapshot.Length)
                {
                    OnTagsChanged(new SnapshotSpan(curSpan.Snapshot, curSpan.End, 1));
                }
            }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan curSpan in spans)
            {
                int multilinePrefixSize;
                RustLexer lexer = InitLexer(curSpan, out multilinePrefixSize);

                IToken lastToken = null;
                for (int i = 0; true; i++)
                {
                    int lastType = lastToken != null ? lastToken.Type : RustLexer.Eof;
                    IToken token = lexer.NextToken();
                    if (token.Type == RustLexer.Eof)
                    {
                        if((lastType == RustLexer.BLOCK_COMMENT || lastType == RustLexer.DOC_BLOCK_COMMENT)
                            && !lastToken.Text.EndsWith("*/"))
                        {
                            UpdateMultilineDict(curSpan, lastType, RustLexer.BLOCK_COMMENT);
                        }
                        else if((lastType == RustLexer.LIT_STR || lastType == RustLexer.LIT_STR_RAW
                            || lastType == RustLexer.LIT_BINARY || lastType == RustLexer.LIT_BINARY_RAW)
                            && !lastToken.Text.EndsWith("\""))
                        {
                            UpdateMultilineDict(curSpan, lastType, RustLexer.LIT_STR);
                        }
                        else if (multilineTokens.Remove(curSpan.Snapshot.GetLineNumberFromPosition(curSpan.Start)) && curSpan.End.Position < curSpan.Snapshot.Length)
                        {
                            OnTagsChanged(new SnapshotSpan(curSpan.Snapshot, curSpan.End, 1));
                        }
                        yield break;
                    }
                    lastToken = token;

                    int realStart;
                    int realTokenLength;
                    if(i == 0)
                    {
                        realStart = curSpan.Start.Position + token.StartIndex;
                        realTokenLength = token.Text.Length - multilinePrefixSize;
                    }
                    else
                    {
                        realStart = curSpan.Start.Position + token.StartIndex - multilinePrefixSize;
                        realTokenLength = token.Text.Length;
                    }
                    var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(realStart, realTokenLength));
                    yield return new TagSpan<ClassificationTag>(tokenSpan, new ClassificationTag(_rustTypes[Utils.LexerTokenToRustToken(token.Text, token.Type)]));
                }
            }
        }
    }
}
