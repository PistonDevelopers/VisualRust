using System;
using System.Runtime.InteropServices;

namespace VisualRust.Cargo
{
    public class OutputTarget
    {
        public OutputTargetType Type { get; set; }
        public UIntPtr? Handle { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public bool? Test { get; set; }
        public bool? Doctest { get; set; }
        public bool? Bench { get; set; }
        public bool? Doc { get; set; }
        public bool? Plugin { get; set; }
        public bool? Harness { get; set; }

        internal OutputTarget(RawOutputTarget t)
        {
            Type = OutputTargetTypeExtensions.FromString(t.Type.ToString());
            Handle = t.Handle;
            Name = t.Name.ToString();
            Path = t.Path.ToString();
            Test = t.Test.ToBool();
            Doctest = t.Doctest.ToBool();
            Bench = t.Bench.ToBool();
            Doc = t.Doc.ToBool();
            Plugin = t.Plugin.ToBool();
            Harness = t.Harness.ToBool();
        }

        public OutputTarget(OutputTargetType type)
        {
            Type = type;
            Handle = null;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    struct RawOutputTarget
    {
        public UIntPtr Handle;
        public Utf8String Type;
        public Utf8String Name;
        public Utf8String Path;
        public Trilean Test;
        public Trilean Doctest;
        public Trilean Bench;
        public Trilean Doc;
        public Trilean Plugin;
        public Trilean Harness;
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