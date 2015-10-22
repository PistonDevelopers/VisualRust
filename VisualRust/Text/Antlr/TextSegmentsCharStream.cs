using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Text.Antlr
{
    class TextSegmentsCharStream : TextReader 
    {
        private IEnumerator<string> segments;
        int index;
        bool finished;

        public TextSegmentsCharStream(IEnumerable<string> segments)
        {
            this.segments = segments.GetEnumerator();
            this.segments.MoveNext();
        }

        public override int Read()
        {
            if(finished)
                return -1;
            if(index >= segments.Current.Length)
            {
                if(!segments.MoveNext())
                {
                    finished = true;
                    return -1;
                }
                index = 0;
            }
            return segments.Current[index++];
        }

        public override int Peek()
        {
            if(finished)
                return -1;
            return segments.Current[index];
        }
    }
}
