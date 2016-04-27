using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace VisualRust.Cargo
{
    public class Manifest : IDisposable
    {
        static Manifest()
        {
            global_init();
        }

        [DllImport("vist_toml.dll")]
        static extern void global_init();

        [DllImport("vist_toml.dll")]
        static extern ParseResult load_from_utf16(IntPtr data, int length);

        [DllImport("vist_toml.dll")]
        static extern void free_manifest(IntPtr manifest);

        [DllImport("vist_toml.dll")]
        static extern StringQueryResult get_string(IntPtr manifest, RawSlice slice);

        [StructLayout(LayoutKind.Sequential)]
        public struct ParseResult
        {
            public IntPtr Manifest;
            public StrBox Error;
        }

        struct EntryMismatch
        {
            public string Path { get; set; }
            public string Expected { get; private set; }
            public string Got { get; private set; }

            public EntryMismatch(string path, string expected, string got) : this()
            {
                Path = path;
                this.Expected = expected;
                this.Got = got;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct QueryError
        {
            public int Depth;
            public StrBox Kind;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct StringQueryResult
        {
            public StrBox Result;
            public QueryError Error;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RawSlice
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

        private readonly IntPtr manifest;

        public Manifest(string text)
        {
            unsafe
            {
                fixed (char* p = text)
                {
                    ParseResult result = Rust.Call(load_from_utf16, new IntPtr(p), text.Length);
                    this.manifest = result.Manifest;
                    if (this.manifest == IntPtr.Zero)
                    {
                        string error = result.Error.ToString();
                        StrBox.Drop(result.Error);
                        throw new ArgumentException(error);
                    }
                }
            }
            EntryMismatch temp;
            name = GetString(out temp, "package", "name");
            version = GetString(out temp, "package", "version");
        }

        private string name;
        public string Name
        {
            get { return name; }
        }

        private string version;
        public string Version
        {
            get { return version; }
        }

        private string GetString(out EntryMismatch error, params string[] path)
        {
            unsafe
            {
                var handles = new GCHandle[path.Length];
                var buffers = new StrBox[path.Length];
                for (int i = 0; i < path.Length; i++)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(path[i]);
                    handles[i] = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    buffers[i] = new StrBox(handles[i].AddrOfPinnedObject(), buffer.Length);
                }
                string result;
                fixed (StrBox* arr = buffers)
                {
                    StringQueryResult ffiResult = Rust.Call(get_string, manifest, new RawSlice(arr, buffers.Length));
                    result = ffiResult.Result.TryToString();
                    if (result == null && ffiResult.Error.Kind.Buffer != IntPtr.Zero)
                    {
                        int length = ffiResult.Error.Depth;
                        string expectedType = length < path.Length - 1 ? "table" : "string";
                        error = new EntryMismatch(String.Join(".", path.Take(length + 1)), expectedType, ffiResult.Error.Kind.TryToString());
                    }
                    else
                    {
                        error = default(EntryMismatch);
                    }
                    StrBox.Drop(ffiResult.Result);
                }
                for (int i = 0; i < handles.Length; i++)
                    handles[i].Free();
                return result;
            }
        }

        private void SetString(string value, params string[] path)
        {
            //return StrBox.With(value, str => Rust.Call(get_string, str));
        }

        public void Dispose()
        {
            Rust.Invoke(free_manifest, this.manifest);
        }
    }
}
