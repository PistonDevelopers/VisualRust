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
            var vm = new OutputTargetSectionViewModel(manifest);
            Assert.NotNull(vm.Library);
            Assert.AreEqual(1, vm.Binaries.Count);
            Assert.AreEqual(1, vm.Benchmarks.Count);
            Assert.AreEqual(1, vm.Tests.Count);
            Assert.AreEqual(1, vm.Examples.Count);
        }
    }
}
