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
    public class ModuleTracker
    {
        public string EntryPoint { get; private set; }

        private HashSet<string> fileRoots = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public ModuleTracker(string root)
        {
            EntryPoint = root;
            fileRoots.Add(Path.GetFullPath(root));
        }

        public void AddRoot(string root)
        {
            string normalized = Path.GetFullPath(root);
            fileRoots.Add(normalized);
        }

        public HashSet<string> ParseReachableNonRootModules()
        {
            HashSet<string> reached = new HashSet<string>();
            HashSet<string> reachedAuthorative = new HashSet<string>();
            foreach(string root in this.fileRoots)
            {
                ExtractReachableModules(reached, reachedAuthorative, this.fileRoots, root, ReadImports);
            }

            /*
             * Reached non-authorative module imports are standard stuff like mod foo { mod bar; }
             * (as opposed to mod foo { #[path="bar.rs"] bar; }).
             * Now, for every terminal module import foo/bar we find out actual backing file in this order:
             * Check for authorative entry foo/bar.rs
             * Check for authorative entry foo/bar/mod.rs
             * Check on the disk for foo/bar.rs
             * Check on the disk for foo/bar/mod.rs
             * If all of the above fails go with broken foo/bar.rs
             */
            foreach(string file in reached)
            {
                string filePath = file + ".rs";
                string subfolderPath = Path.Combine(file, "mod.rs");
                if (reachedAuthorative.Contains(filePath) || reachedAuthorative.Contains(subfolderPath))
                {
                    continue;
                }
                else if (File.Exists(filePath))
                {
                    reachedAuthorative.Add(filePath);
                }
                else if(File.Exists(subfolderPath))
                {
                    reachedAuthorative.Add(subfolderPath);
                }
                else
                {
                    reachedAuthorative.Add(filePath);
                }
            }
            return reachedAuthorative;
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
                                                    HashSet<string> reachableAuthorative,
                                                    HashSet<string> roots,
                                                    string importPath,
                                                    Func<string, ModuleImport> importReader) // the last argument is there just to make unit-testing possible
        {
            ModuleImport imports = importReader(importPath);
            foreach(PathSegment[] import in imports.GetTerminalImports())
            {
                string terminalImportPath = Path.Combine(Path.GetDirectoryName(importPath), Path.Combine(import.Select(i => i.Name).ToArray()));
                if (!roots.Contains(terminalImportPath) && !reachable.Contains(terminalImportPath))
                {
                    if(import[import.Length - 1].IsAuthorative)
                        reachableAuthorative.Add(terminalImportPath);
                    else
                        reachable.Add(terminalImportPath);
                    ExtractReachableModules(reachable, reachableAuthorative, roots, terminalImportPath, importReader);
                }
            }
        }

#if TEST
        [TestFixture]
        private class Test
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
                        { new PathSegment("bar.rs"), new ModuleImport() },
                        { new PathSegment("baz.rs"), new ModuleImport() }
                    };
                ExtractReachableModules(reachable, new HashSet<string>(), roots, importPath, (s) => s.EndsWith("foo.rs") ? imports : new ModuleImport());
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
                        { new PathSegment("bar.rs"), new ModuleImport() },
                        { new PathSegment("baz.rs"), new ModuleImport() },
                        { new PathSegment("frob.rs"), new ModuleImport() },
                    };
                var frobImports = new ModuleImport()
                    {
                        { new PathSegment("in1"), new ModuleImport() {
                            { new PathSegment("in2.rs"), new ModuleImport() }
                        }}
                    };
                ExtractReachableModules(reachable, new HashSet<string>(), roots, importPath, (s) => s.EndsWith("foo.rs") ? imports : s.EndsWith("frob.rs") ? frobImports : new ModuleImport());
                Assert.AreEqual(3, reachable.Count);
                Assert.True(reachable.Contains(@"C:\dev\app\src\baz.rs"));
                Assert.True(reachable.Contains(@"C:\dev\app\src\frob.rs"));
                Assert.True(reachable.Contains(@"C:\dev\app\src\in1\in2.rs"));
            }
        }
#endif
    }
}
