using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    static class EnvironmentPath
    {
        public static string FindExePath(string name)
        {
            string extensionsVariable = Environment.GetEnvironmentVariable("PATHEXT") ?? ".COM;.EXE";
            string[] extensions = extensionsVariable.Split(new [] {";"}, StringSplitOptions.RemoveEmptyEntries);
            string[] fileNames = extensions.Where(s => s.StartsWith(".")).Select(e => name + e).ToArray();
            foreach(string path in SplitPaths(Environment.GetEnvironmentVariable("PATH") ?? ""))
            {
                foreach(string file in fileNames)
                {
                    string fullPath = Path.Combine(path,  file);
                    if(File.Exists(fullPath))
                        return fullPath;
                }
            }
            return null;
        }

        private static List<string> SplitPaths(string path)
        {
            int i = 0;
            List<string> result = new List<string>();
            while(i < path.Length)
            {
                int start = i;
                int end;
                if(path[start] == '"')
                {
                    start++;
                    end = path.IndexOf('"', start);
                    if (end == -1)
                    {
                        end = path.Length;
                        i = path.Length;
                    }
                    else
                    {
                        int semi = path.IndexOf(';', end);
                        if(semi == -1)
                            i = path.Length;
                        else
                            i = semi + 1;
                    }
                }
                else
                {
                    end = path.IndexOf(';', start);
                    if (end == -1)
                    {
                        end = path.Length;
                        i = path.Length;
                    }
                    else
                    {
                        i = end + 1;
                    }
                }
                result.Add(path.Substring(start, end - start));
            }
            return result;
        }

        #if TEST
        [TestFixture]
        private class Test
        {
            [Test]
            public void SplitPathsPlain()
            {
                CollectionAssert.AreEquivalent(
                    new String[] { @"D:\dev\Rust\bin", @"D:\dev\LLVM\bin" },
                    SplitPaths(@"D:\dev\LLVM\bin;D:\dev\Rust\bin"));
            }

            [Test]
            public void SplitPathsSingleChars()
            {
                CollectionAssert.AreEquivalent(
                    new String[] { "C", "D" },
                    SplitPaths("C;D"));
            }

            [Test]
            public void SplitPathsExtraSemicolon()
            {
                CollectionAssert.AreEquivalent(
                    new String[] { @"D:\dev\Rust\bin", @"D:\dev\LLVM\bin" },
                    SplitPaths(@"D:\dev\LLVM\bin;D:\dev\Rust\bin;"));
            }

            [Test]
            public void SplitPathsQuoted()
            {
                CollectionAssert.AreEquivalent(
                    new String[] { @"D:\dev\LLVM\bin", @"C:\main() {printf('%d', 42);}" },
                    SplitPaths(@"D:\dev\LLVM\bin;""C:\main() {printf('%d', 42);}"""));
            }

            [Test]
            public void SplitPathsQuotedExtraSemicolon()
            {
                CollectionAssert.AreEquivalent(
                    new String[] { @"D:\dev\LLVM\bin", @"C:\main() {printf('%d', 42);}" },
                    SplitPaths(@"D:\dev\LLVM\bin;""C:\main() {printf('%d', 42);}"";"));
            }
        }
        #endif
    }
}
