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
        public void ParseSimple()
        {
            LoadError temp;
            var m = Manifest.TryCreate(
                @"[package]
                  name=""foo""
                  authors = [ ""asd"", ""fgh"" ]
                  version=""0.1""",
                out temp);
            Assert.AreEqual("foo", m.Name);
            Assert.AreEqual("0.1", m.Version);
            CollectionAssert.AreEqual(new [] { "asd", "fgh" }, m.Authors);
        }

        [Test]
        public void FailOnWrongStructure()
        {
            LoadError temp;
            var m = Manifest.TryCreate(
                @"[[package]]
                  name=""foo""",
                out temp);
            EntryMismatchError error = temp.LoadErrors.First(e => e.Path == "package");
            Assert.AreEqual("table", error.Expected);
            Assert.AreEqual("array", error.Got);
        }
    }
}
