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

        public IEnumerable<string[]> GetTerminalImports()
        {
            return GetTerminalImports(this, new Queue<string>());
        }

        private IEnumerable<string[]> GetTerminalImports(ModuleImport current, Queue<string> queue)
        {
            foreach(var kvp in current)
            {
                if(kvp.Value.Count == 0)
                {
                    string[] result = new string[queue.Count + 1];
                    queue.CopyTo(result, 0);
                    result[result.Length - 1] = kvp.Key;
                    yield return result;
                }
                else
                {
                    queue.Enqueue(kvp.Key);
                    foreach(var value in GetTerminalImports(kvp.Value, queue))
                    {
                        yield return value;
                    }
                    queue.Dequeue();
                }
            }
        }
    }
}
