using System;
using System.Linq;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace VisualRust
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("rust")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class RustDocumentListener : IVsTextViewCreationListener
    {
        [Import]
        IVsEditorAdaptersFactoryService AdapterService { get; set; }

        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            textView.Properties.GetOrCreateSingletonProperty(() => new RustCompletionCommandHandler(textViewAdapter, textView, CompletionBroker));
            textView.Properties.GetOrCreateSingletonProperty(() => new RustCommentSelectionCommandHandler(textViewAdapter, textView));
            // TODO: also handle FormatDocument and GoToDefiniton here in a similar way
        }
    }
}
