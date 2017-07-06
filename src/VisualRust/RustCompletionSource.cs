using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Antlr4.Runtime;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.Text.RegularExpressions;
using System.Windows;
using VisualRust.Racer;

namespace VisualRust
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("rust")]
    [Name("rustCompletion")]
    internal class RustCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal IGlyphService GlyphService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new RustCompletionSource(textBuffer, GlyphService);
        }
    }


    internal class RustCompletionSource : ICompletionSource
    {
        // These are returned by racer in the fifths column of a complete call.
        private enum CompletableLanguageElement
        {
            Struct,
            Module,
            Function,
            Crate,
            Let,
            StructField,
            Impl,
            Enum,
            EnumVariant,
            Type,
            FnArg,
            Trait,
            Static,
        }

        private readonly IGlyphService glyphService;
        private readonly ITextBuffer buffer;
        private bool disposed;

        private readonly IEnumerable<Completion> keywordCompletions = GetKeywordCompletions();

        /// <summary>
        /// Get completions list filtered by prefix
        /// </summary>
        /// <param name="prefix">
        /// String with prefix before caret in source code
        /// </param>
        /// <returns>
        /// List completions
        /// </returns>
        private static IEnumerable<Completion> GetKeywordCompletions(string prefix = null)
        {
            var keywords = Utils.Keywords;
            var resultKeywords = string.IsNullOrEmpty(prefix) ? keywords : keywords.Where(x => x.StartsWith(prefix));
            var completions = resultKeywords.Select(k => new Completion(k, k + " ", "", null, ""));

            return completions;
        }

        public RustCompletionSource(ITextBuffer buffer, IGlyphService glyphService)
        {
            this.buffer = buffer;
            this.glyphService = glyphService;
        }

        private ImageSource GetCompletionIcon(CompletableLanguageElement elType)
        {
            switch (elType)
            {
                case CompletableLanguageElement.Struct:
                    return glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupStruct, StandardGlyphItem.GlyphItemPublic);
                case CompletableLanguageElement.Module:
                    return glyphService.GetGlyph(StandardGlyphGroup.GlyphAssembly, StandardGlyphItem.GlyphItemPublic);
                case CompletableLanguageElement.Function:
                    return glyphService.GetGlyph(StandardGlyphGroup.GlyphExtensionMethod,
                        StandardGlyphItem.GlyphItemPublic);
                case CompletableLanguageElement.Crate:
                    return glyphService.GetGlyph(StandardGlyphGroup.GlyphAssembly, StandardGlyphItem.GlyphItemPublic);
                case CompletableLanguageElement.Let:
                    return glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupConstant,
                        StandardGlyphItem.GlyphItemPublic);
                case CompletableLanguageElement.StructField:
                    return glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupField, StandardGlyphItem.GlyphItemPublic);
                case CompletableLanguageElement.Impl:
                    return glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupTypedef, StandardGlyphItem.GlyphItemPublic);
                case CompletableLanguageElement.Enum:
                    return glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupEnum, StandardGlyphItem.GlyphItemPublic);
                case CompletableLanguageElement.EnumVariant:
                    return glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupEnumMember,
                        StandardGlyphItem.GlyphItemPublic);
                case CompletableLanguageElement.Type:
                    return glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupTypedef, StandardGlyphItem.GlyphItemPublic);
                case CompletableLanguageElement.Trait:
                    return glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupInterface,
                        StandardGlyphItem.GlyphItemPublic);
                case CompletableLanguageElement.Static:
                    return null;
                case CompletableLanguageElement.FnArg:
                    return null;
                default:
                    Utils.DebugPrintToOutput("Unhandled language element found in racer autocomplete response: {0}", elType);
                    return null;
            }
        }

        /// <summary>
        ///   Fetches auto complete suggestions and appends to the completion sets of the current completion session.
        /// </summary>
        /// <param name="session">The active completion session, initiated from the completion command handler.</param>
        /// <param name="completionSets">A list of completion sets that may be augmented by this source.</param>
        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            ITextSnapshot snapshot = buffer.CurrentSnapshot;
            SnapshotPoint? sp = session.GetTriggerPoint(snapshot);
            if (!sp.HasValue)
                return;

            var triggerPoint = sp.Value;

            var line = triggerPoint.GetContainingLine();
            int col = triggerPoint.Position - line.Start.Position;

            if (line.GetText() == "" || col == 0 || char.IsWhiteSpace(line.GetText()[col - 1]))
            {
                // On empty rows or without a prefix, return only completions for rust keywords.                                
                var location = snapshot.CreateTrackingSpan(col + line.Start.Position, 0, SpanTrackingMode.EdgeInclusive);
                completionSets.Add(new RustCompletionSet("All", "All", location, keywordCompletions, null));
                return;
            }

            // Get token under cursor.            
            var activeToken = GetActiveToken(col, line);
            if (activeToken == null)
                return;

            RustTokenTypes tokenType = Utils.LexerTokenToRustToken(activeToken.Text, activeToken.Type);

            // Establish the extents of the current token left of the cursor.
            var extent = new TextExtent(
                new SnapshotSpan(
                    new SnapshotPoint(snapshot, activeToken.StartIndex + line.Start.Position),
                    triggerPoint),
                tokenType != RustTokenTypes.WHITESPACE);

            var span = snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);

            // Fetch racer completions & return in a completion set.
            string prefix;
            var completions = GetCompletions(RunRacer(snapshot, triggerPoint), out prefix).ToList();
            completions.AddRange(GetKeywordCompletions(prefix));

            completionSets.Add(new RustCompletionSet("All", "All", span, completions, null));

        }

        private static IToken GetActiveToken(int columnIndex, ITextSnapshotLine line)
        {
            var tokens = Utils.LexString(line.GetText());
            if (columnIndex == line.Length)
            {
                var lastToken = tokens.Last();
                if (lastToken.Type == RustLexer.RustLexer.IDENT)
                    return lastToken;
                
                // fake token for an ident not yet started at the end of the line. 
                return new CommonToken(new Tuple<ITokenSource, ICharStream>(lastToken.TokenSource, lastToken.TokenSource.InputStream),
                    RustLexer.RustLexer.IDENT, 0, columnIndex, columnIndex);
            }

            IToken token = null;
            IToken previousToken = null;
            foreach (var currentToken in tokens)
            {
                if (currentToken.StartIndex <= columnIndex && columnIndex <= currentToken.StopIndex)
                {
                    token = currentToken;
                    break;
                }

                previousToken = currentToken;
            }

            if (token == null)
            {
                return null;
            }

            if (token.Type == RustLexer.RustLexer.IDENT)
            {
                return token;
            }

            // if current token isn't identifier and caret at end of ident token
            if (token.StartIndex == columnIndex && previousToken != null && previousToken.Type == RustLexer.RustLexer.IDENT)
            {
                return previousToken;
            }

            // fake token for position between 2 non-ident tokens
            return new CommonToken(new Tuple<ITokenSource, ICharStream>(token.TokenSource, token.TokenSource.InputStream), 1, 0, token.StartIndex, token.StartIndex - 1);
        }

        private static int GetColumn(SnapshotPoint point)
        {
            var line = point.GetContainingLine();
            int col = point.Position - line.Start.Position;
            return col;
        }

        private static string RunRacer(ITextSnapshot snapshot, SnapshotPoint point)
        {
            using (var tmpFile = new TemporaryFile(snapshot.GetText()))
            {
                // Build racer command line: "racer.exe complete lineNo columnNo \"originalFile\" \"substituteFile\"
                ITextDocument document = null;
                snapshot?.TextBuffer?.Properties?.TryGetProperty(typeof(ITextDocument), out document);
                var origPath = document?.FilePath ?? tmpFile.Path;
                int lineNumber = point.GetContainingLine().LineNumber;
                int charNumber = GetColumn(point);
                string args = string.Format("complete-with-snippet {0} {1} \"{2}\" \"{3}\"", lineNumber + 1, charNumber, origPath, tmpFile.Path);
                return Racer.RacerSingleton.Run(args);
            }
        }

        
        // Parses racer output into completions.
        private IEnumerable<Completion> GetCompletions(string racerResponse, out string prefix)
        {
            // Completions from racer.
            var lines = racerResponse.Split(new[] { '\n' }, StringSplitOptions.None);
            prefix = GetPrefix(lines[0]);

            return GetCompletions(lines);
        }

        static readonly Regex ReDocsCodeSection = new Regex(@"(?<=^|\n)```(?<type>[a-zA-Z_0-9]*)\n(?<code>.*?)\n```(?=\n|$)", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
        static readonly Regex ReDocsCodeInline = new Regex(@"`(?<code>.*?)`", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private IEnumerable<Completion> GetCompletions(string[] lines)
        {
            // Emitting code: https://github.com/racer-rust/racer/blob/4d694e1e17f58bbf01e52fc152065d4bc06157e2/src/bin/main.rs#L216-L254

            var matches = lines.Where(l => l.StartsWith("MATCH")).Distinct(StringComparer.Ordinal);

            foreach (var matchLine in matches)
            {
                RacerMatch racerMatch;
                if (!RacerMatch.TryParse(matchLine, RacerMatch.Type.CompleteWithSnippet, RacerMatch.Interface.Default, out racerMatch))
                {
                    Utils.DebugPrintToOutput("Failed to parse racer match line found in racer autocomplete resource: {0}", matchLine);
                    continue;
                }

                var docs = racerMatch.Documentation;
                docs = ReDocsCodeSection.Replace(docs, codeSection => codeSection.Groups["code"].Value);
                docs = ReDocsCodeInline .Replace(docs, codeInline  => codeInline .Groups["code"].Value);

                CompletableLanguageElement elType;
                if (!Enum.TryParse(racerMatch.MatchType, out elType))
                {
                    Utils.DebugPrintToOutput("Failed to parse language element found in racer autocomplete response: {0}", racerMatch.MatchType);
                    continue;
                }

                var displayText = racerMatch.MatchString;
                var insertionText = racerMatch.MatchString;
                var description = string.IsNullOrWhiteSpace(docs) ? racerMatch.Context : racerMatch.Context+"\n"+docs;
                var icon = GetCompletionIcon(elType);
                yield return new Completion(displayText, insertionText, description, icon, "");
            }
        }

        private string GetPrefix(string line)
        {
            var tokens = line.Split(',');
            if(tokens.Length != 3)
                return null;
            var prefix = tokens[2];
            return prefix;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }


    }

    internal class RustCompletionSet : CompletionSet
    {
        public RustCompletionSet(string moniker, string name, ITrackingSpan applicableTo,
            IEnumerable<Completion> completions, IEnumerable<Completion> completionBuilders)
            : base(moniker, name, applicableTo, completions, completionBuilders)
        {
        }

        public override void Filter()
        {
            Filter(CompletionMatchType.MatchInsertionText, true);
        }
    }

}