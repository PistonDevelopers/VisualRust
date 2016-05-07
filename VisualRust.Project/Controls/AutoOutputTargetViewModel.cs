using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Cargo;

namespace VisualRust.Project.Controls
{
    abstract class AutoOutputTargetViewModel : IOutputTargetViewModel
    {
        protected const string AutoDetect = "(auto-detect)";

        protected Manifest Manifest { get; private set; }

        internal AutoOutputTargetViewModel(Manifest m)
        {
            this.Manifest = m;
        }
        public abstract OutputTargetType Type { get; }
        public string TabName { get { return AutoDetect; } }
        public abstract string Name { get; }
        public abstract string Path { get; }
        public virtual bool Bench { get { return true; } }
        public virtual bool Doc { get { return false; } }
        public virtual bool Doctest { get { return false; } }
        public virtual bool Harness { get { return true; } }
        public virtual bool Plugin { get { return false; } }
        public virtual bool Test { get { return true; } }
        public virtual bool IsReadOnly { get { return true; } }
    }
}
