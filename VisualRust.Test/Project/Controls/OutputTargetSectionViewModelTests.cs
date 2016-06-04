using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Cargo;
using VisualRust.Project.Controls;

namespace VisualRust.Test.Project.Controls
{
    class OutputTargetSectionViewModelTests
    {
        [Test]
        public void ShouldExposeDefaultTargets()
        {
            var manifest = Manifest.CreateFake("foo", null);
            var vm = new OutputTargetSectionViewModel(manifest, null);
            Assert.AreEqual(6, vm.Targets.Count);
        }

        [Test]
        public void EmptyLib()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [lib]",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, null);
            OutputTargetViewModel lib = (OutputTargetViewModel)GetOutputTarget(vm.Targets, OutputTargetType.Library);
            Assert.AreEqual("foo", lib.Name);
            Assert.AreEqual(false, lib.IsPathOverriden);
            Assert.AreEqual(@"src\foo.rs", lib.DefaultPath);
        }

        [Test]
        public void NameOnlyLib()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [lib]
                  name=""bar""",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, null);
            OutputTargetViewModel lib = (OutputTargetViewModel)GetOutputTarget(vm.Targets, OutputTargetType.Library);
            Assert.AreEqual("bar", lib.Name);
            Assert.AreEqual(false, lib.IsPathOverriden);
            Assert.AreEqual(@"src\bar.rs", lib.DefaultPath);
        }

        [Test]
        public void PathOnlyLib()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [lib]
                  path=""src\\bar.rs""",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, null);
            OutputTargetViewModel lib = (OutputTargetViewModel)GetOutputTarget(vm.Targets, OutputTargetType.Library);
            Assert.AreEqual("foo", lib.Name);
            Assert.AreEqual(true, lib.IsPathOverriden);
            Assert.AreEqual(@"src\bar.rs", lib.Path);
        }

        static IOutputTargetViewModel GetOutputTarget(IEnumerable<IOutputTargetViewModel> targets, OutputTargetType type)
        {
            return targets.First(t => t.Type == type);
        }

        [Test]
        public void DuplicateTarget()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [[bin]]
                  name=""foo""",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, () => OutputTargetType.Binary);
            vm.Add();
            ((OutputTargetViewModel)vm.Targets[vm.Targets.Count - 1]).Name = "foo";
            ((OutputTargetViewModel)vm.Targets[vm.Targets.Count - 1]).Path = @"src\bar.rs";
            Assert.True(vm.IsDirty);
            var changes = vm.PendingChanges();
            Assert.AreEqual(1, changes.TargetsAdded.Count);
            Assert.AreEqual("foo", changes.TargetsAdded[0].Name);
            Assert.AreEqual(@"src\bar.rs", changes.TargetsAdded[0].Path);
        }

        [Test]
        public void RemoveFirstDuplicate()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [[bin]]
                  name=""foo""
                  path=""src\foo1.rs""
                  [[bin]]
                  name=""src\foo2.rs""",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, null);
            var first = (OutputTargetViewModel)vm.Targets.First(t => t.Type == OutputTargetType.Binary);
            var second = (OutputTargetViewModel)Second(vm.Targets, t => t.Type == OutputTargetType.Binary);
            Assert.AreNotEqual(first.Handle, second.Handle);
            vm.Remove(first);
            Assert.True(vm.IsDirty);
            var changes = vm.PendingChanges();
            Assert.AreEqual(1, changes.TargetsRemoved.Count);
            Assert.AreEqual(first.Handle, changes.TargetsRemoved[0].Handle);
        }

        [Test]
        public void RemoveSecondDuplicate()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [[bin]]
                  name=""foo""
                  path=""src\foo1.rs""
                  [[bin]]
                  name=""src\foo2.rs""",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, null);
            var first = (OutputTargetViewModel)vm.Targets.First(t => t.Type == OutputTargetType.Binary);
            var second = (OutputTargetViewModel)Second(vm.Targets, t => t.Type == OutputTargetType.Binary);
            Assert.AreNotEqual(first.Handle, second.Handle);
            vm.Remove(second);
            Assert.True(vm.IsDirty);
            var changes = vm.PendingChanges();
            Assert.AreEqual(1, changes.TargetsRemoved.Count);
            Assert.AreEqual(second.Handle, changes.TargetsRemoved[0].Handle);
        }

        static T Second<T>(IEnumerable<T> coll, Func<T, bool> picker)
        {
            return coll.Where(x => picker(x)).Skip(1).First();
        }

        [Test]
        public void ExposeChanges()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [[bin]]
                  name=""foo""
                  path=""src\foo1.rs""
                  [[bin]]
                  name=""src\foo2.rs""",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, null);
            var first = (OutputTargetViewModel)vm.Targets.First(t => t.Type == OutputTargetType.Binary);
            var second = (OutputTargetViewModel)Second(vm.Targets, t => t.Type == OutputTargetType.Binary);
            Assert.AreNotEqual(first.Handle, second.Handle);
            first.Harness = true;
            Assert.True(vm.IsDirty);
            var changes = vm.PendingChanges();
            Assert.AreEqual(1, changes.TargetsChanged.Count);
            var changed = changes.TargetsChanged[0];
            Assert.AreEqual(first.Handle, changed.Value.Handle);
            Assert.AreEqual(true, changed.Value.Harness);
            Assert.Null(changed.Value.Name);
            Assert.Null(changed.Value.Path);
            Assert.Null(changed.Value.Test);
            Assert.Null(changed.Value.Doctest);
            Assert.Null(changed.Value.Bench);
            Assert.Null(changed.Value.Doc);
            Assert.Null(changed.Value.Plugin);
        }

        [Test]
        public void RemoveTarget()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                "[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n"
               +"[[bin]]\n"
               +"name=\"foo\"\n"
               +"path=\"src\\foo1.rs\"\n"
               +"[[bin]]\n"
               +"name=\"src\\foo2.rs\"\n",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, null);
            var first = (OutputTargetViewModel)vm.Targets.First(t => t.Type == OutputTargetType.Binary);
            vm.Remove(first);
            vm.Apply();
            Assert.AreEqual(
                "[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n"
               +"[[bin]]\n"
               +"name=\"src\\foo2.rs\"\n",
                m.ToString());
            Assert.False(vm.IsDirty);
        }

        [Test]
        public void AddTarget()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                "[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n"
               +"[[bin]]\n"
               +"name=\"asdf\"",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, () => OutputTargetType.Test);
            vm.Add();
            ((OutputTargetViewModel)vm.Targets[vm.Targets.Count - 1]).Name = "bar";
            ((OutputTargetViewModel)vm.Targets[vm.Targets.Count - 1]).Path = "src\\baz.rs";
            ((OutputTargetViewModel)vm.Targets[vm.Targets.Count - 1]).Plugin = false;
            vm.Apply();
            Assert.AreEqual(
                "[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n"
               +"[[bin]]\n"
               +"name=\"asdf\"\n"
               +"[[test]]\n"
               +"name = \"bar\"\n"
               +"path = \"src\\\\baz.rs\"\n"
               +"plugin = false\n",
                m.ToString());
            Assert.False(vm.IsDirty);
        }

        [Test]
        public void SetTarget()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                "[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n"
               +"[[bin]]\n"
               +"name=\"asdf\"\n"
               +"[[test]]\n"
               +"name = \"bar\"\n"
               +"path = \"src\\\\baz.rs\"\n"
               +"plugin = false\n",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, null);
            var test = (OutputTargetViewModel)vm.Targets.First(t => t.Type == OutputTargetType.Test);
            test.Plugin = true;
            test.Doctest = true;
            Assert.True(vm.IsDirty);
            vm.Apply();
            Assert.AreEqual(
                "[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n"
               +"[[bin]]\n"
               +"name=\"asdf\"\n"
               +"[[test]]\n"
               +"name = \"bar\"\n"
               +"path = \"src\\\\baz.rs\"\n"
               +"doctest = true\n"
               +"plugin = true\n",
                m.ToString());
        }

        [Test]
        public void SetInlineTarget()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                "bin = [ { name = \"asdf\" } ]\n"
               +"[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, null);
            var binary = (OutputTargetViewModel)vm.Targets.First(t => t.Type == OutputTargetType.Binary);
            binary.Name = "qwer";
            binary.Plugin = true;
            binary.Doctest = true;
            Assert.True(vm.IsDirty);
            vm.Apply();
            Assert.AreEqual(
                "bin = [ { name = \"qwer\", doctest = true, plugin = true } ]\n"
               +"[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n",
                m.ToString());
        }

        [Test]
        public void SetInlineTargetLib()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                "lib = { name = \"asdf\" }\n"
               +"[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, null);
            var library = (OutputTargetViewModel)vm.Targets.First(t => t.Type == OutputTargetType.Library);
            library.Name = "qwer";
            library.Plugin = true;
            library.Doctest = true;
            Assert.True(vm.IsDirty);
            vm.Apply();
            Assert.AreEqual(
                "lib = { name = \"qwer\", doctest = true, plugin = true }\n"
               +"[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"",
                m.ToString());
        }

        [Test]
        public void SetTargetLibExotic()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                "[lib.asdf]\n"
               +"name = \"foo\"\n"
               +"[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"",
                out temp);
            var vm = new OutputTargetSectionViewModel(m, null);
            var library = (OutputTargetViewModel)vm.Targets.First(t => t.Type == OutputTargetType.Library);
            Assert.AreEqual(UIntPtr.Zero, library.Handle);
            library.Name = "qwer";
            library.Plugin = true;
            library.Doctest = true;
            Assert.True(vm.IsDirty);
            vm.Apply();
            Assert.AreEqual(
                "[lib.asdf]\n"
               +"name = \"foo\"\n"
               +"[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n"
               +"[lib]\n"
               +"name = \"qwer\"\n"
               +"doctest = true\n"
               +"plugin = true\n",
                m.ToString());
            var libraryAfter = (OutputTargetViewModel)vm.Targets.First(t => t.Type == OutputTargetType.Library);
            Assert.AreNotEqual(UIntPtr.Zero, libraryAfter.Handle);
        }
    }
}
