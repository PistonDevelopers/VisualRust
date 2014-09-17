using Microsoft.VisualStudio.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    class RustProjectNode : ProjectNode
    {
        private Microsoft.VisualStudio.Shell.Package package;
        private ModuleTracker modTracker;

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

        protected override void Reload()
        {
            string outputType = GetProjectProperty(ProjectFileConstants.OutputType, false);
            string entryPoint = Path.Combine(Path.GetDirectoryName(this.FileName), outputType == "library" ? @"\src\lib.rs" : @"\src\main.rs");
            modTracker = new ModuleTracker(Path.GetDirectoryName(this.FileName) + entryPoint);
            base.Reload();
            foreach (string file in modTracker.ParseReachableNonRootModules())
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(file));
                parent.AddChild(this.CreateFileNode(file));
            }
        }

        protected override HierarchyNode AddIndependentFileNode(Microsoft.Build.Evaluation.ProjectItem item)
        {
            HierarchyNode node = base.AddIndependentFileNode(item);
            if(node.GetRelationNameExtension() == ".rs")
            {
                modTracker.AddRoot(node.Url);
            }
            return node;
        }
    }
}
