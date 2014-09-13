using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using System.Linq;

namespace VisualRust.Project
{
    partial class ModuleParser : Parser
    {
        /*
         * There's one fairly important difference between mod resolution in rustc and what we do.
         * Given following code:
         * mod bar { mod a; } mod bar { mod b; }
         * We will merget this to mod bar { mod a; mod b; }, but rustc will error out.
         */
        public static ModuleImport ParseImports(string text)
        {
            var lexer = new ModuleLexer(text);
            var tokens = new CommonTokenStream(lexer);
            var parser = new ModuleParser(tokens);
            BodyContext root = parser.body();
            var imports = new ModuleImport();
            TraverseForImports(root, imports);
            return imports;
        }

        /*
         * Fairly straightforward traversal, with one gotcha: we return bool indicating
         * if the node is contained within exported mod block, it's to deal with the following:
         * mod bar { ... }
         * We don't know at this point if any of the items inside blocks is a module,
         * so decision to add 'bar' ModuleImport has to be delayed.
         */
        private static bool TraverseForImports(ITree node, ModuleImport current)
        {
            bool isContainedInBlock = false;
            for(int i = 0; i < node.ChildCount; i++)
            {
                var child = node.GetChild(i) as IRuleNode;
                if (child == null)
                    continue;
                if(child.RuleContext.RuleIndex == ModuleParser.RULE_block && child.ChildCount == 5)
                {
                    var blockChildren = new ModuleImport();
                    var blockImport = new ModuleImport()
                    {
                        {child.GetChild(1).GetText(), blockChildren }
                    };
                    if (TraverseForImports(child, blockChildren))
                    {
                        current.Merge(blockImport);
                        isContainedInBlock = true;
                    }
                }
                else if (child.RuleContext.RuleIndex == ModuleParser.RULE_mod_import)
                {
                    isContainedInBlock = true;
                    string ident = child.GetChild(1).GetText();
                    current.Merge(new Dictionary<string, ModuleImport>()
                    {
                        { ident, new ModuleImport() }
                    });
                }
                else
                {
                    if (TraverseForImports(child, current))
                        isContainedInBlock = true;
                }
            }
            return isContainedInBlock;
        }
    }
}
