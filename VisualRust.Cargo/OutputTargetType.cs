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
                    return String.Format(@"src\{0}.rs", name);
                case OutputTargetType.Binary:
                    return String.Format(@"src\{0}.rs", name);
                case OutputTargetType.Benchmark:
                    return String.Format(@"benches\{0}.rs", name);
                case OutputTargetType.Test:
                    return String.Format(@"tests\{0}.rs", name);
                case OutputTargetType.Example:
                    return String.Format(@"examples\{0}.rs", name);
            }
            throw new ArgumentException(null, "type");
        }

        public static bool DefaultTest(this OutputTargetType type)
        {
            switch(type)
            {
                default:
                    return true;
                case OutputTargetType.Benchmark:
                    return false;
            }
        }

        public static bool DefaultDoctest(this OutputTargetType type)
        {
            switch(type)
            {
                default:
                    return false;
                case OutputTargetType.Library:
                    return true;
            }
        }

        public static bool DefaultBench(this OutputTargetType type)
        {
            switch(type)
            {
                default:
                    return true;
                case OutputTargetType.Test:
                case OutputTargetType.Example:
                    return false;
            }
        }

        public static bool DefaultDoc(this OutputTargetType type)
        {
            switch(type)
            {
                default:
                    return false;
                case OutputTargetType.Binary:
                case OutputTargetType.Library:
                    return true;
            }
        }

        public static bool DefaultPlugin(this OutputTargetType _)
        {
            return false;
        }

        public static bool DefaultHarness(this OutputTargetType _)
        {
            return true;
        }
    }
}
