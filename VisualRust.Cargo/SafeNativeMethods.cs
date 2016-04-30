using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    static class SafeNativeMethods
    {
        [DllImport("vist_toml.dll")]
        internal static extern void global_init();

        [DllImport("vist_toml.dll")]
        internal static extern void free_strbox(Utf8String s);

        [DllImport("vist_toml.dll")]
        internal static extern void free_strbox_array(Utf8StringArray s);

        [DllImport("vist_toml.dll")]
        internal static extern ParseResult load_from_utf16(IntPtr data, int length);

        [DllImport("vist_toml.dll")]
        internal static extern void free_manifest(IntPtr manifest);

        [DllImport("vist_toml.dll")]
        internal static extern StringQueryResult get_string(IntPtr manifest, RawSlice slice);

        [DllImport("vist_toml.dll")]
        internal static extern StringArrayQueryResult get_string_array(IntPtr manifest, RawSlice slice);
    }
}
