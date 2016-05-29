using System;
using System.Runtime.InteropServices;
using System.Text;

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

        internal void WithRaw(Action<RawOutputTarget> action)
        {
            byte[] type = Encoding.UTF8.GetBytes(Type.ToTypeString());
            byte[] name = Name != null ? Encoding.UTF8.GetBytes(Name) : null;
            byte[] path = Path != null ? Encoding.UTF8.GetBytes(Path) : null;
            unsafe
            {
                fixed (byte* typePtr = type)
                {
                    fixed (byte* namePtr = name)
                    {
                        fixed (byte* pathPtr = path)
                        {
                            var rawtarget = new RawOutputTarget
                            {
                                Handle = this.Handle ?? UIntPtr.Zero,
                                Type = new Utf8String(new IntPtr(typePtr), type.Length),
                                Name = new Utf8String(new IntPtr(namePtr), name != null ? name.Length : 0),
                                Path = new Utf8String(new IntPtr(pathPtr), path != null ? path.Length : 0),
                                Test = this.Test.ToTrilean(),
                                Doctest = this.Doctest.ToTrilean(),
                                Bench = this.Bench.ToTrilean(),
                                Doc = this.Doc.ToTrilean(),
                                Plugin = this.Plugin.ToTrilean(),
                                Harness = this.Harness.ToTrilean(),
                            };
                            action(rawtarget);
                        }
                    }
                }
            }
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