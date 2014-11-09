using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.Contracts;

#if TEST
using NUnit.Framework;
using System.Diagnostics;
#endif

using ImportsChange = VisualRust.Project.CollectionChange<System.Collections.Generic.HashSet<string>>;
using ImportsDifference = VisualRust.Project.CollectionDifference<System.Collections.Generic.HashSet<string>>;

namespace VisualRust.Project
{
    struct CollectionChange<T>
    {
        public T Old { get; private set; }
        public T New { get; private set; }
        public CollectionChange(T old, T @new)
            : this()
        {
            Old = old;
            New = @new;
        }
    }

    public class ModuleTracker
    {
        public string EntryPoint { get; private set; }
        // Set of tracked files with enabled auto-imports
        private HashSet<string> fileRoots = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        // Set of modules with disabled auto-imports, this is relevant in some cases
        private HashSet<string> blockingRoots = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        // We keep track of which modules can reach which modules and which modules are reachable from which modules.
        // This saves us from reparsing everything when a file is added/removed.
        private Dictionary<string, HashSet<string>> moduleImportMap = new Dictionary<string, HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, HashSet<string>> reverseModuleImportMap = new Dictionary<string, HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);
        // I'm not very happy about this but we keep it to avoid molesting disk on every reparse
        private Dictionary<string, ModuleImport> lastParseResult = new Dictionary<string, ModuleImport>(StringComparer.InvariantCultureIgnoreCase);

        public bool IsIncremental { get; private set; }

        public ModuleTracker(string root)
        {
            EntryPoint = root;
            fileRoots.Add(Path.GetFullPath(root));
        }

        public void AddRootModule(string root)
        {
            Contract.Requires(!IsIncremental);
            Contract.Requires(root != null);
            string normalized = Path.GetFullPath(root);
            fileRoots.Add(normalized);
        }

