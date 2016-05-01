using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace VisualRust.Cargo
{
    public class Manifest : IDisposable
    {
        class MismatchComparer : IEqualityComparer<EntryMismatchError>
        {
            public bool Equals(EntryMismatchError x, EntryMismatchError y)
            {
                if (x == null || y == null)
                    return x == y;
                return String.Equals(x.Path, y.Path, StringComparison.Ordinal);
            }

            public int GetHashCode(EntryMismatchError obj)
            {
                return obj.Path.GetHashCode();
            }
        }

        static Manifest()
        {
            SafeNativeMethods.global_init();
        }

        private readonly IntPtr manifest;

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

        private string description;
        public string Description
        {
            get { return description; }
        }

        private string[] authors;
        public string[] Authors
        {
            get { return authors; }
        }

        private string documentation;
        public string Documentation
        {
            get { return documentation; }
        }

        private string homepage;
        public string Homepage
        {
            get { return homepage; }
        }

        private string repository;
        public string Repository
        {
            get { return repository; }
        }

        private string license;
        public string License
        {
            get { return license; }
        }

        Dependency[] dependencies;
        public Dependency[] Dependencies
        {
            get { return dependencies; }
        }

        private Manifest(IntPtr handle)
        {
            this.manifest = handle;
        }

        static IntPtr Parse(string text, out string error)
        {
            unsafe
            {
                fixed (char* p = text)
                {
                    ParseResult result = Rust.Call(SafeNativeMethods.load_from_utf16, new IntPtr(p), text.Length);
                    if (result.Manifest == IntPtr.Zero)
                    {
                        error = result.Error.ToString();
                        Utf8String.Drop(result.Error);
                    }
                    else
                    {
                        error = null;
                    }
                    return result.Manifest;
                }
            }
        }

        public static Manifest TryCreate(string text, out ManifestErrors loadErrors)
        {
            string error;
            IntPtr manifestPtr = Parse(text, out error);
            if (manifestPtr == IntPtr.Zero)
            {
                loadErrors = new ManifestErrors(error);
                return null;
            }
            Manifest manifest = new Manifest(manifestPtr);
            HashSet<EntryMismatchError> errors = manifest.Load();
            if (errors.Count > 0)
            {
                loadErrors = new ManifestErrors(errors);
                return null;
            }
            loadErrors = null;
            return manifest;
        }

        private HashSet<EntryMismatchError> Load()
        {
            HashSet<EntryMismatchError> errors = new HashSet<EntryMismatchError>(new MismatchComparer());
            name = GetString(errors, "package", "name");
            version = GetString(errors, "package", "version");
            authors = GetStringArray(errors, "package", "authors");
            description = GetString(errors, "package", "description");
            documentation = GetString(errors, "package", "documentation");
            homepage = GetString(errors, "package", "homepage");
            repository = GetString(errors, "package", "repository");
            license = GetString(errors, "package", "license");
            dependencies = GetDependencies(errors);
            return errors;
        }

        private Dependency[] GetDependencies(HashSet<EntryMismatchError> errors)
        {
            using (DependenciesQueryResult deps = Rust.Call(SafeNativeMethods.get_dependencies, manifest))
            {
                EntryMismatchError[] callErrors = deps.Errors.ToArray();
                if (callErrors != null)
                {
                    foreach (var error in callErrors)
                        errors.Add(error);
                }
                return deps.Dependencies.ToArray();
            }
        }

        private string GetString(HashSet<EntryMismatchError> errors, params string[] path)
        {
            EntryMismatchError error;
            string value = GetString(path, out error);
            if (error != null)
                errors.Add(error);
            return value;
        }

        private string GetString(string[] path, out EntryMismatchError error)
        {
            unsafe
            {
                var handles = new GCHandle[path.Length];
                var buffers = new Utf8String[path.Length];
                for (int i = 0; i < path.Length; i++)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(path[i]);
                    handles[i] = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    buffers[i] = new Utf8String(handles[i].AddrOfPinnedObject(), buffer.Length);
                }
                string result;
                fixed (Utf8String* arr = buffers)
                {
                    using (StringQueryResult ffiResult = Rust.Call(SafeNativeMethods.get_string, manifest, new RawSlice(arr, buffers.Length)))
                    {
                        result = ffiResult.Result.ToString();
                        if (result == null && ffiResult.Error.Kind.Buffer != IntPtr.Zero)
                        {
                            int length = ffiResult.Error.Depth;
                            string expectedType = length < path.Length - 1 ? "table" : "string";
                            error = new EntryMismatchError(String.Join(".", path.Take(length + 1)), expectedType, ffiResult.Error.Kind.ToString());
                        }
                        else
                        {
                            error = default(EntryMismatchError);
                        }
                    }
                }
                for (int i = 0; i < handles.Length; i++)
                    handles[i].Free();
                return result;
            }
        }

        private string[] GetStringArray(HashSet<EntryMismatchError> errors, params string[] path)
        {
            EntryMismatchError error;
            string[] value = GetStringArray(path, out error);
            if (error != null)
                errors.Add(error);
            return value;
        }

        private string[] GetStringArray(string[] path, out EntryMismatchError error)
        {
            unsafe
            {
                var handles = new GCHandle[path.Length];
                var buffers = new Utf8String[path.Length];
                for (int i = 0; i < path.Length; i++)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(path[i]);
                    handles[i] = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    buffers[i] = new Utf8String(handles[i].AddrOfPinnedObject(), buffer.Length);
                }
                string[] result;
                fixed (Utf8String* arr = buffers)
                {
                    using (StringArrayQueryResult ffiResult = Rust.Call(SafeNativeMethods.get_string_array, manifest, new RawSlice(arr, buffers.Length)))
                    {
                        result = ffiResult.Result.ToArray();
                        if (result == null && ffiResult.Error.Kind.Buffer != IntPtr.Zero)
                        {
                            int length = ffiResult.Error.Depth;
                            string expectedType = length < path.Length - 1 ? "table" : "string";
                            error = new EntryMismatchError(String.Join(".", path.Take(length + 1)), expectedType, ffiResult.Error.Kind.ToString());
                        }
                        else
                        {
                            error = default(EntryMismatchError);
                        }
                    }
                }
                for (int i = 0; i < handles.Length; i++)
                    handles[i].Free();
                return result;
            }
        }

        public void Dispose()
        {
            Rust.Invoke(SafeNativeMethods.free_manifest, this.manifest);
        }
    }
}
