namespace ArkeIndustries.VisualRust
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

    [Export(typeof(ITaggerProvider))]
    [ContentType("rust")]
    [TagType(typeof(RustTokenTag))]
    internal sealed class RustTokenTagProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new RustTokenTagger(buffer) as ITagger<T>;
        }
    }

    public class RustTokenTag : ITag
    {
        public RustTokenTypes type { get; private set; }
        public RustTokenTag(RustTokenTypes type)
        {
            this.type = type;
        }
    }

    internal sealed class RustTokenTagger : ITagger<RustTokenTag>
    {
        ITextBuffer buffer;

        internal RustTokenTagger(ITextBuffer buffer)
        {
            this.buffer = buffer;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<RustTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan curSpan in spans)
            {
                int curLoc = curSpan.Start.GetContainingLine().Start.Position;
                Debug.WriteLine("TokenTagger: asked to get tags for span " + curSpan.ToString());
                var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curLoc, 3));
                if (tokenSpan.IntersectsWith(curSpan))
                    yield return new TagSpan<RustTokenTag>(tokenSpan,
                                                          new RustTokenTag(RustTokenTypes.COMMENT));
            }

        }
    }
}