using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    public class ModuleImport : Dictionary<string, ModuleImport>
    {
        public ModuleImport() : base(StringComparer.InvariantCulture) { }

        public void Merge(Dictionary<string, ModuleImport> obj)
        {
            foreach(var kvp in obj)
            {
                if(this.ContainsKey(kvp.Key))
                {
                    this[kvp.Key].Merge(kvp.Value);
                }
                else
                {
                    this.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }
}
