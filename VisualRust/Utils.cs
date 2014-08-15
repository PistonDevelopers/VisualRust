using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Operations;

namespace ArkeIndustries.VisualRust
{
    using RustLexer;

    class Utils
    {
        [Import]
        public static IEditorOperationsFactoryService editorOpFactory = null;

        public static RustTokenTypes LexerTokenToRustToken(string text, int tok)
        {
            // FIXME: this is super slow.
            var _tt = new Dictionary<int, RustTokenTypes>();
            _tt[RustLexer.EQ] = RustTokenTypes.OP;
            _tt[RustLexer.LT] = RustTokenTypes.OP;
            _tt[RustLexer.LE] = RustTokenTypes.OP;
            _tt[RustLexer.EQEQ] = RustTokenTypes.OP;
            _tt[RustLexer.NE] = RustTokenTypes.OP;
            _tt[RustLexer.GE] = RustTokenTypes.OP;
            _tt[RustLexer.GT] = RustTokenTypes.OP;
            _tt[RustLexer.ANDAND] = RustTokenTypes.OP;
            _tt[RustLexer.OROR] = RustTokenTypes.OP;
            _tt[RustLexer.NOT] = RustTokenTypes.OP;
            _tt[RustLexer.TILDE] = RustTokenTypes.OP;
            _tt[RustLexer.PLUS] = RustTokenTypes.OP;

            _tt[RustLexer.MINUS] = RustTokenTypes.OP;
            _tt[RustLexer.STAR] = RustTokenTypes.OP;
            _tt[RustLexer.SLASH] = RustTokenTypes.OP;
            _tt[RustLexer.PERCENT] = RustTokenTypes.OP;
            _tt[RustLexer.CARET] = RustTokenTypes.OP;
            _tt[RustLexer.AND] = RustTokenTypes.OP;
            _tt[RustLexer.OR] = RustTokenTypes.OP;
            _tt[RustLexer.SHL] = RustTokenTypes.OP;
            _tt[RustLexer.SHR] = RustTokenTypes.OP;
            _tt[RustLexer.BINOP] = RustTokenTypes.OP;

            _tt[RustLexer.BINOPEQ] = RustTokenTypes.OP;
            _tt[RustLexer.AT] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.DOT] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.DOTDOT] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.DOTDOTDOT] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.COMMA] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.SEMI] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.COLON] = RustTokenTypes.STRUCTURAL;

            _tt[RustLexer.MOD_SEP] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.RARROW] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.FAT_ARROW] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.LPAREN] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.RPAREN] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.LBRACKET] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.RBRACKET] = RustTokenTypes.STRUCTURAL;

            _tt[RustLexer.LBRACE] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.RBRACE] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.POUND] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.DOLLAR] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.UNDERSCORE] = RustTokenTypes.STRUCTURAL;
            _tt[RustLexer.LIT_CHAR] = RustTokenTypes.CHAR;

            _tt[RustLexer.LIT_INTEGER] = RustTokenTypes.NUMBER;
            _tt[RustLexer.LIT_FLOAT] = RustTokenTypes.NUMBER;
            _tt[RustLexer.LIT_STR] = RustTokenTypes.STRING;
            _tt[RustLexer.LIT_STR_RAW] = RustTokenTypes.STRING;
            _tt[RustLexer.LIT_BINARY] = RustTokenTypes.STRING;

            _tt[RustLexer.LIT_BINARY_RAW] = RustTokenTypes.STRING;
            _tt[RustLexer.IDENT] = RustTokenTypes.IDENT;
            _tt[RustLexer.LIFETIME] = RustTokenTypes.LIFETIME;
            _tt[RustLexer.WHITESPACE] = RustTokenTypes.WHITESPACE;
            _tt[RustLexer.DOC_COMMENT] = RustTokenTypes.DOC_COMMENT;
            _tt[RustLexer.COMMENT] = RustTokenTypes.COMMENT;

            var _kws = new HashSet<string> {
                "as",
                "box",
                "break",
                "continue",
                "crate",
                "else",
                "enum",
                "extern",
                "false",
                "fn",
                "for",
                "if",
                "impl",
                "in",
                "let",
                "loop",
                "match",
                "mod",
                "mut",
                "priv",
                "proc",
                "pub",
                "ref",
                "return",
                "self",
                "static",
                "struct",
                "super",
                "true",
                "trait",
                "type",
                "unsafe",
                "use",
                "while"
            };

            RustTokenTypes ty = _tt[tok];
            if (ty == RustTokenTypes.IDENT)
            {
                if (_kws.Contains(text))
                {
                    ty = RustTokenTypes.KEYWORD;
                }
                else
                {
                    ty = RustTokenTypes.IDENT;
                }
            }


            return ty;
        }

        public static IEnumerable<Antlr4.Runtime.IToken> LexString(string text)
        {
            var lexer = new RustLexer(text);
            while (true)
            {
                var tok = lexer.NextToken();
                if (tok.Type == RustLexer.Eof)
                {
                    yield break;
                }
                else
                {
                    yield return tok;
                }
            }
        }
    }
}
