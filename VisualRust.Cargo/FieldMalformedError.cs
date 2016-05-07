using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    public class FieldMalformedError
    {
        public string Path { get; set; }
        public bool Exists { get; set; }

        public FieldMalformedError(string path, bool exists)
        {
            Path = path;
            Exists = exists;
        }

        public override string ToString()
        {
            if(!Exists)
                return String.Format("Value for the key `{0}` does not exist", Path);
            else 
                return String.Format("Value for the key `{0}` is empty", Path);
        }
    }
}
