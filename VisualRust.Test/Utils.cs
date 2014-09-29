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

        // We assume that every file has structure: name.ext
        // this is obviously not true in general case, but good enough for tests
        public static TemporaryDirectory LoadResourceDirectory(string path)
        {
            string pathStart = Path.Combine(@"VisualRust\Test", path).Replace('\\', '.');
            TemporaryDirectory dir = new TemporaryDirectory();
            int lastSlash = pathStart.LastIndexOf('.');
            TemporaryDirectory actualRoot = dir.SubDir(pathStart.Substring(lastSlash + 1, pathStart.Length - lastSlash - 1));
            foreach(string resName in resourceNames.Where(p => p.StartsWith(pathStart)))
            {
                string relPath = resName.Substring(pathStart.Length, resName.Length - pathStart.Length);
                int extDot = relPath.LastIndexOf('.');
                int fileDot = relPath.LastIndexOf('.', extDot - 1);
                string subDir = relPath.Substring(0, fileDot);
                string currDir = actualRoot.DirPath;
                if(subDir.Length > 0)
                {
                    currDir = Path.Combine(actualRoot.DirPath, subDir);
                    Directory.CreateDirectory(currDir);
                }
                using(FileStream fileStream = File.Create(Path.Combine(currDir, relPath.Substring(fileDot + 1, relPath.Length - fileDot - 1))))
                {
                    using(var resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName))
                    {
                        resStream.CopyTo(fileStream);
                    }
                }
            }
            return actualRoot;
        }
    }
}
