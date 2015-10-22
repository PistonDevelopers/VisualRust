using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using System.Collections.ObjectModel;

namespace VisualRust.Text
{
    /*
     * Contrary to what some examples on the internet show, there's
     * no guarantee that after disconnecting a buffer it's disposed.
     * One buffer can be connected to multiple text views.
     */
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType("rust")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public class TextBufferListener : IWpfTextViewConnectionListener
    {
        internal class PropertyEntry
        {
            public static readonly object Key = new object();

            private readonly HashSet<IWpfTextView> connectedViews = new HashSet<IWpfTextView>();
            private readonly ITextBuffer buffer;

            internal DocumentState Tokenizer { get; private set; }

            public PropertyEntry(IRustLexer lexer, ITextBuffer buffer)
            {
                this.buffer = buffer;
                this.Tokenizer = new DocumentState(lexer, buffer.CurrentSnapshot);
                buffer.Changed += OnTextChanged;
            }

            public void ConnectView(IWpfTextView view)
            {
                connectedViews.Add(view);
            }

            public void DisconnectView(IWpfTextView view)
            {
                connectedViews.Remove(view);
                if (connectedViews.Count == 0)
                {
                    buffer.Changed -= OnTextChanged;
                    buffer.Properties.RemoveProperty(PropertyEntry.Key);
                }
            }

            private void OnTextChanged(object _, TextContentChangedEventArgs args)
            {
                Tokenizer.TextChanged(args);
            }
        }

        readonly IRustLexer lexer;

        [ImportingConstructor]
        public TextBufferListener(IRustLexer lexer)
        {
            this.lexer = lexer;
        }

        public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            foreach (ITextBuffer buffer in subjectBuffers)
            {
                PropertyEntry conn = buffer.Properties.GetOrCreateSingletonProperty(PropertyEntry.Key, () => new PropertyEntry(lexer, buffer));
                conn.ConnectView(textView);
            }
        }

        public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            foreach (ITextBuffer buffer in subjectBuffers)
            {
                PropertyEntry conn;
                if (!buffer.Properties.TryGetProperty(PropertyEntry.Key, out conn))
                    continue;
                conn.DisconnectView(textView);
            }
        }
    }
}
