using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Text
{
    sealed class RustClassifier : ITagger<ClassificationTag>
    {
        static Dictionary<RustTokenTypes, IClassificationType> _rustTypes;
        readonly IStandardClassificationService standardClassificationService;
        readonly ITextBuffer buffer;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public RustClassifier(ITextBuffer buffer, IStandardClassificationService standardClassificationService)
        {
            this.buffer = buffer;
            this.standardClassificationService = standardClassificationService;
            InitializeClassifierDictionary(standardClassificationService);
        }

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

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            DocumentState document;
            if(!DocumentState.TryGet(buffer, out document))
                yield break;
            if(document.Version.Version.VersionNumber != buffer.CurrentSnapshot.Version.VersionNumber)
                yield break;

            foreach(var span in spans)
            {
                foreach(var token in document.GetTokens(span))
                {
                    if(token.IsEmpty)
                        continue;
                    var tag = new ClassificationTag(_rustTypes[Utils.LexerTokenToRustToken(token.GetText(document.Version), token.Type)]);
                    yield return new TagSpan<ClassificationTag>(new SnapshotSpan(document.Version, token.GetSpan(document.Version)), tag);
                }
            }
        }
    }
}
