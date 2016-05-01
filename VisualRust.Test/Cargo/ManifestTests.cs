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
            CollectionAssert.AreEqual(new [] { "asd", "fgh" }, m.Authors);
            Assert.AreEqual(1, m.Dependencies.Length);
            Assert.AreEqual("foo", m.Dependencies[0].Name);
            Assert.AreEqual("0.1", m.Dependencies[0].Version);
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
                @"[dependencies]
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
                @"[target.x86_64-pc-windows-gnu.dependencies]
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
    }
}
