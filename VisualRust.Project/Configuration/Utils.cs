using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project.Configuration
{
    static class Utils
    {
        public static void FromString(string text, out string val)
        {
            val = text;
        }

        public static void FromString(string text, out bool val)
        {
            val = bool.TryParse(text, out val) && val;
        }
    }
}
