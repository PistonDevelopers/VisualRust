using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;

namespace VisualRust.Project
{
    partial class ModuleParser : Parser
    {
        public static List<string> ParseImports(string text)
        {
            var lexer = new ModuleLexer(text);
            var tokens = new CommonTokenStream(lexer);
            var parser = new ModuleParser(tokens);
            BodyContext root = parser.body();
            var imports = new List<string>();
            TraverseForImports(root, imports);
            return imports;
        }

        private static void TraverseForImports(ITree node, List<string> imports)
        {
            for(int i = 0; i < node.ChildCount; i++)
            {
                var child = node.GetChild(i) as IRuleNode;
                if (child == null)
                    continue;
                if (child.RuleContext.RuleIndex == ModuleParser.RULE_mod_import)
                {
                    imports.Add(child.GetChild(1).GetText());
                }
                TraverseForImports(child, imports);
            }
        }
    }
}
