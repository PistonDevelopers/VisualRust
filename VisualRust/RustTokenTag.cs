namespace ArkeIndustries.VisualRust
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;

    using RustLexer;

    [Export(typeof(ITaggerProvider))]
    [ContentType("rust")]
    [TagType(typeof(RustTokenTag))]
    internal sealed class RustTokenTagProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new RustTokenTagger(buffer) as ITagger<T>;
        }
    }

    public class RustTokenTag : ITag
    {
        public RustTokenTypes type { get; private set; }
        public RustTokenTag(RustTokenTypes type)
        {
            this.type = type;
        }
    }

    internal sealed class RustTokenTagger : ITagger<RustTokenTag>
    {
        ITextBuffer buffer;
        Dictionary<int, RustTokenTypes> _tt;
        HashSet<string> _kws;

        internal RustTokenTagger(ITextBuffer buffer)
        {
            this.buffer = buffer;
            _tt = new Dictionary<int, RustTokenTypes>();
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

            _kws = new HashSet<string>();
            _kws.Add("as");
            _kws.Add("box");
            _kws.Add("break");
            _kws.Add("continue");
            _kws.Add("crate");
            _kws.Add("else");
            _kws.Add("enum");
            _kws.Add("extern");
            _kws.Add("false");
            _kws.Add("fn");
            _kws.Add("for");
            _kws.Add("if");
            _kws.Add("impl");
            _kws.Add("in");
            _kws.Add("let");
            _kws.Add("loop");
            _kws.Add("match");
            _kws.Add("mod");
            _kws.Add("mut");
            _kws.Add("priv");
            _kws.Add("proc");
            _kws.Add("pub");
            _kws.Add("ref");
            _kws.Add("return");
            _kws.Add("self");
            _kws.Add("static");
            _kws.Add("struct");
            _kws.Add("super");
            _kws.Add("true");
            _kws.Add("trait");
            _kws.Add("type");
            _kws.Add("unsafe");
            _kws.Add("use");
            _kws.Add("while");
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<RustTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan curSpan in spans)
            {
                var containingLine = curSpan.Start.GetContainingLine();
                var lexer = new RustLexer(curSpan.Start.GetContainingLine().GetText());
                int curLoc = containingLine.Start.Position;

                while (true)
                {
                    var tok = lexer.NextToken();
                    if (tok.Type == RustLexer.Eof)
                    {
                        yield break;
                    }
                    var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curLoc + tok.StartIndex, tok.Text.Length));
                    if (tokenSpan.IntersectsWith(curSpan))
                    {
                        RustTokenTypes ty = _tt[tok.Type];
                        if (ty == RustTokenTypes.IDENT)
                        {
                            if (_kws.Contains(tok.Text))
                            {
                                ty = RustTokenTypes.KEYWORD;
                            }
                            else
                            {
                                ty = RustTokenTypes.IDENT;
                            }
                        }

                        yield return new TagSpan<RustTokenTag>(tokenSpan, new RustTokenTag(ty));
                    }
                }
            }
        }
    }
}