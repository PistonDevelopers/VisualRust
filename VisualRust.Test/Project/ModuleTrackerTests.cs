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
        [TestFixture]
        public class ExtractReachableAndMakeIncremental
        {
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
                    Assert.AreEqual(1, res.Orphans.Count);
                    CollectionAssert.Contains(res.Orphans, Path.Combine(temp.DirPath, "baz.rs"));
                }
            }

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
                    var rem = tracker.DeleteModule(Path.Combine(temp.DirPath, "foo.rs"));
                    CollectionAssert.Contains(rem.Orphans, Path.Combine(temp.DirPath, "bar.rs"));
                    Assert.False(rem.IsReferenced);
                }
            }


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
                    var rem = tracker.UnrootModule(Path.Combine(temp.DirPath, "foo.rs"));
                    CollectionAssert.Contains(rem, Path.Combine(temp.DirPath, "foo.rs"));
                    CollectionAssert.Contains(rem, Path.Combine(temp.DirPath, "bar.rs"));
                }
            }

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
                    var del = tracker.DeleteModule(Path.Combine(temp.DirPath, "foo.rs"));
                    Assert.AreEqual(1, del.Orphans.Count);
                    Assert.True(del.IsReferenced);
                }
            }

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
                    var unr = tracker.UnrootModule(Path.Combine(temp.DirPath, "foo.rs"));
                    Assert.AreEqual(0, unr.Count);
                    var del = tracker.DeleteModule(Path.Combine(temp.DirPath, "foo.rs"));
                    Assert.AreEqual(1, del.Orphans.Count);
                    Assert.True(del.IsReferenced);
                }
            }
        }
    }
}
