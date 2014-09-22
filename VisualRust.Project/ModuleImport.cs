using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    public class ModuleImport : Dictionary<PathSegment, ModuleImport>
    {
        public void Merge(Dictionary<PathSegment, ModuleImport> obj)
        {
            foreach(var kvp in obj)
            {
                if (this.ContainsKey(kvp.Key))
                {
                    this[kvp.Key].Merge(kvp.Value);
                }
                else if (this.ContainsKey(new PathSegment(kvp.Key.Name, !kvp.Key.IsAuthorative)))
                {
                    this[new PathSegment(kvp.Key.Name, !kvp.Key.IsAuthorative)].Merge(kvp.Value);
                }
                else
                {
                    this.Add(kvp.Key, kvp.Value);
                }
            }
        }

        public IEnumerable<PathSegment[]> GetTerminalImports()
        {
            return GetTerminalImports(this, new Queue<PathSegment>());
        }

        private IEnumerable<PathSegment[]> GetTerminalImports(ModuleImport current, Queue<PathSegment> queue)
        {
            foreach(var kvp in current)
            {
                if(kvp.Value.Count == 0)
                {
                    PathSegment[] result = new PathSegment[queue.Count + 1];
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
