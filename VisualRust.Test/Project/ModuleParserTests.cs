using NUnit.Framework;
using System.Collections.Generic;
using VisualRust.Project;
using System.Linq;
using System;
using Antlr4.Runtime;

namespace VisualRust.Test.Project
{
    public class ModuleParserTests
    {
        [TestFixture]
        public class ParseImports
        {
            [Test]
            public void AttributePaths()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(Utils.LoadResource(@"Internal\mod_paths.rs")));
                Assert.AreEqual(1, importMods.Count);
                Assert.AreEqual(1, importMods[new PathSegment("task_files", true)].Count);
                Assert.False(importMods.ContainsKey(new PathSegment("task_files", false)));
                Assert.AreEqual(0, importMods[new PathSegment("task_files", true)][new PathSegment("tls.rs", true)].Count);
                Assert.False(importMods[new PathSegment("task_files", true)].ContainsKey(new PathSegment("tls.rs", false)));
            }

            [Test]
            public void ParsePubModifier()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(
                    "pub mod foo { pub mod bar; } #[path=\"foo\"] pub mod ex { mod baz; }"));
                Assert.AreEqual(1, importMods.Count);
            }


            [Test]
            public void MergeAuthorative()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(
                    "mod foo { mod bar; } #[path=\"foo\"] mod foo { mod baz; }"));
                Assert.AreEqual(1, importMods.Count);
            }

            [Test]
            public void MergeModBlocks()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(
                    @"fn foo { }  mod asd { mod ext; } mod asd { mod bar { mod ext1; } } mod asd { mod bar { mod ext2; } }"));
                Assert.AreEqual(1, importMods.Count);
                Assert.AreEqual(2, importMods[new PathSegment("asd")].Count);
                Assert.AreEqual(0, importMods[new PathSegment("asd")][new PathSegment("ext")].Count);
                Assert.AreEqual(2, importMods[new PathSegment("asd")][new PathSegment("bar")].Count);
                Assert.AreEqual(0, importMods[new PathSegment("asd")][new PathSegment("bar")][new PathSegment("ext1")].Count);
                Assert.AreEqual(0, importMods[new PathSegment("asd")][new PathSegment("bar")][new PathSegment("ext2")].Count);
            }

            [Test]
            public void ParseModBlock()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(
                    @"fn foo { }  mod asd { mod ext; }"));
                Assert.AreEqual(1, importMods.Count);
                Assert.AreEqual(1, importMods[new PathSegment("asd")].Count);
                Assert.AreEqual(0, importMods[new PathSegment("asd")][new PathSegment("ext")].Count);
            }

            [Test]
            public void ParseCommentBlock()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(
                    @"/* mod ext; */"));
                Assert.AreEqual(0, importMods.Count);
            }

            [Test]
            public void ParseInnerModBlock()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(
                    @"mod foo { mod inner; }"));
                Assert.AreEqual(1, importMods.Count);
                Assert.AreEqual(1, importMods[new PathSegment("foo")].Count);
                Assert.AreEqual(0, importMods[new PathSegment("foo")][new PathSegment("inner")].Count);
            }

            [Test]
            public void ParseLargeInnerModBlock()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(
                    @"mod asd { mod bar { } mod baz { mod inner; } mod ext1; mod ext2; mod ext3; }"));
                Assert.AreEqual(1, importMods.Count);
                Assert.AreEqual(4, importMods[new PathSegment("asd")].Count);
                Assert.AreEqual(1, importMods[new PathSegment("asd")][new PathSegment("baz")].Count);
            }

            [Test]
            public void EmptyModBlock()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(
                    @"mod asd { foo(); }"));
                Assert.AreEqual(0, importMods.Count);
            }

            [Test]
            public void MergeModules()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(
                    @"mod asd { mod foo; } mod asd { mod bar; }"));
                Assert.AreEqual(1, importMods.Count);
                Assert.AreEqual(2, importMods[new PathSegment("asd")].Count);
            }

            [Test]
            public void ParseCommentNewLine()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(
                    @"// mod ext;"));
                Assert.AreEqual(0, importMods.Count);
            }

            [Test]
            public void ParsesServoLayoutLib()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(Utils.LoadResource(@"External\servo\components\layout\lib.rs")));
                Assert.AreEqual(26, importMods.Count);
                Assert.True(importMods[new PathSegment("layout_debug")].Count == 0);
                Assert.True(importMods[new PathSegment("construct")].Count == 0);
                Assert.True(importMods[new PathSegment("context")].Count == 0);
                Assert.True(importMods[new PathSegment("floats")].Count == 0);
                Assert.True(importMods[new PathSegment("flow")].Count == 0);
                Assert.True(importMods[new PathSegment("flow_list")].Count == 0);
                Assert.True(importMods[new PathSegment("flow_ref")].Count == 0);
                Assert.True(importMods[new PathSegment("fragment")].Count == 0);
                Assert.True(importMods[new PathSegment("layout_task")].Count == 0);
                Assert.True(importMods[new PathSegment("inline")].Count == 0);
                Assert.True(importMods[new PathSegment("model")].Count == 0);
                Assert.True(importMods[new PathSegment("parallel")].Count == 0);
                Assert.True(importMods[new PathSegment("table_wrapper")].Count == 0);
                Assert.True(importMods[new PathSegment("table")].Count == 0);
                Assert.True(importMods[new PathSegment("table_caption")].Count == 0);
                Assert.True(importMods[new PathSegment("table_colgroup")].Count == 0);
                Assert.True(importMods[new PathSegment("table_rowgroup")].Count == 0);
                Assert.True(importMods[new PathSegment("table_row")].Count == 0);
                Assert.True(importMods[new PathSegment("table_cell")].Count == 0);
                Assert.True(importMods[new PathSegment("text")].Count == 0);
                Assert.True(importMods[new PathSegment("util")].Count == 0);
                Assert.True(importMods[new PathSegment("incremental")].Count == 0);
                Assert.True(importMods[new PathSegment("wrapper")].Count == 0);
                Assert.True(importMods[new PathSegment("extra")].Count == 0);
                Assert.True(importMods[new PathSegment("css")].Count == 3);
                Assert.True(importMods[new PathSegment("css")][new PathSegment("node_util")].Count == 0);
                Assert.True(importMods[new PathSegment("css")][new PathSegment("matching")].Count == 0);
                Assert.True(importMods[new PathSegment("css")][new PathSegment("node_style")].Count == 0);
            }

            [Test]
            public void ParsesPistonImageLib()
            {
                ModuleImport importMods = ModuleParser.ParseImports(new AntlrInputStream(Utils.LoadResource(@"External\image\src\lib.rs")));
                Assert.AreEqual(9, importMods.Count);
                Assert.True(importMods[new PathSegment("imageops")].Count == 0);
                Assert.True(importMods[new PathSegment("webp")].Count == 0);
                Assert.True(importMods[new PathSegment("ppm")].Count == 0);
                Assert.True(importMods[new PathSegment("png")].Count == 0);
                Assert.True(importMods[new PathSegment("jpeg")].Count == 0);
                Assert.True(importMods[new PathSegment("gif")].Count == 0);
                Assert.True(importMods[new PathSegment("image")].Count == 0);
                Assert.True(importMods[new PathSegment("dynimage")].Count == 0);
                Assert.True(importMods[new PathSegment("color")].Count == 0);
            }
        }
    }
}
