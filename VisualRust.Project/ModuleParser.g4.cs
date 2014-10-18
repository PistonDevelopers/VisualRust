using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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
        public static ModuleImport ParseImports(ICharStream stream)
        {
            var lexer = new ModuleLexer(stream);
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
                if(child.RuleContext.RuleIndex == ModuleParser.RULE_mod_block)
                {
                    var blockChildren = new ModuleImport();
                    var blockImport = new ModuleImport()
                    {
                        { GetModIdent(child), blockChildren }
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
                    current.Merge(new Dictionary<PathSegment, ModuleImport>()
                    {
                        { GetModIdent(child), new ModuleImport() }
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

        private static PathSegment GetModIdent(IRuleNode modNode)
        {
            var firstNode = modNode.GetChild(0) as IRuleNode;
            int currentChild = 0;
            if (firstNode != null)
            {
                currentChild = 1;
                ITree attrRoot = modNode.GetChild(0);
                for(int i = 0; i < attrRoot.ChildCount; i++)
                {
                    IRuleNode attrNode = (IRuleNode)attrRoot.GetChild(i);
                    if(attrNode.GetChild(2).GetText() == "path")
                    {
                        string rawString = EscapeRustString(attrNode.GetChild(4).GetText());
                        return new PathSegment(rawString.Substring(1, rawString.Length - 2), true);
                    }
                }
            }
            var pubOrMod = (ITerminalNode)modNode.GetChild(currentChild);
            IParseTree identNode = modNode.GetChild(pubOrMod.Symbol.Type == ModuleParser.PUB ? currentChild + 2 : currentChild + 1);
            return new PathSegment(identNode.GetText(), false);
        }

        private static string EscapeRustString(string text)
        {
            StringBuilder b = new StringBuilder();
            bool spottedEscape = false;
            for(int i =0; i < text.Length; i++)
            {
                if(text[i] == '\\')
                {
                    if(i < text.Length - 1)
                    {
                        switch(text[i+1])
                        {
                            case '\\':
                                AppendSingleChar(text, b, ref spottedEscape, ref i, '\\'); break;
                            case 'n':
                                AppendSingleChar(text, b, ref spottedEscape, ref i, '\n'); break;
                            case 'r':
                                AppendSingleChar(text, b, ref spottedEscape, ref i, '\r'); break;
                            case 't':
                                AppendSingleChar(text, b, ref spottedEscape, ref i, '\t'); break;
                            case '0':
                                AppendSingleChar(text, b, ref spottedEscape, ref i, '\0'); break;
                            case 'x':
                                if(i < text.Length - 3)
                                    AppendHex(text, b, ref spottedEscape, ref i, 2); break;
                            case 'u':
                                if (i < text.Length - 5)
                                    AppendHex(text, b, ref spottedEscape, ref i, 4); break;
                            case 'U':
                                if (i < text.Length - 9)
                                    AppendHex(text, b, ref spottedEscape, ref i, 8); break;
                        }
                    }
                }

            }
            if (!spottedEscape)
                return text;
            return b.ToString();
        }

        private static void AppendSingleChar(string text, StringBuilder b, ref bool spotted, ref int idx, char target)
        {
            HandleFirstEscape(text, b, ref spotted, idx);
            b.Append(target);
            idx++;
        }

        private static void HandleFirstEscape(string text, StringBuilder b, ref bool spotted, int idx)
        {
            if (!spotted)
            {
                spotted = true;
                b.Append(text, 0, idx);
            }
        }

        private static void AppendHex(string text, StringBuilder b, ref bool spotted, ref int idx, int length)
        {
            uint value;
            if (!uint.TryParse(text.Substring(idx + 2, length), System.Globalization.NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
                return;
            HandleFirstEscape(text, b, ref spotted, idx);
            b.Append((char)value);
            idx += (length + 1);
        }
    }
}
