using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    [StructLayout(LayoutKind.Sequential)]
    struct RawDependency
    {
        public Utf8String Name;
        public Utf8String Version;
        public Utf8String Git;
        public Utf8String Path;
        public Utf8String Target;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RawDependencyArray
    {
        public IntPtr Buffer;
        public int Length;

        public Dependency[] ToArray()
        {
            if (Buffer == IntPtr.Zero)
                return null;
            var buffer = new Dependency[Length];
            for (int i = 0; i < Length; i++)
            {
                unsafe
                {
                    RawDependency* current = ((RawDependency*)Buffer) + i;
                    buffer[i] = new Dependency(*current);
                }
            }
            return buffer;
        }
    }

    public class Dependency
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Git { get; private set; }
        public string Path { get; private set; }
        public string Target { get; private set; }

        internal Dependency(RawDependency r)
        {
            Name = r.Name.ToString();
            Version = r.Version.ToString();
            Git = r.Git.ToString();
            Path = r.Path.ToString();
            Target = r.Target.ToString();
        }
    }
}
