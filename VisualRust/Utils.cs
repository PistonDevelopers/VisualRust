﻿using System;
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
    using Microsoft.VisualStudio.Text;
    using Antlr4.Runtime;
    using System.Runtime.InteropServices;
    using EnvDTE;

    static class Utils
    {
        private static Dictionary<int, RustTokenTypes> _tt = new Dictionary<int, RustTokenTypes>()
        {
            { RustLexer.EQ, RustTokenTypes.OP },
            { RustLexer.LT, RustTokenTypes.OP },
            { RustLexer.LE, RustTokenTypes.OP },
            { RustLexer.EQEQ, RustTokenTypes.OP },
            { RustLexer.NE, RustTokenTypes.OP },
            { RustLexer.GE, RustTokenTypes.OP },
            { RustLexer.GT, RustTokenTypes.OP },
            { RustLexer.ANDAND, RustTokenTypes.OP },
            { RustLexer.OROR, RustTokenTypes.OP },
            { RustLexer.NOT, RustTokenTypes.OP },
            { RustLexer.TILDE, RustTokenTypes.OP },
            { RustLexer.PLUS, RustTokenTypes.OP },
            { RustLexer.MINUS, RustTokenTypes.OP },
            { RustLexer.STAR, RustTokenTypes.OP },
            { RustLexer.SLASH, RustTokenTypes.OP },
            { RustLexer.PERCENT, RustTokenTypes.OP },
            { RustLexer.CARET, RustTokenTypes.OP },
            { RustLexer.AND, RustTokenTypes.OP },
            { RustLexer.OR, RustTokenTypes.OP },
            { RustLexer.SHL, RustTokenTypes.OP },
            { RustLexer.SHR, RustTokenTypes.OP },
            { RustLexer.BINOP, RustTokenTypes.OP },
            { RustLexer.BINOPEQ, RustTokenTypes.OP },
            { RustLexer.LARROW, RustTokenTypes.OP },

            { RustLexer.AT, RustTokenTypes.STRUCTURAL },
            { RustLexer.DOT, RustTokenTypes.STRUCTURAL },
            { RustLexer.DOTDOT, RustTokenTypes.STRUCTURAL },
            { RustLexer.DOTDOTDOT, RustTokenTypes.STRUCTURAL },
            { RustLexer.COMMA, RustTokenTypes.STRUCTURAL },
            { RustLexer.SEMI, RustTokenTypes.STRUCTURAL },
            { RustLexer.COLON, RustTokenTypes.STRUCTURAL },
            { RustLexer.MOD_SEP, RustTokenTypes.STRUCTURAL },
            { RustLexer.RARROW, RustTokenTypes.STRUCTURAL },
            { RustLexer.FAT_ARROW, RustTokenTypes.STRUCTURAL },
            { RustLexer.LPAREN, RustTokenTypes.STRUCTURAL },
            { RustLexer.RPAREN, RustTokenTypes.STRUCTURAL },
            { RustLexer.LBRACKET, RustTokenTypes.STRUCTURAL },
            { RustLexer.RBRACKET, RustTokenTypes.STRUCTURAL },
            { RustLexer.LBRACE, RustTokenTypes.STRUCTURAL },
            { RustLexer.RBRACE, RustTokenTypes.STRUCTURAL },
            { RustLexer.POUND, RustTokenTypes.STRUCTURAL },
            { RustLexer.DOLLAR, RustTokenTypes.STRUCTURAL },
            { RustLexer.UNDERSCORE, RustTokenTypes.STRUCTURAL },
            { RustLexer.LIT_CHAR, RustTokenTypes.CHAR },
            { RustLexer.LIT_BYTE, RustTokenTypes.STRING },

            { RustLexer.LIT_INTEGER, RustTokenTypes.NUMBER },
            { RustLexer.LIT_FLOAT, RustTokenTypes.NUMBER },
            { RustLexer.LIT_STR, RustTokenTypes.STRING },
            { RustLexer.LIT_STR_RAW, RustTokenTypes.STRING },
            { RustLexer.LIT_BYTE_STR, RustTokenTypes.STRING },
            { RustLexer.LIT_BYTE_STR_RAW, RustTokenTypes.STRING },
            { RustLexer.QUESTION, RustTokenTypes.OP },
            
            { RustLexer.IDENT, RustTokenTypes.IDENT },
            { RustLexer.LIFETIME, RustTokenTypes.LIFETIME },
            { RustLexer.WHITESPACE, RustTokenTypes.WHITESPACE },
            { RustLexer.DOC_COMMENT, RustTokenTypes.DOC_COMMENT },
            { RustLexer.COMMENT, RustTokenTypes.COMMENT },
            { RustLexer.SHEBANG, RustTokenTypes.COMMENT },
            { RustLexer.UTF8_BOM, RustTokenTypes.WHITESPACE }
        };

        // These keywords are from rustc /src/libsyntax/parse/token.rs, module keywords
        private static readonly HashSet<string> _kws = new HashSet<string> {
            "abstract",
            "alignof",
            "as",
            "become",
            "box",
            "break",
            "const",
            "continue",
            "crate",
            "do",
            "else",
            "enum",
            "extern",
            "false",
            "final",
            "fn",
            "for",
            "if",
            "impl",
            "in",
            "let",
            "loop",
            "macro",
            "match",
            "mod",
            "move",
            "mut",
            "offsetof",
            "override",
            "priv",
            "proc",
            "pub",
            "pure",
            "ref",
            "return",
            "Self",
            "self",
            "sizeof",
            "static",
            "struct",
            "super",
            "trait",
            "true",
            "type",
            "typeof",
            "unsafe",
            "unsized",
            "use",
            "virtual",
            "where",
            "while",
            "yield"
        };

        private static readonly HashSet<string> primitiveTypes = new HashSet<string> {
            // primitives
            "bool",
            "char",
            "u8",
            "i8",
            "u16",
            "i16",
            "u32",
            "i32",
            "u64",
            "i64",
            "f32",
            "f64",
            "isize",
            "usize",
            "str"
        };

        private static readonly HashSet<string> wellKnownTypes = new HashSet<string> {
            "Copy",
            "Send",
            "Sized",
            "Sync",
            "Drop",
            "Fn",
            "FnMut",
            "FnOnce",
            "Box",
            "ToOwned",
            "Clone",
            "PartialEq",
            "PartialOrd",
            "Eq",
            "Ord",
            "AsRef",
            "Into",
            "From",
            "Default",
            "Iterator",
            "Extend",
            "IntoIterator",
            "DoubleEndedIterator",
            "ExactSizeIterator",
            "Option",
            "Some",
            "Result",
            "None",
            "Ok",
            "Err",
            "SliceConcatExt",
            "String",
            "ToString",
            "Vec",
        };

        public static RustTokenTypes LexerTokenToRustToken(string text, int tok)
        {
            RustTokenTypes ty = _tt[tok];
            if (ty == RustTokenTypes.IDENT)
            {
                if (_kws.Contains(text))
                {
                    ty = RustTokenTypes.KEYWORD;
                }
                else if(primitiveTypes.Contains(text))
                {
                    ty = RustTokenTypes.PRIMITIVE_TYPE;
                }
                else if(wellKnownTypes.Contains(text))
                {
                    ty = RustTokenTypes.TYPE;
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
            PrintToOutput("[DEBUG] " + s + "\n", args);
        }

        internal static void PrintToOutput(string s, params object[] args)
        {
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Guid paneGuid = VSConstants.GUID_OutWindowGeneralPane;
            IVsOutputWindowPane pane;
            ErrorHandler.ThrowOnFailure(outWindow.CreatePane(paneGuid, "General", 1, 0));            
            outWindow.GetPane(ref paneGuid, out pane);
            pane.OutputString(string.Format("[Visual Rust]: " + s, args));
            pane.Activate();
        }

        internal static Tuple<IToken, IToken> GetTokensAtPosition(SnapshotPoint snapshotPoint)
        {
            var line = snapshotPoint.GetContainingLine();
            var tokens = Utils.LexString(line.GetText()).ToList();

            if (tokens.Count == 0)
                return Tuple.Create<IToken, IToken>(null, null);

            int col = snapshotPoint.Position - line.Start.Position;

            IToken leftToken;
            IToken currentToken = tokens.FirstOrDefault(t => col > t.StartIndex && col <= t.StopIndex);

            if (currentToken != null)
            {
                if (currentToken == tokens.First())
                    leftToken = null;
                else
                    leftToken = tokens[tokens.IndexOf(currentToken) - 1];
            }
            else
            {
                leftToken = tokens.Last();
            }

            return Tuple.Create(leftToken, currentToken);
        }

        public static T GetVisualRustProperty<T>(DTE env, string key)
        {
            return (T)env.get_Properties("Visual Rust", "General").Item(key).Value;
        }
    }

    public class TemporaryFile : IDisposable
    {
        public string Path { get; private set; }

        /// <summary>
        ///  Creates a new temporary file with the argument string as UTF-8 content.
        /// </summary>
        public TemporaryFile(string content) : this(content, System.IO.Path.GetTempPath())
        {
        }

        /// <summary>
        /// Creates a temporary file at path with the argument string as UTF-8 content.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="directoryPath"></param>
        public TemporaryFile(string content, string directoryPath)
        {
            Path = System.IO.Path.Combine(directoryPath, System.IO.Path.GetRandomFileName());
            using (var sw = new StreamWriter(Path, false, new UTF8Encoding(false)))
            {
                sw.Write(content);
            }
        }

        public void Dispose()
        {
            if (File.Exists(Path))
                File.Delete(Path);
        }
    }

    public class WindowsErrorMode : IDisposable
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int SetErrorMode(int wMode);

        private readonly int oldErrorMode;

        /// <summary>
        ///     Creates a new error mode context.
        /// </summary>
        /// <param name="mode">Error mode to use. 3 is a useful value.</param>
        public WindowsErrorMode(int mode)
        {
            oldErrorMode = SetErrorMode(mode);
        }

        public void Dispose()
        {
            SetErrorMode(oldErrorMode);
        }
    }
}
