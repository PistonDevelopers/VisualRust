using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    public struct ModuleImport
    {
        public string Ident { get; private set; }
        public List<ModuleImport> Children { get; private set; }

        public ModuleImport(string ident)
            : this()
        {
            Ident = ident;
            Children = new List<ModuleImport>(0);
        }

        public ModuleImport(List<ModuleImport> children)
            : this()
        {
            Children = children;
        }

        public ModuleImport(string ident, List<ModuleImport> children)
            : this()
        {
            Ident = ident;
            Children = children;
        }
    }
}
