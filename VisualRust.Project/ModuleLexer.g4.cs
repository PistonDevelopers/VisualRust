using Antlr4.Runtime;

namespace VisualRust.Project
{
    partial class ModuleLexer : Lexer
    {
        public ModuleLexer(string input) : this(new AntlrInputStream(input))
        {
            
        }
    }
}
