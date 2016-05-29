using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    enum Trilean : byte
    {
        False = 0,
        True = 1,
        Unknown = 2,
    }

    static class TrileanExtensions
    {
        public static bool? ToBool(this Trilean value)
        {
            switch(value)
            {
                case Trilean.False:
                    return false;
                case Trilean.True:
                    return true;
                case Trilean.Unknown:
                    return null;
            }
            return null;
        }
        public static Trilean ToTrilean(this bool? value)
        {
            switch(value)
            {
                case false:
                    return Trilean.False;
                case true:
                    return Trilean.True;
                case null:
                    return Trilean.Unknown;
            }
            return Trilean.Unknown;
        }
    }
}
