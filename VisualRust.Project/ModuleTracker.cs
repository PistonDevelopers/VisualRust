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
        // We keep track of which modules can reach which modules and which modules are reachable from which modules.
        // This saves us from reparsing everything when a file is added/removed.
        private Dictionary<string, HashSet<string>> moduleImportMap = new Dictionary<string, HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, HashSet<string>> reverseModuleImportMap = new Dictionary<string, HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);

        public bool IsIncremental { get; private set; }

        public ModuleTracker(string root)
        {
            EntryPoint = root;
            fileRoots.Add(Path.GetFullPath(root));
        }

        public void AddRootModule(string root)
        {
            if (IsIncremental)
                throw new InvalidOperationException();
            string normalized = Path.GetFullPath(root);
            fileRoots.Add(normalized);
        }

        // This function extracts all reachable modules and moves to incremental mode
        public HashSet<string> ExtractReachableAndMakeIncremental()
        {
            IsIncremental = true;
            return AddModulesInternal(this.fileRoots);
        }

        private HashSet<string> AddModulesInternal(HashSet<string> modulesToParse)
        {
            HashSet<string> reachedAuthorative = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            while (modulesToParse.Count > 0)
            {
                Dictionary<string, HashSet<string>> reached = new Dictionary<string, HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);
                foreach (string root in modulesToParse)
                {
                    ExtractReachableModules(reached, reachedAuthorative, key => this.fileRoots.Contains(key), root, ReadImports);
                }
                modulesToParse = FixNonAuthorativeImports(reached, reachedAuthorative);
            }
            foreach (var kvp in moduleImportMap)
            {
                this.fileRoots.Add(kvp.Key);
            }
            return reachedAuthorative;
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
        private HashSet<string> FixNonAuthorativeImports(Dictionary<string, HashSet<string>> nonAuth, HashSet<string> authorative)
        {
            HashSet<string> newlyAuth = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var kvp in nonAuth)
            {
                string filePath = kvp.Key + ".rs";
                string subfolderPath = Path.Combine(kvp.Key, "mod.rs");
                if (authorative.Contains(filePath) || authorative.Contains(subfolderPath))
                {
                    continue;
                }
                else if (File.Exists(filePath))
                {
                    authorative.Add(filePath);
                    newlyAuth.Add(filePath);
                    foreach (string terminalImportPath in kvp.Value)
                    {
                        AddToSet(moduleImportMap, terminalImportPath, filePath);
                        AddToSet(reverseModuleImportMap, filePath, terminalImportPath);
                    }
                }
                else if (File.Exists(subfolderPath))
                {
                    authorative.Add(subfolderPath);
                    newlyAuth.Add(subfolderPath);
                    foreach (string terminalImportPath in kvp.Value)
                    {
                        AddToSet(moduleImportMap, terminalImportPath, subfolderPath);
                        AddToSet(reverseModuleImportMap, subfolderPath, terminalImportPath);
                    }
                }
                else
                {
                    authorative.Add(filePath);
                    newlyAuth.Add(filePath);
                    foreach (string terminalImportPath in kvp.Value)
                    {
                        AddToSet(moduleImportMap, terminalImportPath, filePath);
                        AddToSet(reverseModuleImportMap, filePath, terminalImportPath);
                    }
                }
            }
            return newlyAuth;
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

        private static void AddToSet(Dictionary<string, HashSet<string>> dict, string key, string value)
        {
            if(!dict.ContainsKey(key))
                dict.Add(key, new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { value });
            else
                dict[key].Add(value);
        }

        /*
         * General algorithm is:
         * # Parse given module (passed as 'importPath') and traverse its imports
         *   # for every terminal import:
         *     # if the import [is authorative] and [not a root] and [not in reachables]
         *       # add it to reachables
         *       # Recursively call ExtractReachableModules on the import
         *     # else if the import [is not authorative]
         *       # add to set of non-authorative imports
         * Algorithm returns set of pairs of non-authorative imports
         */
        private void ExtractReachableModules(Dictionary<string, HashSet<string>> reachable, 
                                                                              HashSet<string> reachableAuthorative,
                                                                              Func<string, bool> isRoot,
                                                                              string importPath,
                                                                              Func<string, ModuleImport> importReader) // the last argument is there just to make unit-testing possible
        {
            ModuleImport imports = importReader(importPath);
            foreach(PathSegment[] import in imports.GetTerminalImports())
            {
                string terminalImportPath = Path.Combine(Path.GetDirectoryName(importPath), Path.Combine(import.Select(i => i.Name).ToArray()));
                if (import[import.Length - 1].IsAuthorative)
                {
                    AddToSet(moduleImportMap, importPath, terminalImportPath);
                    AddToSet(reverseModuleImportMap, terminalImportPath, importPath);
                    if (!isRoot(terminalImportPath) && reachableAuthorative.Add(terminalImportPath))
                    {
                        ExtractReachableModules(reachable, reachableAuthorative, isRoot, terminalImportPath, importReader);
                    }
                }
                else
                {
                    // We can't compare non-authorative paths with roots
                    // Remember to call ExtractReachableModules(...) again after figuring out authorative paths
                    AddToSet(reachable, terminalImportPath, importPath);
                }
            }
        }

        private void RemoveOrphanedReverseModule(string path, string import, HashSet<string> resultSet)
        {
            var reverseSet = reverseModuleImportMap[import];
            if (reverseSet.Count == 1)
            {
                RemoveModuleInner(import, resultSet);
                resultSet.Add(import);
            }
            else
            {
                reverseSet.Remove(path);
            }
        }

        private void RemoveModuleInner(string path, HashSet<string> resultSet)
        {
            HashSet<string> imports;
            this.moduleImportMap.TryGetValue(path, out imports);
            if (imports != null && imports.Count > 0)
            {
                foreach (string import in imports.ToArray()) // copy required to avoid iterator invalidation
                {
                    RemoveOrphanedReverseModule(path, import, resultSet);
                }
            }
            HashSet<string> reverseImports;
            this.reverseModuleImportMap.TryGetValue(path, out reverseImports);
            // Go through all the modules that reference our module
            foreach(string import in reverseImports)
            {
                this.moduleImportMap[import].Remove(path);
            }
            this.moduleImportMap.Remove(path);
            this.reverseModuleImportMap.Remove(path);
        }
        
        // Returns set of modules orphaned by this removal (except the module itself)
        public HashSet<string> RemoveModule(string path)
        {
            if (!IsIncremental)
                throw new InvalidOperationException();
            var orphanSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            RemoveModuleInner(path, orphanSet);
            return orphanSet;
        }

        // Returns set of new modules referenced by this addition (except the module itself)
        public HashSet<string> AddModule(string path)
        {
            if (!IsIncremental)
                throw new InvalidOperationException();
            return AddModulesInternal(new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { path });
        }

        // Call if content of module changed
        public void Reparse(string path, ref HashSet<string> added, ref HashSet<string> removed)
        {
            if (!IsIncremental)
                throw new InvalidOperationException();
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
                        { new PathSegment("bar.rs", true), new ModuleImport() },
                        { new PathSegment("baz.rs", true), new ModuleImport() }
                    };
                var tracker = new ModuleTracker(@"C:\dev\app\src\main.rs");
                tracker.ExtractReachableModules(new Dictionary<string, HashSet<string>>(), reachable, key => roots.Contains(key), importPath, (s) => s.EndsWith("foo.rs") ? imports : new ModuleImport());
                Assert.AreEqual(2, reachable.Count);
                Assert.True(reachable.Contains(@"C:\dev\app\src\bar.rs"));
                Assert.True(reachable.Contains(@"C:\dev\app\src\baz.rs"));
                Assert.AreEqual(1, tracker.moduleImportMap.Count);
                Assert.AreEqual(2, tracker.moduleImportMap[@"C:\dev\app\src\foo.rs"].Count);
                CollectionAssert.Contains(tracker.moduleImportMap[@"C:\dev\app\src\foo.rs"], @"C:\dev\app\src\bar.rs");
                CollectionAssert.Contains(tracker.moduleImportMap[@"C:\dev\app\src\foo.rs"], @"C:\dev\app\src\baz.rs");
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
                        { new PathSegment("bar.rs", true), new ModuleImport() },
                        { new PathSegment("baz.rs", true), new ModuleImport() },
                        { new PathSegment("frob.rs", true), new ModuleImport() },
                    };
                var frobImports = new ModuleImport()
                    {
                        { new PathSegment("in1", true), new ModuleImport() {
                            { new PathSegment("in2.rs", true), new ModuleImport() }
                        }}
                    };
                var tracker = new ModuleTracker(@"C:\dev\app\src\main.rs");
                tracker.ExtractReachableModules(new Dictionary<string, HashSet<string>>(), reachable, key => roots.Contains(key), importPath, (s) => s.EndsWith("foo.rs") ? imports : s.EndsWith("frob.rs") ? frobImports : new ModuleImport());
                Assert.AreEqual(3, reachable.Count);
                Assert.True(reachable.Contains(@"C:\dev\app\src\baz.rs"));
                Assert.True(reachable.Contains(@"C:\dev\app\src\frob.rs"));
                Assert.True(reachable.Contains(@"C:\dev\app\src\in1\in2.rs"));
                Assert.AreEqual(2, tracker.moduleImportMap.Count);
                Assert.AreEqual(4, tracker.reverseModuleImportMap.Count);
                Assert.AreEqual(3, tracker.moduleImportMap[@"C:\dev\app\src\foo.rs"].Count);
                Assert.AreEqual(1, tracker.moduleImportMap[@"C:\dev\app\src\frob.rs"].Count);
                CollectionAssert.Contains(tracker.moduleImportMap[@"C:\dev\app\src\foo.rs"], @"C:\dev\app\src\bar.rs");
                CollectionAssert.Contains(tracker.moduleImportMap[@"C:\dev\app\src\foo.rs"], @"C:\dev\app\src\baz.rs");
                CollectionAssert.Contains(tracker.moduleImportMap[@"C:\dev\app\src\foo.rs"], @"C:\dev\app\src\frob.rs");
                CollectionAssert.Contains(tracker.moduleImportMap[@"C:\dev\app\src\frob.rs"], @"C:\dev\app\src\in1\in2.rs");
            }

            [Test]
            public void RemoveModule()
            {
                var reachable = new HashSet<string>();
                var roots = new HashSet<string>() { @"C:\dev\app\src\main.rs", @"C:\dev\app\src\foo.rs", @"C:\dev\app\src\bar.rs" };
                var importPath = @"C:\dev\app\src\foo.rs";
                var imports = new ModuleImport()
                    {
                        { new PathSegment("bar.rs", true), new ModuleImport() },
                        { new PathSegment("baz.rs", true), new ModuleImport() },
                        { new PathSegment("frob.rs", true), new ModuleImport() },
                    };
                var frobImports = new ModuleImport()
                    {
                        { new PathSegment("in1", true), new ModuleImport() {
                            { new PathSegment("in2.rs", true), new ModuleImport() }
                        }}
                    };
                var in2Imports = new ModuleImport()
                    {
                        { new PathSegment("ext1.rs", true), new ModuleImport() },
                        { new PathSegment("ext2.rs", true), new ModuleImport() }
                    };
                var tracker = new ModuleTracker(@"C:\dev\app\src\main.rs");
                tracker.ExtractReachableModules(new Dictionary<string, HashSet<string>>(), reachable, key => roots.Contains(key), importPath, (s) => s.EndsWith("foo.rs") ? imports : s.EndsWith("frob.rs") ? frobImports : s.EndsWith("in2.rs") ? in2Imports : new ModuleImport());
                var orphanSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                tracker.RemoveModuleInner(@"C:\dev\app\src\frob.rs", orphanSet);
                Assert.AreEqual(1, tracker.moduleImportMap.Count);
                Assert.AreEqual(2, tracker.reverseModuleImportMap.Count);
                Assert.AreEqual(2, tracker.moduleImportMap[@"C:\dev\app\src\foo.rs"].Count);
                Assert.AreEqual(3, orphanSet.Count);
                CollectionAssert.Contains(orphanSet, @"C:\dev\app\src\in1\in2.rs");
                CollectionAssert.Contains(orphanSet, @"C:\dev\app\src\in1\ext1.rs");
                CollectionAssert.Contains(orphanSet, @"C:\dev\app\src\in1\ext2.rs");
            }
        }
#endif
    }
}
