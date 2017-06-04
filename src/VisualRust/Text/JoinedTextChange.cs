using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Text
{
    class JoinedTextChange : ITextChange
    {
        Span oldS;
        int delta;

        public JoinedTextChange(INormalizedTextChangeCollection changes)
        {
            int oldStart = changes[0].OldSpan.Start;
            int oldEnd = changes[changes.Count - 1].OldEnd;
            oldS = new Span(oldStart, oldEnd);
            delta = changes[changes.Count - 1].NewEnd - changes[changes.Count - 1].OldEnd;
        }

        public Span OldSpan { get { return oldS; } }
        public int Delta { get { return delta; } }

        public int LineCountDelta { get { throw new NotImplementedException(); } }
        public int NewEnd { get { throw new NotImplementedException(); } }
        public int NewLength { get { throw new NotImplementedException(); } }
        public int NewPosition { get { throw new NotImplementedException(); } }
        public Span NewSpan { get { throw new NotImplementedException(); } }
        public string NewText { get { throw new NotImplementedException(); } }
        public int OldEnd { get { throw new NotImplementedException(); } }
        public int OldLength { get { throw new NotImplementedException(); } }
        public int OldPosition { get { throw new NotImplementedException(); } }
        public string OldText { get { throw new NotImplementedException(); } }
    }
}
