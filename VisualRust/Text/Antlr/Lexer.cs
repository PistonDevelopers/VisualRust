using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;

namespace VisualRust.Text.Antlr
{
    [Export(typeof(IRustLexer))]
    public class Lexer : IRustLexer
    {
        public IEnumerable<SpannedToken> Run(IEnumerable<string> segments, int offset)
        {
            var lexer = new RustLexer.RustLexer(new AntlrInputStream(new TextSegmentsCharStream(segments)));
            while(true)
            {
                IToken current = lexer.NextToken();
                if(current.Type == RustLexer.RustLexer.Eof)
                    break;
                yield return new SpannedToken(current.Type, new Span(current.StartIndex + offset, current.StopIndex - current.StartIndex + 1));
            }
        }
    }
}
