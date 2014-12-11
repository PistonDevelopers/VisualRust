using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Operations;

namespace VisualRust
{
    using RustLexer;

    static class Utils
    {

        // These keywords are from rustc /src/libsyntax/parse/token.rs, module keywords
        private static readonly HashSet<string> _kws = new HashSet<string> {
                "as",
                "box",
                "break",
                "const",
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
                "move",
                "mut",
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
                "virtual",
                "where",
                "while"
            };

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

        public static IEnumerable<string> Keywords
        {
            get { return _kws; }
        }
        
        [Conditional("DEBUG")]
        internal static void DebugPrintToOutput(string s, params object[] args)
        {        
            PrintToOutput("[DEBUG] "+s, args);
        }

        internal static void PrintToOutput(string s, params object[] args)
        {
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Guid paneGuid = VSConstants.GUID_OutWindowGeneralPane;
            IVsOutputWindowPane pane;
            ErrorHandler.ThrowOnFailure(outWindow.CreatePane(paneGuid, "General", 1, 0));            
            outWindow.GetPane(ref paneGuid, out pane);
            pane.OutputString(string.Format("[VisualRust]: " + s, args) + "\n");
            pane.Activate();
        }
    }

    public class TemporaryFile : IDisposable
    {
        public string Path { get; private set; }

        /// <summary>
        ///  Creates a new, empty temporary file
        /// </summary>
        public TemporaryFile()
        {
            Path = System.IO.Path.GetTempFileName();            
        }

        /// <summary>
        ///  Creates a new temporary file with the argument string as UTF-8 content.
        /// </summary>
        public TemporaryFile(string content)
            : this()
        {
            using (var sw = new StreamWriter(Path, false, new UTF8Encoding(false)))
            {
                sw.Write(content);
            }
        }

        public void Dispose()
        {
            if (!File.Exists(Path))                      
                File.Delete(Path);            
        }
    }

}
