using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    class ReferencedFileNode : FileNode
    {

        private string filePath;
        public override string FilePath { get { return filePath; } }

        public ReferencedFileNode(ProjectNode root, string file)
            : base(root, null)
        {
            this.filePath = file;
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new FileNodeProperties(this);
        }
    }
}
