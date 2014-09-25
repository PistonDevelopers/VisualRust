using Microsoft.VisualStudio.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseFileNode = Microsoft.VisualStudio.Project.FileNode;

namespace VisualRust.Project
{
    class FileNode : BaseFileNode
    {
        public bool IsEntryPoint { get; private set; }

        public FileNode(ProjectNode root, ProjectElement elm) : base(root, elm)
        {

        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new FileNodeProperties(this);
        }
    }
}
