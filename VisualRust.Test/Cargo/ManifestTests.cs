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
        public void ParsesSimple()
        {
            var m = new Manifest(
                @"[package]
                  name=""foo""
                  version=""0.1""");
            Assert.AreEqual("foo", m.Name);
            Assert.AreEqual("0.1", m.Version);
        }
    }
}
