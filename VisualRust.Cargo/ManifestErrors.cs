using System.Collections.Generic;
using System.Linq;

namespace VisualRust.Cargo
{
    public class ManifestErrors
    {
        public string ParseError { get; private set; }
        public HashSet<EntryMismatchError> LoadErrors { get; private set; }

        public ManifestErrors(string parseError)
        {
            ParseError = parseError;
        }

        public ManifestErrors(HashSet<EntryMismatchError> errors)
        {
            LoadErrors = errors;
        }
    }
}