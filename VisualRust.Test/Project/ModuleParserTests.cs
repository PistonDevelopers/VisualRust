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
            public void ParseModBlock()
            {
                List<ModuleImport> importMods = ModuleParser.ParseImports(@"fn foo { }  mod asd { mod ext; }").Children;
                Assert.AreEqual(1, importMods.Count);
            }


            [Test]
            public void ParsesServoLayoutLib()
            {
                List<ModuleImport> importMods = ModuleParser.ParseImports(Utils.LoadResource(@"External\servo\components\layout\lib.rs")).Children;
                Assert.AreEqual(26, importMods.Count);
                Assert.True(importMods.Any(m => m.Ident == "layout_debug" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "construct" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "context" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "floats" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "flow" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "flow_list" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "flow_ref" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "fragment" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "layout_task" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "inline" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "model" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "parallel" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "table_wrapper" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "table" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "table_caption" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "table_colgroup" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "table_rowgroup" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "table_row" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "table_cell" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "text" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "util" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "incremental" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "wrapper" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "extra" && m.Children.Count == 0));
                Assert.True(importMods.Any(m => m.Ident == "css" && m.Children.Count == 3));
                Assert.True(importMods.First(m => m.Ident == "css").Children.Any(m => m.Ident == "node_util" && m.Children.Count == 0));
                Assert.True(importMods.First(m => m.Ident == "css").Children.Any(m => m.Ident == "matching" && m.Children.Count == 0));
                Assert.True(importMods.First(m => m.Ident == "css").Children.Any(m => m.Ident == "node_style" && m.Children.Count == 0));
            }
        }
    }
}
