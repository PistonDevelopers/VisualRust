namespace VisualRust.Build.Message.Human
{
    class RustcMessageHuman
    {
        public RustcMessageType Type;
        public string Message;
        public string ErrorCode;
        public string File;
        public int LineNumber;
        public int ColumnNumber;
        public int EndLineNumber;
        public int EndColumnNumber;
        public bool CanExplain; // TODO: currently we don't do anything with this

        public RustcMessageHuman(RustcMessageType type, string message, string errorCode, string file,
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

        public bool TryMergeWithFollowing(RustcMessageHuman other)
        {
            if ((other.Type == RustcMessageType.Note || other.Type == RustcMessageType.Help)
                && other.File == this.File && other.LineNumber == this.LineNumber && other.ColumnNumber == this.ColumnNumber &&
                other.EndLineNumber == this.EndLineNumber && other.EndColumnNumber == this.EndColumnNumber)
            {
                var prefix = other.Type == RustcMessageType.Note ? "\nnote: " : "\nhelp: ";
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
