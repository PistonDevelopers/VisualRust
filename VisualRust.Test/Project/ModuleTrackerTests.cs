using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Project;

namespace VisualRust.Test.Project
{
    public class ModuleTrackerTests
    {
        private static void ModelCheck(ModuleTracker t, string root, params string[] additionalRoots)
        {
            ModuleTracker model = new ModuleTracker(root);
            foreach (string r in additionalRoots)
                model.AddRootModule(r);
            model.ExtractReachableAndMakeIncremental();
            Assert.True(t.IsEquivalnet(model));
        }

        [TestFixture]
        public class DeleteModule
        {
            // [main] --> foo --> baz
            [Test]
            public void ChainedRemoval()
            {
                using(TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\SimpleChain"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(2, reached.Count);
                    CollectionAssert.Contains(reached, Path.Combine(temp.DirPath, "foo.rs"));
                    CollectionAssert.Contains(reached, Path.Combine(temp.DirPath, "baz.rs"));
                    var res = tracker.DeleteModule(Path.Combine(temp.DirPath, "foo.rs"));
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                    Assert.AreEqual(1, res.Orphans.Count);
                    CollectionAssert.Contains(res.Orphans, Path.Combine(temp.DirPath, "baz.rs"));
                }
            }

            /*
             * [main] --> foo
             *   ^---------┘
             */
            [Test]
            public void EscapedPaths()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\CircularNested"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(1, reached.Count);
                    CollectionAssert.Contains(reached, Path.Combine(temp.DirPath, "in\\foo.rs"));
                }
            }

