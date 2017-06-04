using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Formatting;

namespace VisualRust
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Auto indent controller")]
    [ContentType("rust")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    public class VisualRustAutoIndentProvider : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdaptersService { get; set; }

        [Import]
        internal IEditorOperationsFactoryService OperationsService { get; set; }

        [Import]
        internal ITextUndoHistoryRegistry UndoHistoryRegistry { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = AdaptersService.GetWpfTextView(textViewAdapter);
            if (textView == null)
            {
                Debug.Fail("Unexpected: couldn't get the text view");
                return;
            }

            IEditorOperations operations = OperationsService.GetEditorOperations(textView);
            if (operations == null)
            {
                Debug.Fail("Unexpected: couldn't get the editor operations object");
                return;
            }

            ITextUndoHistory undoHistory;
            if (!UndoHistoryRegistry.TryGetHistory(textView.TextBuffer, out undoHistory))
            {
                Debug.Fail("Unexpected: couldn't get an undo history for the text buffer");
                return;
            }

            Func<VisualRustAutoindent> autodedent = () => new VisualRustAutoindent(textViewAdapter, textView, operations, undoHistory);

            textView.Properties.GetOrCreateSingletonProperty(autodedent);
        }
    }

    internal class VisualRustAutoindent : IOleCommandTarget
    {
        private readonly IWpfTextView _textView;
        private readonly IEditorOperations _operations;
        private readonly ITextUndoHistory _undoHistory;

        private readonly IOleCommandTarget _nextCommandHandler;

        public VisualRustAutoindent(
            IVsTextView textViewAdapter,
            IWpfTextView textView,
            IEditorOperations operations,
            ITextUndoHistory undoHistory)
        {
            _textView = textView;
            _operations = operations;
            _undoHistory = undoHistory;

            textViewAdapter.AddCommandFilter(this, out _nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvalIn, IntPtr pvalOut)
        {
            if (ProcessInput(nCmdID, pvalIn))
            {
                return VSConstants.S_OK;
            }

            return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvalIn, pvalOut);
        }

        private bool ProcessInput(uint nCmdId, IntPtr pvalIn)
        {
            // make sure the input is a char before getting it
            if (nCmdId == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                // Make sure the number fits into a char before trying to use it
                var typedCharId = Marshal.GetObjectForNativeVariant(pvalIn);
                if (typedCharId is ushort && (ushort)typedCharId <= char.MaxValue)
                {
                    var typedChar = (char)(ushort)typedCharId;
                    return ProcessCharInput(typedChar);
                }
            }

            return false;
        }

        private bool ProcessCharInput(char typedChar)
        {
            if ((typedChar == '}' || typedChar == ')') && IsWhiteSpacesBeforeCaret())
            {
                return SetDecreasedIndention(typedChar);
            }

            return false;
        }

        private bool IsWhiteSpacesBeforeCaret()
        {
            var caret = _textView.Caret;
            var caretPosition = caret.Position.BufferPosition.Position;
            var textSnapshot = _textView.TextSnapshot;
            var caretLine = textSnapshot.GetLineFromPosition(caretPosition);
            var caretPositionInLine = caretPosition - caretLine.Start.Position;

            if (caretLine.Length == 0)
            {
                return true;
            }
            
            var span = new Span(caretLine.Start, caretPositionInLine);
            var text = textSnapshot.GetText(span);

            foreach (var ch in text)
            {
                if (!char.IsWhiteSpace(ch))
                {
                    return false;
                }
            }

            return true;
        }

        private bool SetDecreasedIndention(char typedChar)
        {
            using (var undo = _undoHistory.CreateTransaction("set indention"))
            {
                _operations.InsertText(typedChar.ToString());

                ISmartIndent smartIndent = new VisualRustSmartIndent(_textView);

            var caret = _textView.Caret;
            var caretPosition = caret.Position.BufferPosition.Position;
            var textSnapshot = _textView.TextSnapshot;
            var caretLine = textSnapshot.GetLineFromPosition(caretPosition);
                var desiredIndentation = smartIndent.GetDesiredIndentation(caretLine);
                if (desiredIndentation.HasValue)
                {
                    _operations.MoveToPreviousCharacter(false);
                    _operations.DeleteToBeginningOfLine();
                    _operations.InsertText(GetSpaces(desiredIndentation.Value));
                    _operations.MoveToNextCharacter(false);

                    undo.Complete();
                }
                else
                {
                    return false;
                }

                return true;
            }
        }

        private static string GetSpaces(int count)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(' ');
            }

            return sb.ToString();
        }
    }
}