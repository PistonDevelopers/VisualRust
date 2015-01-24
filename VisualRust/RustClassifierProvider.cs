namespace VisualRust
{
    using System;
    using System.Diagnostics;
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
    public sealed class RustClassifierProvider : ITaggerProvider
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
        internal IStandardClassificationService StandardClassificationService = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new RustClassifier(buffer, StandardClassificationService) as ITagger<T>;
        }
    }

}
