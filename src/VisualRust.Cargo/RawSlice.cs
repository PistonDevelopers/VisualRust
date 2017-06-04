using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    [StructLayout(LayoutKind.Sequential)]
    struct RawSlice
    {
        public IntPtr Buffer;
        public int Length;

        public unsafe RawSlice(void* buf, int length)
            : this()
        {
            Buffer = new IntPtr(buf);
            Length = length;
        }
    }
}
