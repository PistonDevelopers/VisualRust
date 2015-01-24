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
    using RustLexer;
    sealed class RustClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
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
        }

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

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan curSpan in spans)
            {
                var containingLine = curSpan.Start.GetContainingLine();
                var lexer = new RustLexer(curSpan.Start.GetContainingLine().GetText());
                int curLoc = containingLine.Start.Position;

                while (true)
                {
                    var tok = lexer.NextToken();
                    if (tok.Type == RustLexer.Eof)
                    {
                        yield break;
                    }
                    var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curLoc + tok.StartIndex, tok.Text.Length));
                    yield return new TagSpan<ClassificationTag>(tokenSpan, new ClassificationTag(_rustTypes[Utils.LexerTokenToRustToken(tok.Text, tok.Type)]));
                }
            }
        }
    }
}
