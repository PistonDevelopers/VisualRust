using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VisualRust.Cargo
{
    [StructLayout(LayoutKind.Sequential)]
    struct Utf8String
    {
        public IntPtr Buffer;
        public int Length;

        public unsafe Utf8String(IntPtr buf, int length)
            : this()
        {
            Buffer = buf;
            Length = length;
        }

        public new string ToString()
        {
            if (Buffer == IntPtr.Zero)
                return null;
            var buffer = new byte[Length];
            Marshal.Copy(Buffer, buffer, 0, Length);
            return Encoding.UTF8.GetString(buffer, 0, Length);
        }

        public static void Drop(Utf8String s)
        {
            SafeNativeMethods.free_strbox(s);
        }
    }
}