namespace RustLexer
{
    using Antlr4.Runtime;
    partial class RustLexer : Lexer
    {
        public RustLexer(string input) : this(new AntlrInputStream(input))
        {
            
        }
        
        private bool isAt(int pos) {
            return _input.Index == pos;
        }
    }
}
