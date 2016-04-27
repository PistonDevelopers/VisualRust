using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VisualRust.Cargo
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StrBox
    {
        public IntPtr Buffer;
        public int Length;

        public unsafe StrBox(IntPtr buf, int length)
            : this()
        {
            Buffer = buf;
            Length = length;
        }

        public static T With<T>(string s, Func<StrBox, T> f)
        {
            byte[] utf8 = Encoding.UTF8.GetBytes(s);
            unsafe
            {
                fixed (byte* p = utf8)
                {
                    return f(new StrBox(new IntPtr(p), utf8.Length));
                }
            }
        }

        public string TryToString()
        {
            if (Buffer == IntPtr.Zero)
                return null;
            var buffer = new byte[Length];
            Marshal.Copy(Buffer, buffer, 0, Length);
            return Encoding.UTF8.GetString(buffer, 0, Length);
        }

        [DllImport("vist_toml.dll")]
        static extern void free_strbox(StrBox s);

        public static void Drop(StrBox s)
        {
            free_strbox(s);
        }
    }
}