using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace VisualRust.Text
{
    class DocumentState
    {
        readonly TrackingToken.NonOverlappingComparer comparer = new TrackingToken.NonOverlappingComparer();
        SortedSet<TrackingToken> tree;
        readonly IRustLexer lexer;

        public DocumentState(IRustLexer lexer, ITextSnapshot snapshot)
        {
            this.lexer = lexer;
            comparer.Version = snapshot;
            Initialize();
        }

        private void Initialize()
        {
            tree = new SortedSet<TrackingToken>(lexer.Run(new string[] { comparer.Version.GetText() }, 0).Select(t => new TrackingToken(comparer.Version, t)), comparer);
        }

        public IList<TrackingToken> GetInvalidated(ITextSnapshot oldSnapshot, ITextChange change)
        {
            return tree.CoveringTokens(oldSnapshot, change.OldSpan);
        }

        public IList<TrackingToken> Rescan(ITextSnapshot oldSnapshot, IList<TrackingToken> invalid, int delta)
        {
            Span invalidatedSpan = InvalidatedSpan(oldSnapshot, invalid, delta);
            string invalidatedText = comparer.Version.GetText(invalidatedSpan);
            return RescanCore(invalidatedSpan, invalidatedText);
        }

        private IList<TrackingToken> RescanCore(Span invalidatedSpan, string invalidatedText)
        {
            int end = 0;
            List<TrackingToken> rescanned = new List<TrackingToken>();
            // this lazy iterator walks tokens that are outside of the initial invalidation span
            var excessText = tree.InOrderAfter(comparer.Version, invalidatedSpan.End).Select(t => GetTextAndMarkEnd(t, ref end));
            var tokens = lexer.Run(new string[] { invalidatedText }.Concat(excessText), invalidatedSpan.Start);
            foreach (var token in tokens)
            {
                rescanned.Add(new TrackingToken(comparer.Version, token));
                if (token.Span.End == invalidatedSpan.End || token.Span.End == end)
                    break;
            }
            return rescanned;
        }

        private string GetTextAndMarkEnd(TrackingToken current, ref int end)
        {
            end = current.GetEnd(comparer.Version);
            return current.GetText(comparer.Version);
        }

        internal IEnumerable<TrackingToken> GetTokens(SnapshotSpan span)
        {
            return tree.CoveringTokens(Version, span);
        }

        private Span InvalidatedSpan(ITextSnapshot oldSnapshot, IList<TrackingToken> invalid, int delta)
        {
            int invalidationStart = invalid[0].GetStart(oldSnapshot); // this position is the same in both versions
            int invalidationEnd = GetInvalidationEnd(oldSnapshot, invalid, delta);
            return new Span(invalidationStart, invalidationEnd - invalidationStart);
        }

        private int GetInvalidationEnd(ITextSnapshot oldSnapshot, IList<TrackingToken> invalid, int delta)
        {
            var oldSpan = invalid[invalid.Count - 1].GetSpan(oldSnapshot);
            var newSpan = invalid[invalid.Count - 1].GetSpan(comparer.Version);
            var tokenStartDelta = newSpan.Start - oldSpan.Start;
            return newSpan.End + delta - tokenStartDelta;
        }

        public void ApplyVersion(ITextSnapshot currentSnapshot)
        {
            comparer.Version = currentSnapshot;
        }

        public ITextSnapshot Version { get { return comparer.Version; } }

        public void TextChanged(TextContentChangedEventArgs args)
        {
            foreach(var change in args.Changes)
            {
                var invalidated = GetInvalidated(args.Before, change);
                ApplyVersion(args.After);
                var updated = Rescan(args.Before, invalidated, change.Delta);
                foreach(var token in invalidated)
                    tree.Remove(token);
                foreach(var token in updated)
                    tree.Add(token);
            }
        }

        public static bool TryGet(ITextBuffer b, out DocumentState t)
        {
            TextBufferListener.PropertyEntry entry;
            if(!b.Properties.TryGetProperty<TextBufferListener.PropertyEntry>(TextBufferListener.PropertyEntry.Key, out entry))
            {
                t = null;
                return false;
            }
            t = entry.Tokenizer;
            return true;
        }
    }
}
