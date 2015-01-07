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

        private static readonly Dictionary<int, RustTokenTypes> _rustTokenTypesDictionary;

        static Utils()
        {
            _rustTokenTypesDictionary = new Dictionary<int, RustTokenTypes>();
            _rustTokenTypesDictionary[RustLexer.EQ] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.LT] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.LE] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.EQEQ] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.NE] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.GE] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.GT] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.ANDAND] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.OROR] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.NOT] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.TILDE] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.PLUS] = RustTokenTypes.OP;

            _rustTokenTypesDictionary[RustLexer.MINUS] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.STAR] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.SLASH] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.PERCENT] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.CARET] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.AND] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.OR] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.SHL] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.SHR] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.BINOP] = RustTokenTypes.OP;

            _rustTokenTypesDictionary[RustLexer.BINOPEQ] = RustTokenTypes.OP;
            _rustTokenTypesDictionary[RustLexer.AT] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.DOT] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.DOTDOT] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.DOTDOTDOT] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.COMMA] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.SEMI] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.COLON] = RustTokenTypes.STRUCTURAL;

            _rustTokenTypesDictionary[RustLexer.MOD_SEP] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.RARROW] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.FAT_ARROW] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.LPAREN] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.RPAREN] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.LBRACKET] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.RBRACKET] = RustTokenTypes.STRUCTURAL;

            _rustTokenTypesDictionary[RustLexer.LBRACE] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.RBRACE] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.POUND] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.DOLLAR] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.UNDERSCORE] = RustTokenTypes.STRUCTURAL;
            _rustTokenTypesDictionary[RustLexer.LIT_CHAR] = RustTokenTypes.CHAR;

            _rustTokenTypesDictionary[RustLexer.LIT_INTEGER] = RustTokenTypes.NUMBER;
            _rustTokenTypesDictionary[RustLexer.LIT_FLOAT] = RustTokenTypes.NUMBER;
            _rustTokenTypesDictionary[RustLexer.LIT_STR] = RustTokenTypes.STRING;
            _rustTokenTypesDictionary[RustLexer.LIT_STR_RAW] = RustTokenTypes.STRING;
            _rustTokenTypesDictionary[RustLexer.LIT_BINARY] = RustTokenTypes.STRING;

            _rustTokenTypesDictionary[RustLexer.LIT_BINARY_RAW] = RustTokenTypes.STRING;
            _rustTokenTypesDictionary[RustLexer.IDENT] = RustTokenTypes.IDENT;
            _rustTokenTypesDictionary[RustLexer.LIFETIME] = RustTokenTypes.LIFETIME;
            _rustTokenTypesDictionary[RustLexer.WHITESPACE] = RustTokenTypes.WHITESPACE;
            _rustTokenTypesDictionary[RustLexer.DOC_COMMENT] = RustTokenTypes.DOC_COMMENT;
            _rustTokenTypesDictionary[RustLexer.COMMENT] = RustTokenTypes.COMMENT;            
        }

        public static RustTokenTypes GetRustTokenType(this Antlr4.Runtime.IToken token)
        {
            return LexerTokenToRustToken(token.Text, token.Type);
        }

        public static RustTokenTypes LexerTokenToRustToken(string text, int tok)
        {
            RustTokenTypes ty = _rustTokenTypesDictionary[tok];
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
