using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    [StructLayout(LayoutKind.Sequential)]
    struct QueryError
    {
        public int Depth;
        public Utf8String Kind;
    }
}