            /*
             * [main]     [foo] --> bar
             *   ^-------------------┘
             */
            [Test]
            public void CircularAddRemove()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\CircularAdd"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(0, reached.Count);
                    HashSet<string> added = tracker.AddRootModuleIncremental(Path.Combine(temp.DirPath, "foo.rs"));
                    CollectionAssert.Contains(added, Path.Combine(temp.DirPath, "bar.rs"));
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"), Path.Combine(temp.DirPath, "foo.rs"));
                    var rem = tracker.DeleteModule(Path.Combine(temp.DirPath, "foo.rs"));
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    Assert.AreEqual(2, rem.Orphans.Count);
                    CollectionAssert.Contains(rem.Orphans, Path.Combine(temp.DirPath, "bar.rs"));
                    CollectionAssert.Contains(rem.Orphans, Path.Combine(temp.DirPath, "foo.rs"));
                    Assert.False(rem.IsReferenced);
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                }
            }

            /*
             * [main]     [foo] --> bar
             *   ^-------------------┘
             */
            [Test]
            public void CircularAddUnroot()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\CircularAdd"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(0, reached.Count);
                    HashSet<string> added = tracker.AddRootModuleIncremental(Path.Combine(temp.DirPath, "foo.rs"));
                    CollectionAssert.Contains(added, Path.Combine(temp.DirPath, "bar.rs"));
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"), Path.Combine(temp.DirPath, "foo.rs"));
                    var rem = tracker.DowngradeModule(Path.Combine(temp.DirPath, "foo.rs"));
                    CollectionAssert.Contains(rem.Orphans, Path.Combine(temp.DirPath, "foo.rs"));
                    CollectionAssert.Contains(rem.Orphans, Path.Combine(temp.DirPath, "bar.rs"));
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                }
            }

            /*
             * [main] --> foo --> bar
             *   ^-----------------┘
             */
            [Test]
            public void ExplicitlyAddRemoveExisting()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\CircularDowngrade"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(2, reached.Count);
                    HashSet<string> added = tracker.AddRootModuleIncremental(Path.Combine(temp.DirPath, "foo.rs"));
                    Assert.AreEqual(0, added.Count);
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"), Path.Combine(temp.DirPath, "foo.rs"));
                    var del = tracker.DeleteModule(Path.Combine(temp.DirPath, "foo.rs"));
                    Assert.AreEqual(1, del.Orphans.Count);
                    Assert.True(del.IsReferenced);
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                }
            }

            /*
             * [main] --> foo --> bar
             *   ^-----------------┘
             */
            [Test]
            public void ExplicitlyAddUnrootExisting()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\CircularDowngrade"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(2, reached.Count);
                    HashSet<string> added = tracker.AddRootModuleIncremental(Path.Combine(temp.DirPath, "foo.rs"));
                    Assert.AreEqual(0, added.Count);
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"), Path.Combine(temp.DirPath, "foo.rs"));
                    var unr = tracker.DowngradeModule(Path.Combine(temp.DirPath, "foo.rs"));
                    Assert.AreEqual(0, unr.Orphans.Count);
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                    var del = tracker.DeleteModule(Path.Combine(temp.DirPath, "foo.rs"));
                    Assert.AreEqual(1, del.Orphans.Count);
                    Assert.True(del.IsReferenced);
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                }
            }

            /*
             * [lib] --> [foo] --> bar
             *   ^------------------┘
             */
            [Test]
            public void NonIncrRootRemoval()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\CircularMultiRoot"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "lib.rs"));
                    tracker.AddRootModule(Path.Combine(temp.DirPath, "foo.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(1, reached.Count);
                    HashSet<string> orphans = tracker.DowngradeModule(Path.Combine(temp.DirPath, "foo.rs")).Orphans;
                    Assert.AreEqual(2, orphans.Count);
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "lib.rs"));
                }
            }

            /*
             * [main]     [lib] <-> foo <-> bar
             *              ^----------------^
             */
            [Test]
            public void CircularHard()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\Circular"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(0, reached.Count);
                    var added = tracker.AddRootModuleIncremental(Path.Combine(temp.DirPath, "foo.rs"));
                    Assert.AreEqual(2, added.Count);
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"), Path.Combine(temp.DirPath, "foo.rs"));
                    var del = tracker.DeleteModule(Path.Combine(temp.DirPath, "foo.rs"));
                    Assert.AreEqual(3, del.Orphans.Count);
                    Assert.False(del.IsReferenced);
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                }
            }
        }
        [TestFixture]
        public class AddRootModuleIncremental
        {
            /*
             * [main.rs] ---> bar/mod.rs <--- [baz]
             */
            [Test]
            public void ResolveToAlreadyExisting()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\ResolveNonAuthImport"))
                {
                    // main file points to bar/mod.rs. project gets loaded
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(1, reached.Count);
                    CollectionAssert.Contains(reached, Path.Combine(temp.DirPath, @"bar\mod.rs"));
                    // in the mean time bar.rs gets created
                    File.Create(Path.Combine(temp.DirPath, "bar.rs")).Close();
                    // newly added baz.rs should import existing bar\mod.rs
                    // instead of touching the disk
                    var rootAdd = tracker.AddRootModuleIncremental(Path.Combine(temp.DirPath, "baz.rs"));
                    Assert.AreEqual(0, rootAdd.Count);
                    // WARNING: Dont ModelCheck(...) incrementally created ModTracker with the fresh ones.
                    // In this one case mod name resolution will be slightly different.
                    // It is working as indended.
                }
            }
        }

        [TestFixture]
        public class DowngradeModule
        {
            /*
             * [main] ---> [lib] <-> foo <-> bar
             *              ^----------------^
             */
            [Test]
            public void UpgradeAndDowngrade()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\CircularConnected"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    tracker.AddRootModule(Path.Combine(temp.DirPath, "foo.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(2, reached.Count);
                    CollectionAssert.Contains(reached, Path.Combine(temp.DirPath, "baz.rs"));
                    CollectionAssert.Contains(reached, Path.Combine(temp.DirPath, "bar.rs"));
                    var res = tracker.DowngradeModule(Path.Combine(temp.DirPath, "foo.rs"));
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                    tracker.UpgradeModule(Path.Combine(temp.DirPath, "baz.rs"));
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"), Path.Combine(temp.DirPath, "baz.rs"));
                    res = tracker.DowngradeModule(Path.Combine(temp.DirPath, "baz.rs"));
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                    Assert.AreEqual(0, res.Orphans.Count);
                    Assert.True(res.IsReferenced);
                }
            }
        }

        [TestFixture]
        public class Reparse
        {
            [Test]
            public void MultipleBackReferenced()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\ClosedCircle"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(3, reached.Count);
                    CollectionAssert.Contains(reached, Path.Combine(temp.DirPath, "baz.rs"));
                    CollectionAssert.Contains(reached, Path.Combine(temp.DirPath, "bar.rs"));
                    CollectionAssert.Contains(reached, Path.Combine(temp.DirPath, "foo.rs"));
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    File.Create(Path.Combine(temp.DirPath, "foo.rs")).Close();
                    var diff = tracker.Reparse(Path.Combine(temp.DirPath, "foo.rs"));
                    // Return check
                    Assert.AreEqual(0, diff.Added.Count);
                    Assert.AreEqual(2, diff.Removed.Count);
                    // Model check
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                }
            }


            [Test]
            public void ClearCircularRoot()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\Circular"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    tracker.AddRootModule(Path.Combine(temp.DirPath, "foo.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(2, reached.Count);
                    CollectionAssert.Contains(reached, Path.Combine(temp.DirPath, "baz.rs"));
                    CollectionAssert.Contains(reached, Path.Combine(temp.DirPath, "bar.rs"));
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    File.Create(Path.Combine(temp.DirPath, "foo.rs")).Close();
                    var diff = tracker.Reparse(Path.Combine(temp.DirPath, "foo.rs"));
                    // Return check
                    Assert.AreEqual(0, diff.Added.Count);
                    Assert.AreEqual(2, diff.Removed.Count);
                    // Model check
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"), Path.Combine(temp.DirPath, "foo.rs"));
                }
            }

            [Test]
            public void AddCircularRoot()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\Circular"))
                {
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    File.Create(Path.Combine(temp.DirPath, "foo.rs")).Close();
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    tracker.AddRootModule(Path.Combine(temp.DirPath, "foo.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(0, reached.Count);
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    using (var stream = File.Open(Path.Combine(temp.DirPath, "foo.rs"), FileMode.CreateNew))
                    {
                        using (var textStream = new StreamWriter(stream))
                        {
                            textStream.Write("mod bar;");
                        }
                    }
                    var diff = tracker.Reparse(Path.Combine(temp.DirPath, "foo.rs"));
                    // Return check
                    Assert.AreEqual(2, diff.Added.Count);
                    Assert.AreEqual(0, diff.Removed.Count);
                    // Model check
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"), Path.Combine(temp.DirPath, "foo.rs"));
                }
            }

            [Test]
            public void RemoveLimited()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\CircularConnected"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(3, reached.Count);
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    using (var stream = File.Open(Path.Combine(temp.DirPath, "foo.rs"), FileMode.CreateNew))
                    {
                        using (var textStream = new StreamWriter(stream))
                        {
                            textStream.Write("mod bar;");
                        }
                    }
                    var diff = tracker.Reparse(Path.Combine(temp.DirPath, "foo.rs"));
                    // Return check
                    Assert.AreEqual(0, diff.Added.Count);
                    Assert.AreEqual(0, diff.Removed.Count);
                    // Model check
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                }
            }

            [Test]
            public void RemoveAndAdd()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\CircularConnected"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(3, reached.Count);
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    using (var stream = File.Open(Path.Combine(temp.DirPath, "foo.rs"), FileMode.CreateNew))
                    {
                        using (var textStream = new StreamWriter(stream))
                        {
                            textStream.Write("mod bar;");
                        }
                    }
                    var diff = tracker.Reparse(Path.Combine(temp.DirPath, "foo.rs"));
                    // Return check
                    Assert.AreEqual(0, diff.Added.Count);
                    Assert.AreEqual(0, diff.Removed.Count);
                    // Model check
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    using (var stream = File.Open(Path.Combine(temp.DirPath, "foo.rs"), FileMode.CreateNew))
                    {
                        using (var textStream = new StreamWriter(stream))
                        {
                            textStream.Write("mod bar; mod baz;");
                        }
                    }
                    diff = tracker.Reparse(Path.Combine(temp.DirPath, "foo.rs"));
                    // Return check
                    Assert.AreEqual(0, diff.Added.Count);
                    Assert.AreEqual(0, diff.Removed.Count);
                    // Model check
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                }
            }

            [Test]
            public void RemoveZombie()
            {
                using (TemporaryDirectory temp = Utils.LoadResourceDirectory(@"Internal\SimpleChain"))
                {
                    var tracker = new ModuleTracker(Path.Combine(temp.DirPath, "main.rs"));
                    var reached = tracker.ExtractReachableAndMakeIncremental();
                    Assert.AreEqual(2, reached.Count);
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    File.Delete(Path.Combine(temp.DirPath, "baz.rs"));
                    using (var stream = File.Open(Path.Combine(temp.DirPath, "foo.rs"), FileMode.CreateNew))
                    {
                        using (var textStream = new StreamWriter(stream))
                        {
                            textStream.Write("mod bar;");
                        }
                    }
                    var diff = tracker.Reparse(Path.Combine(temp.DirPath, "foo.rs"));
                    // Return check
                    Assert.AreEqual(1, diff.Added.Count);
                    Assert.AreEqual(1, diff.Removed.Count);
                    // Model check
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                    File.Delete(Path.Combine(temp.DirPath, "foo.rs"));
                    using (var stream = File.Open(Path.Combine(temp.DirPath, "foo.rs"), FileMode.CreateNew))
                    {
                        using (var textStream = new StreamWriter(stream))
                        {
                            textStream.Write("mod baz;");
                        }
                    }
                    diff = tracker.Reparse(Path.Combine(temp.DirPath, "foo.rs"));
                    // Return check
                    Assert.AreEqual(1, diff.Added.Count);
                    Assert.AreEqual(1, diff.Removed.Count);
                    // Model check
                    ModelCheck(tracker, Path.Combine(temp.DirPath, "main.rs"));
                }
            }
        }
    }
}
