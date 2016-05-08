using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    public enum OutputTargetType
    {
        Library,
        Binary,
        Benchmark,
        Test,
        Example
    }

    public static class OutputTargetTypeExtensions
    {
        public static OutputTargetType FromString(string type)
        {
            switch(type)
            {
                case "lib":
                    return OutputTargetType.Library;
                case "bin":
                    return OutputTargetType.Binary;
                case "bench":
                    return OutputTargetType.Benchmark;
                case "test":
                    return OutputTargetType.Test;
                case "example":
                    return OutputTargetType.Example;
            }
            throw new ArgumentException(null, "type");
        }

        public static string ToTypeString(this OutputTargetType type)
        {
            switch(type)
            {
                case OutputTargetType.Library:
                    return "lib";
                case OutputTargetType.Binary:
                    return "bin";
                case OutputTargetType.Benchmark:
                    return "bench";
                case OutputTargetType.Test:
                    return "test";
                case OutputTargetType.Example:
                    return "example";
            }
            throw new ArgumentException(null, "type");
        }
        public static string DefaultPath(this OutputTargetType type, string name)
        {
            switch(type)
            {
                case OutputTargetType.Library:
                    return @"src\lib.rs";
                case OutputTargetType.Binary:
                    return @"src\main.rs";
                case OutputTargetType.Benchmark:
                    return String.Format(@"benches\{0}.rs", name);
                case OutputTargetType.Test:
                    return String.Format(@"tests\{0}.rs", name);
                case OutputTargetType.Example:
                    return String.Format(@"examples\{0}.rs", name);
            }
            throw new ArgumentException(null, "type");
        }
    }
}
