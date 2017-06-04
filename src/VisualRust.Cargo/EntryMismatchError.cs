using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    public class EntryMismatchError
    {
        public string Path { get; set; }
        public string Expected { get; private set; }
        public string Got { get; private set; }

        public EntryMismatchError(string path, string expected, string got)
        {
            Path = path;
            Expected = expected;
            Got = got;
        }

        internal EntryMismatchError(RawDependencyError e)
        {
            Path = e.Path.ToString();
            Expected = e.Expected.ToString();
            Got = e.Got.ToString();
        }

        public override string ToString()
        {
            return String.Format(
                "Expected a value of type `{0}`, but found a value of type `{1}` for the key `{2}`",
                Expected,
                Got,
                Path);
        }
    }
}
