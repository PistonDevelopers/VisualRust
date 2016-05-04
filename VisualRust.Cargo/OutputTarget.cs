using System;
using System.Runtime.InteropServices;

namespace VisualRust.Cargo
{
    public class OutputTarget
    {
        public string Type { get; private set; }
        public string Name { get; private set; }
        public string Path { get; private set; }
        public bool Test { get; private set; }
        public bool Doctest { get; private set; }
        public bool Bench { get; private set; }
        public bool Doc { get; private set; }
        public bool Plugin { get; private set; }
        public bool Harness { get; private set; }

        internal OutputTarget(RawOutputTarget t)
        {
            Type = t.Type.ToString();
            Name = t.Name.ToString();
            Path = t.Path.ToString();
            Test = t.Test;
            Doctest = t.Doctest;
            Bench = t.Bench;
            Doc = t.Doc;
            Plugin = t.Plugin;
            Harness = t.Harness;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    struct RawOutputTarget
    {
        public Utf8String Type;
        public Utf8String Name;
        public Utf8String Path;
        public bool Test;
        public bool Doctest;
        public bool Bench;
        public bool Doc;
        public bool Plugin;
        public bool Harness;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RawOutputTargetArray
    {
        public IntPtr Buffer;
        public int Length;

        public OutputTarget[] ToArray()
        {
            if (Buffer == IntPtr.Zero)
                return new OutputTarget[0];
            var buffer = new OutputTarget[Length];
            for (int i = 0; i < Length; i++)
            {
                unsafe
                {
                    RawOutputTarget* current = ((RawOutputTarget*)Buffer) + i;
                    buffer[i] = new OutputTarget(*current);
                }
            }
            return buffer;
        }
    }
}