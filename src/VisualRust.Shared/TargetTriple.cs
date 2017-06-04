using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Shared
{
    public class TargetTriple
    {
        public string Arch { get; private set; }
        public string Vendor { get; private set; }
        public string System { get; private set; }
        public string Abi { get; private set; }

        TargetTriple(string arch, string vendor, string sys, string abi)
        {
            Arch = arch;
            Vendor = vendor;
            System = sys;
            Abi = abi;
        }

        public static TargetTriple Create(string text)
        {
            if(text == null)
                return null;
            string[] parts = text.ToLowerInvariant().Split('-');
            string abi;
            if (parts.Length < 3)
                return null;
            else if (parts.Length < 4)
                abi = "";
            else if (parts.Length == 4)
                abi = parts[3];
            else
                return null;
            return new TargetTriple(parts[0], parts[1], parts[2], abi);
        }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Abi))
                return String.Format("{0}-{1}-{2}", Arch, Vendor, System);
            else
                return String.Format("{0}-{1}-{2}-{3}", Arch, Vendor, System, Abi);
        }
    }
}
