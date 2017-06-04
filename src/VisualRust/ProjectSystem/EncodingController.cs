using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using System.Text;

namespace VisualRust.ProjectSystem
{
    /// <summary>
    /// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
    /// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class EncodingController : IWpfTextViewCreationListener
    {

        private static readonly Encoding utf8 = new UTF8Encoding(false);

        /// <summary>
        /// Called when a text view having matching roles is created over a text data model having a matching content type.
        /// Instantiates a EncodingController manager when the textView is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
        public void TextViewCreated(IWpfTextView textView)
        {
            var properies = textView?.TextDataModel?.DocumentBuffer?.Properties;
            if (properies == null)
                return;

            ITextDocument textDocument;
            bool extracted = properies.TryGetProperty(typeof(ITextDocument), out textDocument);
            if (!extracted || textDocument == null)
                return;

            if (textDocument.FilePath.EndsWith(".rs") || textDocument.FilePath.EndsWith(".toml"))
                textDocument.Encoding = utf8;
        }

    }
}
