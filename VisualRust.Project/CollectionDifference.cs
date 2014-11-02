using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    public struct CollectionDifference<T>
    {
        public T Added { get; private set; }
        public T Removed { get; private set; }
        public CollectionDifference(T add, T rem)
            : this()
        {
            Added = add;
            Removed = rem;
        }
    }
}
