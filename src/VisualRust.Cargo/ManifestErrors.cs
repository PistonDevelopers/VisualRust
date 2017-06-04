using System.Collections.Generic;
using System.Linq;

namespace VisualRust.Cargo
{
    public class ManifestErrors
    {
        public string ParseError { get; private set; }
        public HashSet<EntryMismatchError> LoadErrors { get; private set; }
        public IList<FieldMalformedError> ValidationErrors { get; private set; }

        public ManifestErrors(string parseError)
        {
            ParseError = parseError;
        }

        public ManifestErrors(HashSet<EntryMismatchError> mismatchErrors, IList<FieldMalformedError> validationErrors)
        {
            LoadErrors = mismatchErrors;
            ValidationErrors = validationErrors;
        }

        public string[] GetErrors()
        {
            if(ParseError != null)
                return new string[] {  ParseError };
            return LoadErrors.Select(e => e.ToString())
                             .Union(ValidationErrors.Select(e => e.ToString()))
                             .ToArray();
        }
    }
}