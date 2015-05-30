using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using VisualRust.Racer;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust
{
    class RustGoToDefinitionCommandHandler : VSCommandTarget<VSConstants.VSStd97CmdID>
    {
        readonly ITextBuffer _buffer;
        readonly IServiceProvider _serviceProvider;

        public RustGoToDefinitionCommandHandler(IServiceProvider serviceProvider, IVsTextView vsTextView, IWpfTextView textView)
            : base(vsTextView, textView)
        {
            _buffer = textView.TextBuffer;
            _serviceProvider = serviceProvider;
        }

        protected override IEnumerable<VSConstants.VSStd97CmdID> SupportedCommands
        {
            get
            {
                yield return VSConstants.VSStd97CmdID.GotoDefn;
            }
        }

        protected override VSConstants.VSStd97CmdID ConvertFromCommandId(uint id)
        {
            return (VSConstants.VSStd97CmdID)id;
        }

        protected override uint ConvertFromCommand(VSConstants.VSStd97CmdID command)
        {
            return (uint)command;
        }

        protected override bool Execute(VSConstants.VSStd97CmdID command, uint options, IntPtr pvaIn, IntPtr pvaOut)
        {
            var snapshotPoint = TextView.Caret.Position.BufferPosition;
            var tokens = Utils.GetTokensAtPosition(snapshotPoint);

            // TODO: Taken from Execute in RustF1HelpCommandHandler
            var leftToken = tokens.Item1;
            var currentToken = tokens.Item2;
            var findToken = currentToken ?? leftToken;
            if (findToken == null)
            {
                return false;
            }

            var path = _buffer.GetFilePath();
            var line = snapshotPoint.GetContainingLine();
            // line.LineNumber uses 0 based indexing
            var row = line.LineNumber + 1;
            var column = snapshotPoint.Position - line.Start.Position;
            var args = $"find-definition {row} {column} {path}";
            var findOutput = RacerSingleton.Run(args);
            if (Regex.IsMatch(findOutput, "^MATCH"))
            {
                var results = findOutput.Substring(6).Split(',');
                // 1 based indexing
                var targetLine = Convert.ToInt32(results[1]) - 1;
                var targetColumn = Convert.ToInt32(results[2]);
                var fname = results[3];
                if (fname == path)
                {
                    // Current File
                    var newLine = _buffer.CurrentSnapshot.Lines.ElementAt(targetLine);
                    TextView.Caret.MoveTo(newLine.Start.Add(targetColumn));
                    TextView.ViewScroller.EnsureSpanVisible(TextView.GetTextElementSpan(newLine.Start));
                }
                else
                {
                    VsUtilities.NavigateTo(_serviceProvider, fname, Guid.Empty, targetLine, targetColumn);
                }
                return true;
            }
            return false;
        }
    }
}
