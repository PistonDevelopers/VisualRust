using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    public struct ModuleRemovalResult
    {
        public HashSet<string> Orphans { get; private set; }
        public bool IsReferenced { get; private set; }

        public ModuleRemovalResult(HashSet<string> o, bool isRef) : this()
        {
            Orphans = o;
            IsReferenced = isRef;
        }
    }
}
