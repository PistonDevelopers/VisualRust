using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VisualRust.Cargo;

namespace VisualRust.Test.Cargo
{
    class ManifestTests
    {
        [Test]
        public void SimpleManifest()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  authors = [ ""asd"", ""fgh"" ]
                  version=""0.1""
                  [dependencies]
                  foo = ""0.1""",
                out temp);
            Assert.AreEqual("foo", m.Name);
            Assert.AreEqual("0.1", m.Version);
            CollectionAssert.AreEqual(new[] { "asd", "fgh" }, m.Authors);
            Assert.AreEqual(1, m.Dependencies.Length);
            Assert.AreEqual("foo", m.Dependencies[0].Name);
            Assert.AreEqual("0.1", m.Dependencies[0].Version);
            Assert.AreEqual(0, m.OutputTargets.Count);
        }

        [Test]
        public void FailOnWrongStructure()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[[package]]
                  name=""foo""",
                out temp);
            EntryMismatchError error = temp.LoadErrors.First(e => e.Path == "package");
            Assert.AreEqual("table", error.Expected);
            Assert.AreEqual("array", error.Got);
        }

        [Test]
        public void FailOnWrongTypedDependency()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[dependencies]
                  foo   = []",
                out temp);
            EntryMismatchError error = temp.LoadErrors.First(e => e.Path == "dependencies.foo");
            Assert.AreEqual("string", error.Expected);
            Assert.AreEqual("array", error.Got);
        }

        [Test]
        public void ComplicatedDependency()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  version = ""0.1.0""
                  [dependencies]
                  hammer  = { version = ""0.5.0"", git = ""https://github.com/wycats/hammer.rs"" }",
                out temp);
            Assert.AreEqual(1, m.Dependencies.Length);
            Assert.AreEqual("hammer", m.Dependencies[0].Name);
            Assert.AreEqual("0.5.0", m.Dependencies[0].Version);
            Assert.AreEqual("https://github.com/wycats/hammer.rs", m.Dependencies[0].Git);
        }

        [Test]
        public void TargetDependency()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  version = ""0.1.0""
                  [target.x86_64-pc-windows-gnu.dependencies]
                  winhttp = ""0.4.0""",
                out temp);
            Assert.AreEqual(1, m.Dependencies.Length);
            Assert.AreEqual("winhttp", m.Dependencies[0].Name);
            Assert.AreEqual("0.4.0", m.Dependencies[0].Version);
            Assert.AreEqual("x86_64-pc-windows-gnu", m.Dependencies[0].Target);
        }

        [Test]
        public void FailOnMistypedTargetDependency()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[target.x86_64-pc-windows-gnu.dependencies]
                  winhttp = 1",
                out temp);
            EntryMismatchError error = temp.LoadErrors.First(e => e.Path == "target.x86_64-pc-windows-gnu.dependencies.winhttp");
            Assert.AreEqual("string", error.Expected);
            Assert.AreEqual("integer", error.Got);
        }

        [Test]
        public void EmptyDependencies()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  version=""0.1""",
                out temp);
            Assert.AreEqual(0, m.Dependencies.Length);
        }

        [Test]
        public void LibOutputTarget()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [lib]
                  name = ""bar""
                  path = ""src.rs""",
                out temp);
            Assert.AreEqual(1, m.OutputTargets.Count);
            Assert.AreEqual("bar", m.OutputTargets[0].Name);
            Assert.AreEqual("src.rs", m.OutputTargets[0].Path);
        }

        [Test]
        public void MultipleLibOutputTargets()
        {
            ManifestErrors temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  version=""0.1""
                  [[bench]]
                  name = ""bench1""
                  path = ""bench1.rs""
                  [lib]
                  name = ""bar""
                  path = ""src.rs""
                  [[bin]]
                  name = ""bin1""
                  path = ""bin1.rs""
                  [[bench]]
                  name = ""bench2""
                  path = ""bench2.rs""
                  [[bin]]
                  name = ""bin2""
                  path = ""bin2.rs""",
                out temp);
            OutputTarget t1 = GetOutputTarget(m.OutputTargets, OutputTargetType.Benchmark, "bench1");
            Assert.AreEqual("bench1.rs", t1.Path);
            OutputTarget t2 = GetOutputTarget(m.OutputTargets, OutputTargetType.Library, "bar");
            Assert.AreEqual("src.rs", t2.Path);
            OutputTarget t3 = GetOutputTarget(m.OutputTargets, OutputTargetType.Binary, "bin1");
            Assert.AreEqual("bin1.rs", t3.Path);
            OutputTarget t4 = GetOutputTarget(m.OutputTargets, OutputTargetType.Benchmark, "bench2");
            Assert.AreEqual("bench2.rs", t4.Path);
            OutputTarget t5 = GetOutputTarget(m.OutputTargets, OutputTargetType.Binary, "bin2");
            Assert.AreEqual("bin2.rs", t5.Path);
        }

        static OutputTarget GetOutputTarget(IReadOnlyList<OutputTarget> targets, OutputTargetType type, string name)
        {
            return targets.First(t => t.Type == type && t.Name == name);
        }
    }
}
