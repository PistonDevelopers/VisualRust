using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Shared
{
    public enum BuildOutputType
    {
        Application,
        Library
    }

    public static class BuildOutputTypeExtension
    {
        public static BuildOutputType Parse(string val)
        {
            if(val == null)
                return BuildOutputType.Application;
            if(val.Equals("exe", StringComparison.OrdinalIgnoreCase))
                return BuildOutputType.Application;
            if(val.Equals("library", StringComparison.OrdinalIgnoreCase))
                return BuildOutputType.Library;
            return BuildOutputType.Application;
        }

        public static string ToBuildString(this BuildOutputType val)
        {
            switch(val)
            {
                case BuildOutputType.Application:
                    return "exe";
                case BuildOutputType.Library:
                    return "library";
                default:
                    throw new ArgumentException(null, "val");
            }
        }

        public static string ToDisplayString(this BuildOutputType val)
        {
            switch(val)
            {
                case BuildOutputType.Application:
                    return "Application";
                case BuildOutputType.Library:
                    return "Library";
                default:
                    throw new ArgumentException(null, "val");
            }
        }

        public static string ToRustcString(this BuildOutputType val)
        {
            switch(val)
            {
                case BuildOutputType.Application:
                    return "bin";
                case BuildOutputType.Library:
                    return "lib";
                default:
                    throw new ArgumentException(null, "val");
            }
        }

        public static string ToCrateFile(this BuildOutputType val)
        {
            switch(val)
            {
                case BuildOutputType.Application:
                    return "main.rs";
                case BuildOutputType.Library:
                    return "lib.rs";
                default:
                    throw new ArgumentException(null, "val");
            }
        }
    }
}
