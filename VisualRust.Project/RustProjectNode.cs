using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
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
     * From the perspective of VisualRust items can be split into four categories:
     * # Tracked files (stored in .rsproj)
     *   ## Module roots with auto-import
     *      Code files that will automatically include all reference file 
     *      (eg. "mod foo;" inside them adds untracked foo.rs)
     *   ## Module roots
     *      Code files with disabled "Automatically track module imports".
     *      (This distinction is important for module roots with auto-import,
     *      eg. auto-importing main.rs includes foo.rs with disabled auto-tracking,
     *      tracking includes of foo.rs should be skipped)
     *   ## Others
     *      Other files that have disabled option for auto-tracking. Bitmaps, sql files, docs, etc.
     * # Untracked files (not stored in .rsproj, calculated ad-hoc)
     *   Code files that were reached from module roots by automatic tracking
     * Also, items can be in a zombie stated (one way or another item is present in the project
     * hierarchy but actual file doesn't exist on disk)
     *
     * Operations for project items and their behavior:
     * # Delete
     *   ## Module roots with auto-import
     *      Delete the file, exclude untracked files that depend only on it.
     *      This might turn the item into a zombified Untracked item.
     *   ## Module roots, Untracked
     *      Delete the file. This might turn the item into a zombified Untracked item.
     *   ## Others
     *      Simply delete the file and the item
     * # Create (new command)
     *   ## Module roots (with/without auto-tracking), Untracked
     *      If the file does not exist, create it
     * # Include
     *   ## Untracked
     *      Convert to a module root with auto-import
     * # Exclude
     *   ## Module roots with auto-import
     *      Exclude from the project and exclude all auto-imported references.
     *   ## Module roots
     *      Exclude from the project. As a side-effect, this might re-add
     *      this module as an Untracked item and cascadingly add new Unracked items.
     *      Surprising, but correct!
     * # Disable auto-import
     *   ## Module roots with auto-import
     *      Get the list of Untrackd items depending ony on this root, exclude them
     * # Enable auto-import
     *   ## Module roots
     *      Parse and add newly found Untracked items
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
            foreach (string file in modTracker.ExtractReachableAndMakeIncremental())
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
                modTracker.AddRootModule(node.Url);
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


        internal void DeleteFileNode(BaseFileNode srcNode)
        {
            var result = modTracker.DeleteModule(srcNode.Url);
            foreach (string path in result.Orphans)
            {
                uint item;
                this.ParseCanonicalName(path, out item);
                if (item != (uint)VSConstants.VSITEMID.Nil)
                {
                    HierarchyNode node = this.NodeFromItemId(item);
                    node.Remove((!result.IsReferenced) && node == srcNode);
                }
            }
            if (result.IsReferenced)
            {
                // TODO: Mark node of deleted file as a zombie
            }
            this.BuildProject.Save();
        }

        private void ReplaceAndSelect(HierarchyNode old, HierarchyNode newN)
        {
            HierarchyNode parent = old.Parent;
            old.Remove(false);
            parent.AddChild(newN);
            // Adjust UI
            IVsUIHierarchyWindow uiWindow = UIHierarchyUtilities.GetUIHierarchyWindow(this.ProjectMgr.Site, SolutionExplorer);
            if (uiWindow != null)
            {
                ErrorHandler.ThrowOnFailure(uiWindow.ExpandItem(this.ProjectMgr.InteropSafeIVsUIHierarchy, newN.ID, EXPANDFLAGS.EXPF_SelectItem));
            }
        }

        internal void IncludeFileNode(ReferencedFileNode node)
        {
            modTracker.UpgradeModule(node.FilePath);
            ReplaceAndSelect(node, CreateFileNode(node.FilePath));
        }

        internal void ExcludeFileNode(BaseFileNode srcNode)
        {
            // Ask mod tracker for a professional opinion
            string fullPath = GetAbsolutePath(srcNode);
            RootRemovalResult downgradeResult = modTracker.DowngradeModule(fullPath);
            if (downgradeResult.IsReferenced)
            {
                ReplaceAndSelect(srcNode, new ReferencedFileNode(this, fullPath));
            }
            else
            {
                foreach (string path in downgradeResult.Orphans)
                {
                    uint item;
                    this.ParseCanonicalName(path, out item);
                    if (item != (uint)VSConstants.VSITEMID.Nil)
                    {
                        HierarchyNode node = this.NodeFromItemId(item);
                        node.Remove(false);
                    }
                }
            }
        }

        private string GetAbsolutePath(BaseFileNode node)
        {
            string path = node.FilePath;
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            else
            {
                // Path is relative, so make it relative to project path
                return Path.Combine(this.ProjectMgr.BaseURI.AbsoluteUrl, path);
            }
        }

        internal void DisableAutoImport(HierarchyNode node)
        {

        }

        internal void EnableAutoImport(HierarchyNode node)
        {

        }
    }
}
