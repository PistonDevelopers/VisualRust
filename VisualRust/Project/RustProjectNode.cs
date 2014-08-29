using Microsoft.VisualStudio.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    class RustProjectNode : ProjectNode
    {
        private Microsoft.VisualStudio.Shell.Package package;

        public RustProjectNode(Microsoft.VisualStudio.Shell.Package package)
        {
            this.package = package;
        }
        public override System.Guid ProjectGuid
        {
            get { return new System.Guid("F8DE8B7D-BE47-4D31-900E-F9576A926EB3"); }
        }

        public override string ProjectType
        {
            get { return "Rust"; }
        }
    }
}
