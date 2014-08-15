using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Operations;

using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

namespace VisualRust
{
    internal sealed class VisualRustTextVewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService adaptor = null;

        internal void TextViewCreated(object sender, TextViewCreatedEventArgs args)
        {
            Debug.WriteLine("Text view created!");
            var textView = args.TextView;
            VisualRustCommandFilter filter = new VisualRustCommandFilter(textView);
            IOleCommandTarget next;
            if (ErrorHandler.Succeeded(adaptor.GetViewAdapter(textView).AddCommandFilter(filter, out next)))
                filter.Next = next;
        }
    }

    internal sealed class VisualRustCommandFilter : IOleCommandTarget
    {
        private IEditorOperations ed;
        private ITextView textView;
        internal IOleCommandTarget Next { get; set; }

        internal VisualRustCommandFilter(ITextView _textView)
        {
            textView = _textView;
            ed = Utils.editorOpFactory.GetEditorOperations(textView);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                var typeChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
                if (typeChar == '}')
                {
                    //ed.DecreaseLineIndent();
                    Debug.WriteLine("We should have dedented the line!");
                }
            }
            return Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return Next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
