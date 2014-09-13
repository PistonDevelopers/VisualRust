using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Test
{
    static class Utils
    {
        private static string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        public static string LoadResource(string path)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceNames.First(s => s.EndsWith(path.Replace(Path.DirectorySeparatorChar, '.')))))
            {
                using(StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
