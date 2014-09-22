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
            private class ModulePathComparer : IEqualityComparer<PathSegment[]>
            {
                public bool Equals(PathSegment[] x, PathSegment[] y)
                {
                    if(x.Length != y.Length)
                        return false;
                    for(int i =0; i < x.Length; i++)
                    {
                        if (x[i].Name != y[i].Name || x[i].IsAuthorative != y[i].IsAuthorative)
                            return false;
                    }
                    return true;
                }

                public int GetHashCode(PathSegment[] obj)
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
                    { new PathSegment("foo"), new ModuleImport() },
                    { new PathSegment("bar"), new ModuleImport() }
                };
                var terminals = empty.GetTerminalImports().ToArray();
                Assert.AreEqual(2, terminals.Length);
                Assert.True(terminals.Contains(new PathSegment[] { new PathSegment("foo") }, pathComparer));
                Assert.True(terminals.Contains(new PathSegment[] { new PathSegment("bar") }, pathComparer));
            }

            [Test]
            public void Varied()
            {
                var empty = new ModuleImport()
                {
                    { new PathSegment("foo"), new ModuleImport()
                        { 
                            { new PathSegment("baz"), new ModuleImport() },
                            { new PathSegment("m1"), new ModuleImport()
                                {
                                    { new PathSegment("m21"), new ModuleImport() },
                                    { new PathSegment("m22"), new ModuleImport() }
                                }
                            }
                        }
                    },
                    { new PathSegment("bar"), new ModuleImport() }
                };
                var terminals = empty.GetTerminalImports().ToArray();
                Assert.AreEqual(4, terminals.Length);
                Assert.True(terminals.Contains(new PathSegment[] { new PathSegment("foo"), new PathSegment("baz") }, pathComparer));
                Assert.True(terminals.Contains(new PathSegment[] { new PathSegment("foo"), new PathSegment("m1"), new PathSegment("m21") }, pathComparer));
                Assert.True(terminals.Contains(new PathSegment[] { new PathSegment("foo"), new PathSegment("m1"), new PathSegment("m22") }, pathComparer));
                Assert.True(terminals.Contains(new PathSegment[] { new PathSegment("bar") }, pathComparer));
            }
        }
    }
}
