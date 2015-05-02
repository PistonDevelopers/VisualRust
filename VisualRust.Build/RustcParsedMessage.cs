using System;

namespace VisualRust.Build
{
    enum RustcParsedMessageType
    {
        Error,
        Warning,
        Note,
        Help
    }

    class RustcParsedMessage
    {
        public RustcParsedMessageType Type;
        public string Message;
        public string ErrorCode;
        public string File;
        public int LineNumber;
        public int ColumnNumber;
        public int EndLineNumber;
        public int EndColumnNumber;
        public bool CanExplain; // TODO: currently we don't do anything with this

        public RustcParsedMessage(RustcParsedMessageType type, string message, string errorCode, string file,
            int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber)
        {
            Type = type;
            Message = message;
            ErrorCode = errorCode;
            File = file;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
            EndLineNumber = endLineNumber;
            EndColumnNumber = endColumnNumber;
            CanExplain = false;
        }

        public bool TryMergeWithFollowing(RustcParsedMessage other)
        {
            if ((other.Type == RustcParsedMessageType.Note || other.Type == RustcParsedMessageType.Help)
                && other.File == this.File && other.LineNumber == this.LineNumber && other.ColumnNumber == this.ColumnNumber &&
                other.EndLineNumber == this.EndLineNumber && other.EndColumnNumber == this.EndColumnNumber)
            {
                var prefix = other.Type == RustcParsedMessageType.Note ? "\nnote: " : "\nhelp: ";
                this.Message += prefix + other.Message;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
