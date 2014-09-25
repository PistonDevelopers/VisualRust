using Microsoft.VisualStudio.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseFileNode = Microsoft.VisualStudio.Project.FileNode;

namespace VisualRust.Project
{
    class RustProjectNode : ProjectNode
    {
        private Microsoft.VisualStudio.Shell.Package package;
        private bool containsEntryPoint;
        private ModuleTracker modTracker;

        public RustProjectNode(Microsoft.VisualStudio.Shell.Package package)
        {
            this.package = package;
            this.CanProjectDeleteItems = true;
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
            string entryPoint = Path.Combine(Path.GetDirectoryName(this.FileName), outputType == "library" ? @"src\lib.rs" : @"src\main.rs");
            containsEntryPoint = false;
            modTracker = new ModuleTracker(entryPoint);
            base.Reload();
            // This project for some reason doesn't include entrypoint node, add it
            if (!containsEntryPoint)
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(entryPoint));
                parent.AddChild(this.CreateFileNode(entryPoint));
            }
            foreach (string file in modTracker.ParseReachableNonRootModules())
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(file));
                parent.AddChild(new ReferencedFileNode(this, file));
            }
        }

        public override BaseFileNode CreateFileNode(ProjectElement item)
        {
            return new FileNode(this, item);
        }

        public override BaseFileNode CreateFileNode(string file)
        {
            ProjectElement item = this.AddFileToMsBuild(file);
            return this.CreateFileNode(item);
        }

        protected override HierarchyNode AddIndependentFileNode(Microsoft.Build.Evaluation.ProjectItem item)
        {
            HierarchyNode node = base.AddIndependentFileNode(item);
            if(node.GetRelationNameExtension() == ".rs")
            {
                modTracker.AddRoot(node.Url);
                if (node.Url == modTracker.EntryPoint)
                    containsEntryPoint = true;
            }
            return node;
        }
    }
}
