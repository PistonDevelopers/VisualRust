using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;

namespace VisualRust.Project
{
    partial class ModuleParser : Parser
    {
        public static ModuleImport ParseImports(string text)
        {
            var lexer = new ModuleLexer(text);
            var tokens = new CommonTokenStream(lexer);
            var parser = new ModuleParser(tokens);
            BodyContext root = parser.body();
            var imports = new ModuleImport(new List<ModuleImport>());
            TraverseForImports(root, imports);
            return imports;
        }

        private static void TraverseForImports(ITree node, ModuleImport current)
        {
            for(int i = 0; i < node.ChildCount; i++)
            {
                var child = node.GetChild(i) as IRuleNode;
                if (child == null)
                    continue;
                if(child.RuleContext.RuleIndex == ModuleParser.RULE_block && child.ChildCount == 5)
                {
                    var blockImport = new ModuleImport(child.GetChild(1).GetText(), new List<ModuleImport>());
                    current.Children.Add(blockImport);
                    TraverseForImports(child, blockImport);
                }
                else if (child.RuleContext.RuleIndex == ModuleParser.RULE_mod_import)
                {
                    current.Children.Add(new ModuleImport(child.GetChild(1).GetText()));
                }
                else
                {
                    TraverseForImports(child, current);
                }
            }
        }
    }
}
