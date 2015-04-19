using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Shared
{
    public enum OptimizationLevel
    {
        O0,
        O1,
        O2,
        O3
    }

    public static class OptimizationLevelExtension
    {
        public static OptimizationLevel Parse(string val)
        {
            if(val == null)
                return OptimizationLevel.O0;
            if(val.Equals("0", StringComparison.OrdinalIgnoreCase))
                return OptimizationLevel.O0;
            if(val.Equals("1", StringComparison.OrdinalIgnoreCase))
                return OptimizationLevel.O1;
            if(val.Equals("2", StringComparison.OrdinalIgnoreCase))
                return OptimizationLevel.O2;
            if(val.Equals("3", StringComparison.OrdinalIgnoreCase))
                return OptimizationLevel.O3;
            return OptimizationLevel.O0;
        }

        public static string ToBuildString(this OptimizationLevel val)
        {
            switch(val)
            {
                case OptimizationLevel.O0:
                    return "0";
                case OptimizationLevel.O1:
                    return "1";
                case OptimizationLevel.O2:
                    return "2";
                case OptimizationLevel.O3:
                    return "3";
                default:
                    throw new ArgumentException("val");
            }
        }

        public static string ToDisplayString(this OptimizationLevel val)
        {
            switch(val)
            {
                case OptimizationLevel.O0:
                    return "none (O0)";
                case OptimizationLevel.O1:
                    return "minimal (O1)";
                case OptimizationLevel.O2:
                    return "optimized (O2)";
                case OptimizationLevel.O3:
                    return "aggresive (O3)";
                default:
                    throw new ArgumentException("val");
            }
        }
    }
}
