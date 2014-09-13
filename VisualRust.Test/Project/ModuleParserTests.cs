using NUnit.Framework;
using System.Collections.Generic;
using VisualRust.Project;
using System.Linq;
using System;

namespace VisualRust.Test.Project
{
    public class ModuleParserTests
    {
        [TestFixture]
        public class ParseImports
        {
            [Test]
            public void MergeModBlocks()
            {
                ModuleImport importMods = ModuleParser.ParseImports(@"fn foo { }  mod asd { mod ext; } mod asd { mod bar { mod ext1; } } mod asd { mod bar { mod ext2; } }");
                Assert.AreEqual(1, importMods.Count);
                Assert.AreEqual(2, importMods["asd"].Count);
                Assert.AreEqual(0, importMods["asd"]["ext"].Count);
                Assert.AreEqual(2, importMods["asd"]["bar"].Count);
                Assert.AreEqual(0, importMods["asd"]["bar"]["ext1"].Count);
                Assert.AreEqual(0, importMods["asd"]["bar"]["ext2"].Count);
            }

            [Test]
            public void ParseModBlock()
            {
                ModuleImport importMods = ModuleParser.ParseImports(@"fn foo { }  mod asd { mod ext; }");
                Assert.AreEqual(1, importMods.Count);
                Assert.AreEqual(1, importMods["asd"].Count);
                Assert.AreEqual(0, importMods["asd"]["ext"].Count);
            }

            [Test]
            public void ParseCommentBlock()
            {
                ModuleImport importMods = ModuleParser.ParseImports(@"/* mod ext; */");
                Assert.AreEqual(0, importMods.Count);
            }

            [Test]
            public void ParseInnerModBlock()
            {
                ModuleImport importMods = ModuleParser.ParseImports(@"mod foo { mod inner; }");
                Assert.AreEqual(1, importMods.Count);
                Assert.AreEqual(1, importMods["foo"].Count);
                Assert.AreEqual(0, importMods["foo"]["inner"].Count);
            }

            [Test]
            public void ParseLargeInnerModBlock()
            {
                ModuleImport importMods = ModuleParser.ParseImports(@"mod asd { mod bar { } mod baz { mod inner; } mod ext1; mod ext2; mod ext3; }");
                Assert.AreEqual(1, importMods.Count);
                Assert.AreEqual(4, importMods["asd"].Count);
                Assert.AreEqual(1, importMods["asd"]["baz"].Count);
            }

            [Test]
            public void EmptyModBlock()
            {
                ModuleImport importMods = ModuleParser.ParseImports(@"mod asd { foo(); }");
                Assert.AreEqual(0, importMods.Count);
            }

            [Test]
            public void MergeModules()
            {
                ModuleImport importMods = ModuleParser.ParseImports(@"mod asd { mod foo; } mod asd { mod bar; }");
                Assert.AreEqual(1, importMods.Count);
                Assert.AreEqual(2, importMods["asd"].Count);
            }

            [Test]
            public void ParseCommentNewLine()
            {
                ModuleImport importMods = ModuleParser.ParseImports(@"// mod ext;");
                Assert.AreEqual(0, importMods.Count);
            }

            [Test]
            public void ParsesServoLayoutLib()
            {
                ModuleImport importMods = ModuleParser.ParseImports(Utils.LoadResource(@"External\servo\components\layout\lib.rs"));
                Assert.AreEqual(26, importMods.Count);
                Assert.True(importMods["layout_debug"].Count == 0);
                Assert.True(importMods["construct"].Count == 0);
                Assert.True(importMods["context"].Count == 0);
                Assert.True(importMods["floats"].Count == 0);
                Assert.True(importMods["flow"].Count == 0);
                Assert.True(importMods["flow_list"].Count == 0);
                Assert.True(importMods["flow_ref"].Count == 0);
                Assert.True(importMods["fragment"].Count == 0);
                Assert.True(importMods["layout_task"].Count == 0);
                Assert.True(importMods["inline"].Count == 0);
                Assert.True(importMods["model"].Count == 0);
                Assert.True(importMods["parallel"].Count == 0);
                Assert.True(importMods["table_wrapper"].Count == 0);
                Assert.True(importMods["table"].Count == 0);
                Assert.True(importMods["table_caption"].Count == 0);
                Assert.True(importMods["table_colgroup"].Count == 0);
                Assert.True(importMods["table_rowgroup"].Count == 0);
                Assert.True(importMods["table_row"].Count == 0);
                Assert.True(importMods["table_cell"].Count == 0);
                Assert.True(importMods["text"].Count == 0);
                Assert.True(importMods["util"].Count == 0);
                Assert.True(importMods["incremental"].Count == 0);
                Assert.True(importMods["wrapper"].Count == 0);
                Assert.True(importMods["extra"].Count == 0);
                Assert.True(importMods["css"].Count == 3);
                Assert.True(importMods["css"]["node_util"].Count == 0);
                Assert.True(importMods["css"]["matching"].Count == 0);
                Assert.True(importMods["css"]["node_style"].Count == 0);
            }
        }
    }
}
