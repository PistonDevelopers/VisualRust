using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    public struct PathSegment
    {
        public string Name { get; private set; }
        // This property is true if the value came from #[path  = ...], meaning we
        // have actual filename in the Name property
        public bool IsAuthorative { get; private set; }

        public PathSegment(string name)
            : this(name, false)
        {
        }

        public PathSegment(string name, bool auth) : this()
        {
            Name = name;
            IsAuthorative = auth;
        }
    }
}