        // This function extracts all reachable modules and moves to incremental mode
        public HashSet<string> ExtractReachableAndMakeIncremental()
        {
            Contract.Requires(!IsIncremental);
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
                    if (this.moduleImportMap.ContainsKey(root))
                        continue;
                    ExtractReachableModules(reached, reachedAuthorative, key => this.fileRoots.Contains(key), root, ReadImports);
                }
                modulesToParse = FixNonAuthorativeImports(reached, reachedAuthorative);
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
        private HashSet<string> FixNonAuthorativeImports(Dictionary<string, HashSet<string>> nonAuth,
                                                         HashSet<string> justParsed)
        {
            HashSet<string> newlyAuth = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var kvp in nonAuth)
            {
                string authPath;
                if(!ResolveAuthorativeImport(kvp.Key, justParsed, out authPath))
                {
                    justParsed.Add(authPath);
                    newlyAuth.Add(authPath);
                }
                AddToModuleSets(kvp.Value, authPath);
            }
            return newlyAuth;
        }

        // return value indicates if resolution found module in existing modules
        private bool ResolveAuthorativeImport(string nonAuthPath,
                                              HashSet<string> justParsed,
                                              out string path)
        {
            string filePath = nonAuthPath + ".rs";
            string subfolderPath = Path.Combine(nonAuthPath, "mod.rs");
            if (justParsed.Contains(filePath) || fileRoots.Contains(filePath) || reverseModuleImportMap.ContainsKey(filePath))
            {
                path = filePath;
                return true;
            }
            else if (justParsed.Contains(subfolderPath) || fileRoots.Contains(subfolderPath) || reverseModuleImportMap.ContainsKey(subfolderPath))
            {
                path = subfolderPath;
                return true;
            }
            else if (File.Exists(filePath))
            {
                path = filePath;
                return false;
            }
            else if (File.Exists(subfolderPath))
            {
                path = subfolderPath;
                return false;
            }
            else
            {
                path = filePath;
                return false;
            }
        }

        private void AddToModuleSets(HashSet<string> set, string filePath)
        {
            foreach (string terminalImportPath in set)
            {
                if (String.Equals(terminalImportPath, filePath, StringComparison.OrdinalIgnoreCase))
                    continue;
                AddToSet(moduleImportMap, terminalImportPath, filePath);
                AddToSet(reverseModuleImportMap, filePath, terminalImportPath);
            }
        }

        private ModuleImport ReadImports(string path)
        {
            ModuleImport imports = ReadImportsRaw(path);
            lastParseResult[path] = imports;
            return imports;
        }

        private ModuleImport ReadImportsRaw(string path)
        {
            if (blockingRoots.Contains(path))
                return new ModuleImport();
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

        private static bool DeleteFromSet(Dictionary<string, HashSet<string>> dict, string key, string value)
        {
            HashSet<string> s;
            if(dict.TryGetValue(key, out s))
            {
                bool wasRemoved = s.Remove(value);
                if(wasRemoved && s.Count == 0)
                    dict.Remove(key);
                return wasRemoved;
            }
            return false;
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
                string terminalImportPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(importPath), Path.Combine(import.Select(i => i.Name).ToArray())));
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

        private static void Increment(Dictionary<string, int> dict, string key)
        {
            int current;
            dict.TryGetValue(key, out current);
            dict[key] = current + 1;
        }

        private static IEnumerable<string> GetSetFromDictionary(Dictionary<string, HashSet<string>> dict, string key)
        {
            HashSet<string> set;
            if(dict.TryGetValue(key, out set))
                return set;
            else
                return new string[0] { };
        }

        private void TraverseForDependants(string current, string root, HashSet<string> nodes, bool delete, ref bool refFromOutside)
        {
            if (refFromOutside && !delete)
                return;
            if (String.Equals(current, root, StringComparison.InvariantCultureIgnoreCase))
            {
                refFromOutside = true;
                return;
            }
            if(fileRoots.Contains(current)
                || !nodes.Remove(current))
                return;
            foreach (string mod in GetSetFromDictionary(moduleImportMap, current))
                TraverseForDependants(mod, root, nodes, delete, ref refFromOutside);
        }

        // Returns all the module that are reachable only from the given root
        private HashSet<string> CalculateDependants(string source, bool delete, out bool refFromOutside)
        {
            // FIXME: that could use some optimization
            // We simply traverse the graph starting from the the other roots, storing all traversed edges.
            // All untraversed nodes depend on this root
            HashSet<string> nodes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach(var pair in reverseModuleImportMap)
            {
                if (!fileRoots.Contains(pair.Key))
                    nodes.Add(pair.Key);
            }
            refFromOutside = false;
            foreach (string mod in fileRoots.Where(s => !s.Equals(source, StringComparison.InvariantCultureIgnoreCase)).SelectMany(r => GetSetFromDictionary(moduleImportMap, r)))
            {
                TraverseForDependants(mod, source, nodes, delete, ref refFromOutside);
            }
            if (delete && refFromOutside)
                nodes.Remove(source);
            if (!refFromOutside)
                nodes.Add(source);
            return nodes;
        }

        private HashSet<string> CalculateDependantsInner(HashSet<string> seen,
                                                         Dictionary<string, int> degrees,
                                                         string currentNode,
                                                         string root,
                                                         ref int circles)
        {
            seen.Add(currentNode);
            HashSet<string> children = null;
            if(this.moduleImportMap.TryGetValue(currentNode, out children))
            {
                foreach(string child in children)
                {
                    if (String.Equals(root, child, StringComparison.InvariantCultureIgnoreCase))
                    {
                        circles++;
                        continue;
                    }
                    if (this.fileRoots.Contains(child))
                        continue;
                    Increment(degrees, child);
                    if(!seen.Contains(child))
                        CalculateDependantsInner(seen, degrees, child, root, ref circles);
                }
            }
            return children;
        }

        private void DeleteModuleData(string mod)
        {
            HashSet<string> importSet;
            moduleImportMap.TryGetValue(mod, out importSet);
            if(importSet != null)
            {
                foreach(string child in importSet)
                {
                    DeleteFromSet(reverseModuleImportMap, child, mod);
                }
            }
            HashSet<string> reverseImportSet;
            reverseModuleImportMap.TryGetValue(mod, out reverseImportSet);
            if(reverseImportSet != null)
            {
                foreach(string parent in reverseImportSet)
                {
                    DeleteFromSet(moduleImportMap, mod, parent);
                }
            }
            moduleImportMap.Remove(mod);
            reverseModuleImportMap.Remove(mod);
            lastParseResult.Remove(mod);
        }

        // Returns set of modules orphaned by this deletion (including the module itself)
        // and if the module is still referenced by another non-removed module
        public ModuleRemovalResult DeleteModule(string path)
        {
            Contract.Requires(IsIncremental);
            Contract.Requires(path != null);
            bool referencedFromOutside;
            HashSet<string> markedForRemoval = CalculateDependants(path, true, out referencedFromOutside);
            foreach(string mod in markedForRemoval)
            {
                DeleteModuleData(mod);
            }
            moduleImportMap.Remove(path);
            if (!referencedFromOutside)
                lastParseResult.Remove(path);
            else
                lastParseResult[path] = new ModuleImport();
            this.fileRoots.Remove(path);
            return new ModuleRemovalResult(markedForRemoval, referencedFromOutside);
        }

        // Returns set of new modules referenced by this addition (except the module itself)
        public HashSet<string> AddRootModuleIncremental(string path)
        {
            Contract.Requires(IsIncremental);
            Contract.Requires(path != null);
            fileRoots.Add(path);
            HashSet<string> result = AddModulesInternal(new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { path });
            return result;
        }

        public void UpgradeModule(string path)
        {
            Contract.Requires(IsIncremental);
            Contract.Requires(path != null);
            Contract.Assert(!this.fileRoots.Contains(path));
            fileRoots.Add(path);
        }

        // When a module and all its children form a strongly connected component
        // downgrading a module can orphan some modules
        public ModuleRemovalResult DowngradeModule(string path)
        {
            Contract.Requires(IsIncremental);
            Contract.Requires(path != null);
            Contract.Assert(this.fileRoots.Contains(path));
            bool referencedFromOutside;
            HashSet<string> dependingOnRoot = CalculateDependants(path, false, out referencedFromOutside);
            fileRoots.Remove(path);
            if (referencedFromOutside)
                return new ModuleRemovalResult(new HashSet<string>(StringComparer.InvariantCultureIgnoreCase), true);
            foreach(string mod in dependingOnRoot)
            {
                DeleteModuleData(mod);
            }
            return new ModuleRemovalResult(dependingOnRoot, false);
        }

        public HashSet<string> DisableTracking(string path)
        {
            Contract.Requires(!IsIncremental);
            Contract.Requires(path != null);
            Contract.Assert(this.fileRoots.Contains(path));
            throw new NotImplementedException();
        }

        public HashSet<string> EnableTracking(string path)
        {
            Contract.Requires(IsIncremental);
            Contract.Requires(path != null);
            Contract.Assert(this.fileRoots.Contains(path));
            throw new NotImplementedException();
        }

        // HACK ALERT: We exploit the fact that passed pair of sets is not used afterwards
        private static ImportsDifference DiffReparseSets(string root, ImportsChange change)
        {
            HashSet<string> removed = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach(string path in change.Old)
            {
                if (!change.New.Remove(path))
                {
                    removed.Add(path);
                }
            }
            return new ImportsDifference(change.New, removed);
        }

        private void NormalizeSingleImport(string rootDir, PathSegment[] segs, HashSet<string> added, bool addResolved)
        {
            string rootedPath = Path.Combine(rootDir, String.Join(Path.DirectorySeparatorChar.ToString(), segs));
            if (!segs[segs.Length - 1].IsAuthorative)
            {
                string validImport;
                ResolveAuthorativeImport(rootedPath, added, out validImport);
                added.Add(validImport);
            }
        }

        private ImportsChange NormalizeImports(string root, ModuleImport oldI, ModuleImport newI)
        {
            string rootDir = Path.GetDirectoryName(root);
            HashSet<string> added = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach(PathSegment[] segs in oldI.GetTerminalImports())
            {
                NormalizeSingleImport(rootDir, segs, added, true);
            }
            HashSet<string> removed = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (PathSegment[] segs in newI.GetTerminalImports())
            {
                NormalizeSingleImport(rootDir, segs, removed, false);
            }
            return new ImportsChange(added, removed);
        }

        /* 
         * This function is called when during reparsing
         * a set of module imports is removed.
         * We have to check if it's safe to remove those node,
         * that is if there is no other path through which this node
         * can reach a root. For example:
         * [main] ---> (bar) <--- foo <--- (baz)
         *   └---------------------^--------^
         * Main is a reparsed file and bar and baz have been "removed".
         * We start by removing references main --> bar and main --> baz
         * (this is done by an outside function):
         * [main]      (bar) <--- foo <--- (baz)
         *   └---------------------^
         * Then, for every node in the set of removed nodes we backtrack.
         * If we can reach the target node or a root, that means the node is not removed.
         * For example with bar we go through foo, then reach main, which means that it stays.
         * For baz we can't find such path (or in fact any other) which means that baz goes away.
         */
        private bool AdditionalImportPathExists(string current, string target, HashSet<string> removed, HashSet<string> seen)
        {
            if (!seen.Add(current))
                return false;
            HashSet<string> children;
            if(reverseModuleImportMap.TryGetValue(current, out children))
            {
                foreach(string child in children)
                {
                    if (removed.Contains(child))
                        continue;
                    if (String.Equals(child, target, StringComparison.InvariantCultureIgnoreCase) || fileRoots.Contains(child))
                        return true;
                }
                return children.Any(child => AdditionalImportPathExists(child, target, removed, seen));
            }
            return false;
        }

        public ImportsDifference Reparse(string path)
        {
            ModuleImport oldImports = lastParseResult[path];
            ModuleImport newImports = ReadImports(path);
            ImportsChange normalized = NormalizeImports(path, oldImports, newImports);
            ImportsDifference diff = DiffReparseSets(path, normalized);
            HashSet<string> removedFromProject = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            if(diff.Removed.Count > 0)
            {
                HashSet<string> allImports = moduleImportMap[path];
                foreach (string mod in diff.Removed)
                {
                    if (allImports.Remove(mod))
                    {
                        bool removed = DeleteFromSet(reverseModuleImportMap, mod, path);
                        Debug.Assert(removed);
                    }
                    if(allImports.Count == 0)
                        moduleImportMap.Remove(path);
                }
                foreach(string mod in diff.Removed)
                {
                    if (!AdditionalImportPathExists(mod, path, diff.Removed, new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)))
                        removedFromProject.Add(mod);
                }
                foreach(string mod in removedFromProject.ToArray())
                {
                    foreach (string removedMod in DeleteModule(mod).Orphans)
                        removedFromProject.Add(removedMod);
                }
            }
            HashSet<string> addedFromParsing = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach(string mod in diff.Added)
            {
                AddToSet(moduleImportMap, path, mod);
                AddToSet(reverseModuleImportMap, mod, path);
                if(!lastParseResult.ContainsKey(mod))
                {
                    addedFromParsing.Add(mod);
                }
            }
            addedFromParsing.UnionWith(AddModulesInternal(diff.Added));
            return new ImportsDifference(addedFromParsing, removedFromProject);
        }

