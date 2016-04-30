using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    [StructLayout(LayoutKind.Sequential)]
    struct Dependency
    {
        public Utf8String Version;
        public Utf8String Git;
        public Utf8String Path;
        public Utf8String Target;
    }
}
