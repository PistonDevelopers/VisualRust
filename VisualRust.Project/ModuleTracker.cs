using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

#if TEST
using NUnit.Framework;
#endif

namespace VisualRust.Project
{
    /*
     * This class keeps track of modules.
     * Order of operations:
     * Create: .ctor()
     * Add roots: AddRoot(...)
     * Trim duplicate roots: Normalize()
     * Find modules that are not part of the project: GetReachableModules()
     */
    public class ModuleTracker
    {
        private HashSet<string> fileRoots = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public ModuleTracker(string root)
        {
            fileRoots.Add(Path.GetFullPath(root));
        }

        public void AddRoot(string root)
        {
            string normalized = Path.GetFullPath(root);
            fileRoots.Add(normalized);
        }

        private static string PathGetRelativeOrFull(string source, string target)
        {
            Contract.Assert(source.EndsWith(@"\"));
            // This is undocumented, but GetFullPath normalizes the path, eg turns c:\foo\..\bar into c:\bar
            string normalizedTarget = Path.GetFullPath(target);
            int commonLength = 0;
            int lastSlash = -1;
            for (; commonLength < Math.Min(source.Length, normalizedTarget.Length); commonLength++)
            {
                if (source[commonLength] != normalizedTarget[commonLength])
                    break;
                if (source[commonLength] == Path.DirectorySeparatorChar || source[commonLength] == Path.AltDirectorySeparatorChar)
                    lastSlash = commonLength;
            }
            string pathFromSlash = normalizedTarget.Substring(lastSlash + 1, normalizedTarget.Length - lastSlash - 1);
            if (lastSlash != -1 && commonLength != source.Length && pathFromSlash.LastIndexOf('\\') != -1)
                return @"..\" + pathFromSlash;
            else
                return pathFromSlash;
        }

        public HashSet<string> ParseReachableNonRootModules()
        {
            HashSet<string> reached = new HashSet<string>();
            foreach(string root in this.fileRoots)
            {
                ExtractReachableModules(reached, this.fileRoots, root, ReadImports);
            }
            return reached;
        }

        private static ModuleImport ReadImports(string path)
        {
            try
            {
                using(var stream = File.OpenRead(path))
                {
                    return ModuleParser.ParseImports(new AntlrInputStream(stream));
                }
            }
            catch(PathTooLongException)
            {
                throw;
            }
            catch(IOException)
            {
                return new ModuleImport();
            }
            catch(UnauthorizedAccessException)
            {
                return new ModuleImport();
            }
        }

        /*
         * General algorithm is:
         * # traverse all imports (passed as 'imports') of the given module (passed as 'importPath'):
         *   # for every terminal import:
         *     # if the import is (not a root) and (not in reachables)
         *       # add it to reachables
         *       # open the file and recursively call ExtractReachableModules
         *     # else do nothing, we will parse this import when going through roots sooner or later
         */
        private static void ExtractReachableModules(HashSet<string> reachable,
                                                    HashSet<string> roots,
                                                    string importPath,
                                                    Func<string, ModuleImport> importReader) // the last argument is there just to make unit-testing possible
        {
            ModuleImport imports = importReader(importPath);
            foreach(string[] import in imports.GetTerminalImports())
            {
                string terminalImportPath = Path.Combine(Path.GetDirectoryName(importPath), Path.Combine(import)) + ".rs";
                if (!roots.Contains(terminalImportPath) && !reachable.Contains(terminalImportPath))
                {
                    reachable.Add(terminalImportPath);
                    ExtractReachableModules(reachable, roots, terminalImportPath, importReader);
                }
            }
        }

#if TEST
        private class Test
        {
            [TestFixture]
            private class PathGetRelativeOrFull
            {
                [Test]
                public void GetPathRelative()
                {
                    Assert.AreEqual(@"file.txt", PathGetRelativeOrFull(@"C:\", @"C:\foo\..\bar\..\file.txt"));
                    Assert.AreEqual(@"src\main.rs", PathGetRelativeOrFull(@"C:\dev\", @"C:\dev\..\dev\src\main.rs"));
                }

                [Test]
                public void GetPathFull()
                {
                    Assert.AreEqual(@"D:\file.txt", PathGetRelativeOrFull(@"C:\", @"D:\foo\..\bar\..\file.txt"));
                }

                [Test]
                public void GetPathRelativeAbove()
                {
                    Assert.AreEqual(@"..\baz\file.txt", PathGetRelativeOrFull(@"C:\foo\bar\", @"C:\foo\baz\file.txt"));
                }

                [Test]
                public void Normalization()
                {
                    var tracker = new ModuleTracker(@"C:\dev\src\main.rs");
                    tracker.AddRoot(@"C:\dev\..\dev\src\main.rs");
                    Assert.AreEqual(1, tracker.fileRoots.Count);
                }
            }

            [TestFixture]
            private class ExtractReachableModules
            {
                [Test]
                public void ExtractSimple()
                {
                    // src\main.rs referenced nothing, we now scan src\foo.rs, which references two empty modules
                    var reachable = new HashSet<string>();
                    var roots = new HashSet<string>() { @"C:\dev\app\src\main.rs", @"C:\dev\app\src\foo.rs" };
                    var importPath = @"C:\dev\app\src\foo.rs";
                    var imports = new ModuleImport()
                    {
                        { "bar.rs", new ModuleImport() },
                        { "baz.rs", new ModuleImport() }
                    };
                    ExtractReachableModules(reachable, roots, importPath, (s) => s.EndsWith("foo.rs") ? imports : new ModuleImport());
                    Assert.AreEqual(2, reachable.Count);
                    Assert.True(reachable.Contains(@"C:\dev\app\src\bar.rs"));
                    Assert.True(reachable.Contains(@"C:\dev\app\src\baz.rs"));
                }


                [Test]
                public void ExtractNested()
                {
                    // src\main.rs referenced nothing, we now scan src\foo.rs, which references three modules
                    var reachable = new HashSet<string>();
                    var roots = new HashSet<string>() { @"C:\dev\app\src\main.rs", @"C:\dev\app\src\foo.rs", @"C:\dev\app\src\bar.rs" };
                    var importPath = @"C:\dev\app\src\foo.rs";
                    var imports = new ModuleImport()
                    {
                        { "bar.rs", new ModuleImport() },
                        { "baz.rs", new ModuleImport() },
                        { "frob.rs", new ModuleImport() },
                    };
                    var frobImports = new ModuleImport()
                    {
                        { "in1", new ModuleImport() {
                            { "in2.rs", new ModuleImport() }
                        }}
                    };
                    ExtractReachableModules(reachable, roots, importPath, (s) => s.EndsWith("foo.rs") ? imports : s.EndsWith("frob.rs") ? frobImports : new ModuleImport());
                    Assert.AreEqual(3, reachable.Count);
                    Assert.True(reachable.Contains(@"C:\dev\app\src\baz.rs"));
                    Assert.True(reachable.Contains(@"C:\dev\app\src\frob.rs"));
                    Assert.True(reachable.Contains(@"C:\dev\app\src\in1\in2.rs"));
                }
            }
        }
#endif
    }
}