#if TEST

        private static bool AreEqual<T>(Dictionary<string, ModuleImport> d1, Dictionary<string, ModuleImport> d2)
        {
            return d1.Count == d2.Count && d1.All(x => AreEqual(x.Value, d2[x.Key]));
        }

        private static bool AreEqual(ModuleImport m1, ModuleImport m2)
        {
            return m1.Count == m2.Count && m1.All(x => AreEqual(x.Value, m2[x.Key]));
        }

        private static bool AreEqual<T>(Dictionary<string, HashSet<T>> d1, Dictionary<string, HashSet<T>> d2)
        {
            return d1.Count == d2.Count && d1.All(x => AreEqual(x.Value, d2[x.Key]));
        }

        private static bool AreEqual<T>(HashSet<T> s1, HashSet<T> s2)
        {
            return s1.Count == s2.Count && s1.All(x => s2.Contains(x));
        }

        // It's formatted this way to make unit test failures more readable
        public bool IsEquivalnet(ModuleTracker other)
        {
            if (EntryPoint != other.EntryPoint)
                return false;
            if (!AreEqual(this.fileRoots, other.fileRoots))
                return false;
            if (!AreEqual(this.blockingRoots, other.blockingRoots))
                return false;
            if (!AreEqual(this.moduleImportMap, other.moduleImportMap))
                return false;
            if (!AreEqual(this.reverseModuleImportMap, other.reverseModuleImportMap))
                return false;
            if (!AreEqual<string>(this.lastParseResult, other.lastParseResult))
                return false;
            return true;
        }

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
            public void RemoveCircular()
            {
                var reachable = new HashSet<string>();
                var roots = new HashSet<string>() { @"C:\dev\app\src\main.rs", @"C:\dev\app\src\foo.rs"};
                var imports = new ModuleImport()
                    {
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
                        { new PathSegment("ext2.rs", true), new ModuleImport() },
                        { new PathSegment(@"..\frob.rs", true), new ModuleImport() },
                        { new PathSegment(@"..\main.rs", true), new ModuleImport() },
                        { new PathSegment(@"..\foo.rs", true), new ModuleImport() },
                    };
                var tracker = new ModuleTracker(@"C:\dev\app\src\main.rs");
                foreach(string path in roots)
                    tracker.AddRootModule(path);
                tracker.ExtractReachableAndMakeIncremental();
                var orphanSet = tracker.DeleteModule(@"C:\dev\app\src\in1\in2.rs");
            }
        }
#endif
    }
}
