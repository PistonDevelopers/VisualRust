using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    [StructLayout(LayoutKind.Sequential)]
    struct Utf8StringArray
    {
        public IntPtr Buffer;
        public int Length;

        public string[] ToArray()
        {
            if (Buffer == IntPtr.Zero)
                return null;
            var buffer = new string[Length];
            for (int i = 0; i < Length; i++)
            {
                unsafe
                {
                    Utf8String* current = ((Utf8String*)Buffer) + i;
                    buffer[i] = (*current).ToString();
                }
            }
            return buffer;

        }

        public static void Drop(Utf8StringArray s)
        {
            SafeNativeMethods.free_strbox_array(s);
        }
    }
}
