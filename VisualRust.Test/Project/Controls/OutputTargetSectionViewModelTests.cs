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
    }
}
