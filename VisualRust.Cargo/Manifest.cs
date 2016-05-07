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
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string[] Authors { get; set; }
        public string Documentation { get; set; }
        public string Homepage { get; set; }
        public string Repository { get; set; }
        public string License { get; set; }
        public Dependency[] Dependencies { get; set; }
        public OutputTarget[] OutputTargets { get; set; }

        private Manifest(IntPtr handle)
        {
            this.manifest = handle;
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
            HashSet<EntryMismatchError> mismatchErrors = manifest.Load();
            IList<FieldMalformedError> validateErrors = manifest.Validate();
            if (mismatchErrors.Count > 0 || validateErrors.Count > 0)
            {
                loadErrors = new ManifestErrors(mismatchErrors, validateErrors);
                return null;
            }
            else
            {
                loadErrors = null;
                return manifest;
            }
        }

        public static Manifest CreateFake(string name, string version)
        {
            var manifest = new Manifest(IntPtr.Zero);
            manifest.Name = name;
            manifest.Version = version;
            manifest.Dependencies = new Dependency[0];
            manifest.OutputTargets = new OutputTarget[0];
            return manifest;
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

        HashSet<EntryMismatchError> Load()
        {
            HashSet<EntryMismatchError> errors = new HashSet<EntryMismatchError>(new MismatchComparer());
            Name = GetString(errors, "package", "name");
            Version = GetString(errors, "package", "version");
            Authors = GetStringArray(errors, "package", "authors");
            Description = GetString(errors, "package", "description");
            Documentation = GetString(errors, "package", "documentation");
            Homepage = GetString(errors, "package", "homepage");
            Repository = GetString(errors, "package", "repository");
            License = GetString(errors, "package", "license");
            Dependencies = GetDependencies(errors);
            OutputTargets = GetOutputTargets(errors);
            return errors;
        }

        IList<FieldMalformedError> Validate()
        {
            List<FieldMalformedError> errors = new List<FieldMalformedError>(2);
            if (String.IsNullOrWhiteSpace(Name))
                errors.Add(new FieldMalformedError("package.name", Name == null));
            if (String.IsNullOrWhiteSpace(Version))
                errors.Add(new FieldMalformedError("package.version", Version == null));
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

        private OutputTarget[] GetOutputTargets(HashSet<EntryMismatchError> errors)
        {
            using (OutputTargetsQueryResult result = Rust.Call(SafeNativeMethods.get_output_targets, manifest))
            {
                EntryMismatchError[] callErrors = result.Errors.ToArray();
                if (callErrors != null)
                {
                    foreach (var error in callErrors)
                        errors.Add(error);
                }
                return result.Targets.ToArray();
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
