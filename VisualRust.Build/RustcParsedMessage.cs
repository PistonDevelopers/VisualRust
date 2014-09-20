using System;

namespace VisualRust.Build
{
    enum RustcParsedMessageType
    {
        Error,
        Warning,
        Note
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
        }

        public bool TryMergeWithFollowing(RustcParsedMessage other)
        {
            if (other.Type == RustcParsedMessageType.Note && other.File == this.File &&
                other.LineNumber == this.LineNumber && other.ColumnNumber == this.ColumnNumber &&
                other.EndLineNumber == this.EndLineNumber && other.EndColumnNumber == this.EndColumnNumber)
            {
                this.Message += "\nnote: " + other.Message;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
