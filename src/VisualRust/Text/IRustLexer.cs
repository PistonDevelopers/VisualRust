using System.Collections.Generic;

namespace VisualRust.Text
{
    public interface IRustLexer
    {
        IEnumerable<SpannedToken> Run(IEnumerable<string> textSegments, int offset);
    }
}