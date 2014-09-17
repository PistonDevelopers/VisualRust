using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Project;

namespace VisualRust.Test.Project
{
    class ModuleImportTests
    {
        [TestFixture]
        public class GetTerminalImports
        {
            private class ModulePathComparer : IEqualityComparer<string[]>
            {
                public bool Equals(string[] x, string[] y)
                {
                    if(x.Length != y.Length)
                        return false;
                    for(int i =0; i < x.Length; i++)
                    {
                        if (x[i] != y[i])
                            return false;
                    }
                    return true;
                }

                public int GetHashCode(string[] obj)
                {
                    return obj.Aggregate(0, (acc, str) => acc ^ str.GetHashCode());
                }
            }

            private static ModulePathComparer pathComparer = new ModulePathComparer();

            [Test]
            public void Empty()
            {
                var empty = new ModuleImport();
                CollectionAssert.AreEquivalent(new string[0, 0], empty.GetTerminalImports());
            }

            [Test]
            public void SingleLevel()
            {
                var empty = new ModuleImport()
                {
                    { "foo", new ModuleImport() },
                    { "bar", new ModuleImport() }
                };
                var terminals = empty.GetTerminalImports().ToArray();
                Assert.AreEqual(2, terminals.Length);
                Assert.True(terminals.Contains(new string[] { "foo" }, pathComparer));
                Assert.True(terminals.Contains(new string[] { "bar" }, pathComparer));
            }

            [Test]
            public void Varied()
            {
                var empty = new ModuleImport()
                {
                    { "foo", new ModuleImport()
                        { 
                            { "baz", new ModuleImport() },
                            { "m1", new ModuleImport()
                                {
                                    { "m21", new ModuleImport() },
                                    { "m22", new ModuleImport() }
                                }
                            }
                        }
                    },
                    { "bar", new ModuleImport() }
                };
                var terminals = empty.GetTerminalImports().ToArray();
                Assert.AreEqual(4, terminals.Length);
                Assert.True(terminals.Contains(new string[] { "foo", "baz" }, pathComparer));
                Assert.True(terminals.Contains(new string[] { "foo", "m1", "m21" }, pathComparer));
                Assert.True(terminals.Contains(new string[] { "foo", "m1", "m22" }, pathComparer));
                Assert.True(terminals.Contains(new string[] { "bar" }, pathComparer));
            }
        }
    }
}
