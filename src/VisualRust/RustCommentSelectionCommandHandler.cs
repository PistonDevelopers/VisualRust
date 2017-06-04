using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace VisualRust
{
    class RustCommentSelectionCommandHandler : VSCommandTarget<VSConstants.VSStd2KCmdID>
    {
        readonly ITextBuffer Buffer;
        public RustCommentSelectionCommandHandler(IVsTextView vsTextView, IWpfTextView textView)
            : base(vsTextView, textView)
        {
            Buffer = textView.TextBuffer;
        }

        protected override bool Execute(VSConstants.VSStd2KCmdID command, uint options, IntPtr pvaIn, IntPtr pvaOut)
        {
            var snapshot = Buffer.CurrentSnapshot;
            int start = TextView.Selection.Start.Position.Position;
            int end = TextView.Selection.End.Position.Position;

            // this is what we will store start offset in to get maximal indentation amount
            int? insertStartOffset = null;

            using (var edit = Buffer.CreateEdit())
            {
                // NOTE: At this point always start <= end, so we can safely run the loop at least once
                //       in order to get the desired effect even when start == end, i.e. there is just a cursor
                do
                {
                    var line = snapshot.GetLineFromPosition(start);
                    var text = line.GetText();
                    switch (command)
                    {
                        case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                            {
                                if (insertStartOffset == null) insertStartOffset = GetOffset(snapshot, start, end);
                                if (!string.IsNullOrWhiteSpace(text))
                                    edit.Insert(line.Start.Position + insertStartOffset.GetValueOrDefault(), "//");

                                break;
                            }
                        case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                            {
                                // NOTE: it is important to use the lexer here, because we want to keep doc comments
                                //       (purely textual check would result in '///' being changed to '/')
                                foreach (var token in Utils.LexString(text))
                                {
                                    // TODO: how to deal with block comments and other multiline tokens?
                                    if (token.Type == RustLexer.RustLexer.COMMENT)
                                    {
                                        edit.Delete(line.Start.Position + token.StartIndex, 2);
                                    }
                                    else if (token.Type != RustLexer.RustLexer.WHITESPACE)
                                    {
                                        break;
                                    }
                                }

                                break;
                            }
                    }

                    start = line.EndIncludingLineBreak.Position;
                } while (start < end);

                edit.Apply();
            }

            return true;
        }

        private int GetOffset(ITextSnapshot snapshot, int start, int end)
        {
            int offset = int.MaxValue;
            do
            {
                var line = snapshot.GetLineFromPosition(start);
                var text = line.GetText();

                for (int i = 0; i < text.Length; i++)
                {
                    if (!char.IsWhiteSpace(text[i]))
                        offset = Math.Min(offset, i);
                }

                start = line.EndIncludingLineBreak.Position;
            } while (start < end);

            return offset == int.MaxValue ? 0 : offset;
        }

        protected override IEnumerable<VSConstants.VSStd2KCmdID> SupportedCommands
        {
            get
            {
                yield return VSConstants.VSStd2KCmdID.COMMENTBLOCK;
                yield return VSConstants.VSStd2KCmdID.COMMENT_BLOCK;
                yield return VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK;
                yield return VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK;
            }
        }

        protected override VSConstants.VSStd2KCmdID ConvertFromCommandId(uint id)
        {
            return (VSConstants.VSStd2KCmdID)id;
        }

        protected override uint ConvertFromCommand(VSConstants.VSStd2KCmdID command)
        {
            return (uint)command;
        }
    }
}
