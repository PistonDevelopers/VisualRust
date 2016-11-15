using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualRust.Shared.Message
{
    public class RustcMessageJson
    {
        public class DiagnosticCode
        {
            /// <summary>
            /// The code itself.
            /// </summary>
            public string code { get; set; }
            /// <summary>
            /// An explanation for the code.
            /// </summary>
            public string explanation { get; set; }
        }

        public class DiagnosticSpanLine
        {
            public string text { get; set; }
            public int highlight_start { get; set; }
            public int highlight_end { get; set; }
        }

        public class DiagnosticSpanMacroExpansion
        {
            /// <summary>
            /// span where macro was applied to generate this code; note that
            /// this may itself derive from a macro (if
            /// `span.expansion.is_some()`)
            /// </summary>
            public DiagnosticSpan span { get; set; }
            /// <summary>
            /// name of macro that was applied (e.g., "foo!" or "#[derive(Eq)]")
            /// </summary>
            public String macro_decl_name { get; set; }
            /// <summary>
            /// span where macro was defined (if known)
            /// </summary>
            public DiagnosticSpan def_site_span { get; set; }
        }

        public class DiagnosticSpan
        {
            public string file_name { get; set; }
            public int byte_start { get; set; }
            public int byte_end { get; set; }
            public int line_start { get; set; }
            public int line_end { get; set; }
            public int column_start { get; set; }
            public int column_end { get; set; }
            /// <summary>
            /// Is this a "primary" span -- meaning the point, or one of the points,
            /// where the error occurred?
            /// </summary>
            public bool is_primary { get; set; }
            /// <summary>
            /// Source text from the start of line_start to the end of line_end.
            /// </summary>
            public List<DiagnosticSpanLine> text { get; set; }
            /// <summary>
            /// Label that should be placed at this location (if any)
            /// </summary>
            public object label { get; set; }
            /// <summary>
            /// If we are suggesting a replacement, this will contain text
            /// that should be sliced in atop this span. You may prefer to
            /// load the fully rendered version from the parent `Diagnostic`,
            /// however.
            /// </summary>
            public String suggested_replacement { get; set; }
            /// <summary>
            /// Macro invocations that created the code at this span, if any.
            /// </summary>
            public DiagnosticSpanMacroExpansion expansion { get; set; }
        }

        /// <summary>
        /// The primary error message.
        /// </summary>
        public string message { get; set; }
        public DiagnosticCode code { get; set; }
        /// <summary>
        /// "error: internal compiler error", "error", "warning", "note", "help"
        /// </summary>
        public string level { private get; set; }
        public List<DiagnosticSpan> spans { get; set; }
        /// <summary>
        /// Associated diagnostic messages.
        /// </summary>
        public List<RustcMessageJson> children { get; set; }
        /// <summary>
        /// The message as rustc would render it. Currently this is only
        /// `Some` for "suggestions", but eventually it will include all
        /// snippets.
        /// </summary>
        public object rendered { get; set; }

        public RustcMessageType GetLevelAsEnum()
        {
            if (level == "error: internal compiler error" || level == "error")
                return RustcMessageType.Error;
            if (level == "warning")
                return RustcMessageType.Warning;
            if (level == "note")
                return RustcMessageType.Note;
            if (level == "help")
                return RustcMessageType.Help;

            return RustcMessageType.Error;
        }

        public DiagnosticSpan GetPrimarySpan()
        {
            if (spans == null || spans.Count == 0)
                return null;

            return spans.FirstOrDefault(a => a.is_primary);
        }

        public String GetErrorCodeAsString()
        {
            if (code == null)
                return string.Empty;
            return code.code;
        }

        public String GetMessage()
        {
            var hasMeaningfulMessage = !String.IsNullOrWhiteSpace(message);

            if (children != null && children.Count > 0)
            {
                var stackedMessages = hasMeaningfulMessage ? new StringBuilder(message) : new StringBuilder();
                if (hasMeaningfulMessage) stackedMessages.Append(" (");

                foreach (var child in children)
                {
                    if (child.GetLevelAsEnum() == RustcMessageType.Note)
                    {

                        // i don't know, maybe the notes can have their own notes,
                        // so lets append them also
                        var innerMessage = child.GetMessage();
                        if (!String.IsNullOrWhiteSpace(innerMessage))
                        {
                            stackedMessages.Append(innerMessage);
                        }
                    }
                }
                if (hasMeaningfulMessage) stackedMessages.Append(')');

                return stackedMessages.ToString();
            }
            else return hasMeaningfulMessage ? message : String.Empty;
        }
    }
}
