using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    [StructLayout(LayoutKind.Sequential)]
    struct RawDependencyError
    {
        public Utf8String Path;
        public Utf8String Expected;
        public Utf8String Got;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    struct RawDependencyErrorArray
    {
        public IntPtr Buffer;
        public int Length;

        public EntryMismatchError[] ToArray()
        {
            if (Buffer == IntPtr.Zero)
                return null;
            var buffer = new EntryMismatchError[Length];
            for (int i = 0; i < Length; i++)
            {
                unsafe
                {
                    RawDependencyError* current = ((RawDependencyError*)Buffer) + i;
                    buffer[i] = new EntryMismatchError(*current);
                }
            }
            return buffer;
        }
    }
}
