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
    /*
     * From the perspective of VisualRust files can be split into three categories:
     * # Tracked files (stored in .rsproj)
     *   # Module roots
     *     Files that are marked with "Automatically track module imports"
     *   # Others
     *     Files that are not marked with "Automatically track module imports"
     * # Untracked files (not stored in .rsproj, calculated ad-hoc)
     *   Code files that were reached from module roots by automatic tracking
     * This is good but there are few things to consider:
     * # What if we want to disable automatic tracking of module imports from untracked file?
     *   Can't be done, properties field for auto mod tracking is read-only True.
     *   This behaviour will be explained in the Description pane
     * # What if we want to "promote" untracked file to a tracked file?
     *   "Include in project"
     * # What if we want to "demote" tracked file to an untracked file?
     *   "Exclude from project"
     * # What about broken files (this will happen often when writing new modules top-down style)?
     *   We need a new command for this
     */
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
                BaseFileNode node = this.CreateFileNode(entryPoint);
                ((FileNode)node).IsEntryPoint = true;
                parent.AddChild(node);
            }
            foreach (string file in modTracker.ParseReachableNonRootModules())
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(file));
                parent.AddChild(new ReferencedFileNode(this, file));
            }
            this.BuildProject.Save();
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
                {
                    ((FileNode)node).IsEntryPoint = true;
                    containsEntryPoint = true;
                }
            }
            return node;
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new RustProjectNodeProperties(this);
        }
    }
}
