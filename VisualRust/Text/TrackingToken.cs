using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Text
{
    public struct TrackingToken
    {
        public class NonOverlappingComparer : IComparer<TrackingToken>
        {
            public ITextSnapshot Version;

            public int Compare(TrackingToken x, TrackingToken y)
            {
                int result = x.Start.GetPosition(Version).CompareTo(y.Start.GetPosition(Version));
#if DEBUG
                if(result == 0)
                    System.Diagnostics.Debug.Assert(x.Length == y.Length);
#endif
                return result;
            }
        }

        public ITrackingPoint Start;
        public int Length;
        public int Type;

        public bool IsEmpty { get { return Start == null; } }
        public static TrackingToken Empty { get { return new TrackingToken(); } }

        internal TrackingToken(ITextSnapshot snapshot, SpannedToken arg) : this()
        {
            Start = snapshot.CreateTrackingPoint(arg.Span.Start, PointTrackingMode.Positive);
            Length = arg.Span.Length;
            Type = arg.Type;
        }

        public Span GetSpan(ITextSnapshot snap)
        {
            return new Span(Start.GetPosition(snap), Length);
        }

        public int GetStart(ITextSnapshot snap)
        {
            return Start.GetPosition(snap);
        }

        public int GetEnd(ITextSnapshot snap)
        {
            return Start.GetPosition(snap) + Length;
        }

        public string GetText(ITextSnapshot snap)
        {
            return snap.GetText(GetSpan(snap));
        }
    }
}
