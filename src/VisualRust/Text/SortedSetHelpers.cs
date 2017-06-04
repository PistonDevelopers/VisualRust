using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Text
{
    static class SortedSetHelpers
    {
        internal static List<TrackingToken> GetInvalidatedBy(this SortedSet<TrackingToken> tree, ITextSnapshot version, Span span)
        {
            List<TrackingToken> tokens = new List<TrackingToken>();
            if (tree.Root != null)
            {
                FillInvalidatedTokens(tree.Root, version, span, tokens);
                if (tokens.Count == 0)
                    tokens.Add(tree.Min);
            }
            return tokens;
        }

        internal static IEnumerable<TrackingToken> InOrderAfter(this SortedSet<TrackingToken> tree, ITextSnapshot version, int start)
        {
            // search first
            SortedSet<TrackingToken>.Node current = tree.Root;
            if (tree.Root == null)
                yield break;
            Stack<SortedSet<TrackingToken>.Node> stack = new Stack<SortedSet<TrackingToken>.Node>(2 * (SortedSet<TrackingToken>.log2(tree.Count + 1)));
            // find exact
            while (true)
            {
                if(current == null)
                    yield break;
                Span currentSpan = current.Item.GetSpan(version);
                if (currentSpan.Contains(start))
                {
                    if (currentSpan.Start == start)
                        yield return current.Item;
                    break;
                }
                else if (start < currentSpan.Start)
                {
                    if (current.Left == null)
                    {
                        yield return current.Item;
                        break;
                    }
                    stack.Push(current);
                    current = current.Left;
                }
                else
                {
                    stack.Push(current);
                    current = current.Right;
                }
            }
            // yield next
            while (true)
            {
                current = Next(current, stack);
                if (current == null)
                    break;
                yield return current.Item;
            }
        }

        internal static TrackingToken GetCoveringToken(this SortedSet<TrackingToken> tree, ITextSnapshot version, int pos)
        {
            SortedSet<TrackingToken>.Node current = tree.Root;
            while (current != null)
            {
                Span span = current.Item.GetSpan(version);
                if (span.Contains(pos))
                {
                    return current.Item;
                }
                else
                {
                    current = (pos < span.Start) ? current.Left : current.Right;
                }
            }
            throw new ArgumentOutOfRangeException("pos");
        }

        internal static IEnumerable<TrackingToken> GetCoveringTokens(this SortedSet<TrackingToken> tree, ITextSnapshot version, Span span)
        {
            List<TrackingToken> tokens = new List<TrackingToken>();
            if (tree.Root != null)
                FillCoveringTokens(tree.Root, version, span, tokens);
            return tokens;
        }

        public static void FillCoveringTokens(SortedSet<TrackingToken>.Node current, ITextSnapshot version, Span span, List<TrackingToken> tokens)
        {
            var currentSpan = current.Item.GetSpan(version);
            if (current.Left != null && span.Start < currentSpan.Start)
                FillCoveringTokens(current.Left, version, span, tokens);
            if (currentSpan.OverlapsWith(span))
                tokens.Add(current.Item);
            if (current.Right != null && span.End > currentSpan.End)
                FillCoveringTokens(current.Right, version, span, tokens);
        }

        static void FillInvalidatedTokens(SortedSet<TrackingToken>.Node current, ITextSnapshot version, Span span, List<TrackingToken> tokens)
        {
            var currentSpan = current.Item.GetSpan(version);
            if (current.Left != null && span.Start <= currentSpan.Start)
                FillInvalidatedTokens(current.Left, version, span, tokens);
            if (RightInclusiveOverlap(currentSpan, span))
                tokens.Add(current.Item);
            if (current.Right != null && span.End >= currentSpan.End)
                FillInvalidatedTokens(current.Right, version, span, tokens);
        }

        static bool RightInclusiveOverlap(Span current, Span span)
        {
            if (span.End >= current.End)
                return span.Start <= current.End;
            if (span.Start <= current.Start)
                return span.End > current.Start;
            return true;
        }

        static SortedSet<TrackingToken>.Node Next(SortedSet<TrackingToken>.Node current, Stack<SortedSet<TrackingToken>.Node> parents)
        {
            if (current.Right != null)
            {
                parents.Push(current);
                return Minimum(current.Right, parents);
            }
            return PopUntilLeftChild(current, parents);
        }

        static SortedSet<TrackingToken>.Node Minimum(SortedSet<TrackingToken>.Node current, Stack<SortedSet<TrackingToken>.Node> parents)
        {
            while (current.Left != null)
            {
                parents.Push(current);
                current = current.Left;
            }
            return current;
        }

        static SortedSet<TrackingToken>.Node PopUntilLeftChild(SortedSet<TrackingToken>.Node current, Stack<SortedSet<TrackingToken>.Node> parents)
        {
            while (parents.Count > 0)
            {
                var parent = parents.Pop();
                if (current == parent.Left)
                    return parent;
                current = parent;
            }
            return null;
        }
    }
}
