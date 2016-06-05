using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Cargo;
using VisualRust.Project;
using VisualRust.Project.Controls;

namespace VisualRust.Test.Project.Controls
{
    class OutputTargetSectionViewModelTests
    {
        [Test]
        public void EmptyLib()
        {
            var m = CreateManifest(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [lib]");
            var vm = new OutputTargetSectionViewModel(m, null);
            OutputTargetViewModel lib = (OutputTargetViewModel)GetOutputTarget(vm.Targets, OutputTargetType.Library);
            Assert.AreEqual("foo", lib.Name);
            Assert.AreEqual(false, lib.IsPathOverriden);
            Assert.AreEqual(@"src\foo.rs", lib.DefaultPath);
        }

        [Test]
        public void NameOnlyLib()
        {
            var m = CreateManifest(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [lib]
                  name=""bar""");
            var vm = new OutputTargetSectionViewModel(m, null);
            OutputTargetViewModel lib = (OutputTargetViewModel)GetOutputTarget(vm.Targets, OutputTargetType.Library);
            Assert.AreEqual("bar", lib.Name);
            Assert.AreEqual(false, lib.IsPathOverriden);
            Assert.AreEqual(@"src\bar.rs", lib.DefaultPath);
        }

        [Test]
        public void PathOnlyLib()
        {
            var m = CreateManifest(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [lib]
                  path=""src\\bar.rs""");
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
            var m = CreateManifest(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [[bin]]
                  name=""foo""");
            var vm = new OutputTargetSectionViewModel(m, _ => OutputTargetType.Binary);
            vm.Add();
            ((OutputTargetViewModel)vm.Targets[vm.Targets.Count - 1]).Name = "foo";
            ((OutputTargetViewModel)vm.Targets[vm.Targets.Count - 1]).Path = @"src\bar.rs";
            Assert.True(vm.IsDirty);
            var changes = vm.PendingChanges();
            Assert.AreEqual(1, changes.TargetsAdded.Count);
            Assert.AreEqual("foo", changes.TargetsAdded[0].Value.Name);
            Assert.AreEqual(@"src\bar.rs", changes.TargetsAdded[0].Value.Path);
        }

        [Test]
        public void RemoveFirstDuplicate()
        {
            var m = CreateManifest(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [[bin]]
                  name=""foo""
                  path=""src\foo1.rs""
                  [[bin]]
                  name=""src\foo2.rs""");
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
            var m = CreateManifest(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [[bin]]
                  name=""foo""
                  path=""src\foo1.rs""
                  [[bin]]
                  name=""src\foo2.rs""");
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
            var m = CreateManifest(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [[bin]]
                  name=""foo""
                  path=""src\foo1.rs""
                  [[bin]]
                  name=""src\foo2.rs""");
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
            var m = CreateManifest(
                "[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n"
               +"[[bin]]\n"
               +"name=\"foo\"\n"
               +"path=\"src\\foo1.rs\"\n"
               +"[[bin]]\n"
               +"name=\"src\\foo2.rs\"\n");
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
                m.Manifest.ToString());
            Assert.False(vm.IsDirty);
            Assert.True(m.Manifest.OutputTargets.All(t => t.Handle != first.Handle));
        }

        [Test]
        public void AddTarget()
        {
            var m = CreateManifest(
                "[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n"
               +"[[bin]]\n"
               +"name=\"asdf\"");
            var vm = new OutputTargetSectionViewModel(m, _ => OutputTargetType.Test);
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
                m.Manifest.ToString());
            Assert.False(vm.IsDirty);
            var test = vm.Targets.OfType<OutputTargetViewModel>().First(t => t.Type == OutputTargetType.Test);
            Assert.AreNotEqual(UIntPtr.Zero, test.Handle);
            Assert.NotNull(test.Handle);
            Assert.True(m.Manifest.OutputTargets.Any(t => t.Handle == test.Handle));
        }

        [Test]
        public void SetTarget()
        {
            var m = CreateManifest(
                "[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n"
               +"[[bin]]\n"
               +"name=\"asdf\"\n"
               +"[[test]]\n"
               +"name = \"bar\"\n"
               +"path = \"src\\\\baz.rs\"\n"
               +"plugin = false\n");
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
                m.Manifest.ToString());
            var rawTest = m.Manifest.OutputTargets.First(t => t.Type == OutputTargetType.Test);
            Assert.AreEqual(true, rawTest.Plugin);
            Assert.AreEqual(true, rawTest.Doctest);
        }

        [Test]
        public void SetInlineTarget()
        {
            var m = CreateManifest(
                "bin = [ { name = \"asdf\" } ]\n"
               +"[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n");
            var vm = new OutputTargetSectionViewModel(m,null);
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
                m.Manifest.ToString());
        }

        [Test]
        public void SetInlineTargetLib()
        {
            var m = CreateManifest(
                "lib = { name = \"asdf\" }\n"
               +"[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"");
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
                m.Manifest.ToString());
        }

        [Test]
        public void SetTargetLibImplicit()
        {
            var m = CreateManifest(
                "[lib.asdf]\n"
               +"name = \"foo\"\n"
               +"[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"");
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
                m.Manifest.ToString());
            var libraryAfter = (OutputTargetViewModel)vm.Targets.First(t => t.Type == OutputTargetType.Library);
            Assert.AreNotEqual(UIntPtr.Zero, libraryAfter.Handle);
            Assert.NotNull(libraryAfter.Handle);
        }

        [Test]
        public void RemoveTargetLibImplicit()
        {
            var m = CreateManifest(
                "[lib.asdf]\n"
               +"name = \"foo\"\n"
               +"[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n");
            var vm = new OutputTargetSectionViewModel(m, null);
            var library = (OutputTargetViewModel)vm.Targets.First(t => t.Type == OutputTargetType.Library);
            vm.Remove(library);
            Assert.True(vm.IsDirty);
            vm.Apply();
            Assert.AreEqual(
                "[package]\n"
               +"name=\"foo\"\n"
               +"version=\"0.1\"\n",
                m.Manifest.ToString());
            Assert.AreEqual(0, m.Manifest.OutputTargets.Count);
        }

        static ManifestFile CreateManifest(string text)
        {
            ManifestErrors temp;
            return ManifestFile.Create(new MockFileSystem(), "Cargo.toml", _ => ManifestLoadResult.CreateSuccess("Cargo.toml", Manifest.TryCreate(text, out temp)));
        }
    }
}
