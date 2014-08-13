namespace ArkeIndustries.VisualRust
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;
    using Microsoft.VisualStudio.Language.StandardClassification;

    [Export(typeof(ITaggerProvider))]
    [ContentType("rust")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class RustClassifierProvider : ITaggerProvider
    {
        [Export]
        [Name("rust")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition RustContentType = null;

        [Export]
        [FileExtension(".rs")]
        [ContentType("rust")]
        internal static FileExtensionToContentTypeDefinition RustFileType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagAggregator<RustTokenTag> rustTagAgg = aggregatorFactory.CreateTagAggregator<RustTokenTag>(buffer);
            return new RustClassifier(buffer, rustTagAgg, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    internal sealed class RustClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITagAggregator<RustTokenTag> _agg;
        IDictionary<RustTokenTypes, IClassificationType> _rustTypes;

        internal RustClassifier(ITextBuffer buffer, ITagAggregator<RustTokenTag> rustTagAgg, IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _agg = rustTagAgg;
            _rustTypes = new Dictionary<RustTokenTypes, IClassificationType>();
            _rustTypes[RustTokenTypes.COMMENT] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            _rustTypes[RustTokenTypes.DOC_COMMENT] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            _rustTypes[RustTokenTypes.CHAR] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Character);
            _rustTypes[RustTokenTypes.IDENT] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Identifier);
            _rustTypes[RustTokenTypes.LIFETIME] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Identifier);
            _rustTypes[RustTokenTypes.NUMBER] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Number);
            _rustTypes[RustTokenTypes.OP] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Operator);
            _rustTypes[RustTokenTypes.STRING] = typeService.GetClassificationType(PredefinedClassificationTypeNames.String);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return new List<ITagSpan<ClassificationTag>>();
        }
    }
}
