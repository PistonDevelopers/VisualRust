using System.Collections.Generic;
using System.Linq;

namespace VisualRust.Cargo
{
    public class LoadError
    {
        public string ParseError { get; private set; }
        public HashSet<EntryMismatchError> LoadErrors { get; private set; }

        public LoadError(string parseError)
        {
            ParseError = parseError;
        }

        public LoadError(HashSet<EntryMismatchError> errors)
        {
            LoadErrors = errors;
        }
    }
}