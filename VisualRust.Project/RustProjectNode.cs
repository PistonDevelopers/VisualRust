 using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

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
        private ImageHandler handler;
        private Microsoft.VisualStudio.Shell.Package package;
        private bool containsEntryPoint;
        internal ModuleTracker ModuleTracker { get; private set; }
        private FileChangeManager fileWatcher;

        public RustProjectNode(Microsoft.VisualStudio.Shell.Package package)
        {
            this.package = package;
            this.CanProjectDeleteItems = true;
        }

        public override int InitializeForOuter(string filename, string location, string name, uint flags, ref Guid iid, out IntPtr projectPointer, out int canceled)
        {
            return base.InitializeForOuter(filename, location, name, flags, ref iid, out projectPointer, out canceled);
        }

        public override System.Guid ProjectGuid
        {
            get { return typeof(RustProjectFactory).GUID; }
        }

        public override string ProjectType
        {
            get { return "Rust"; }
        }

        public ImageHandler RustImageHandler
        {
            get
            {
                if (null == handler)
                {
                    handler = new ImageHandler(VisualRust.Project.Properties.Resources.IconList);
                }
                return handler;
            }
        }

        public override object GetProperty(int propId)
        {
            if(propId == (int)__VSHPROPID.VSHPROPID_IconImgList)
                return RustImageHandler.ImageList.Handle;
            return base.GetProperty(propId);
        }

        public override int ImageIndex
        {
            get { return (int)IconIndex.RustProject; }
        }

        protected override int UnloadProject()
        {
            if (fileWatcher != null)
                fileWatcher.Dispose();
            return base.UnloadProject();
        }

        protected override void Reload()
        {
            fileWatcher = new FileChangeManager(this.Site);
            string outputType = GetProjectProperty(ProjectFileConstants.OutputType, false);
            string entryPoint = Path.Combine(Path.GetDirectoryName(this.FileName), outputType == "library" ? @"src\lib.rs" : @"src\main.rs");
            containsEntryPoint = false;
            ModuleTracker = new ModuleTracker(entryPoint);
            base.Reload();
            // This project for some reason doesn't include entrypoint node, add it
            if (!containsEntryPoint)
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(entryPoint));
                TrackedFileNode node = (TrackedFileNode)this.CreateFileNode(entryPoint);
                node.IsEntryPoint = true;
                parent.AddChild(node);
            }
            foreach (string file in ModuleTracker.ExtractReachableAndMakeIncremental())
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(file));
                parent.AddChild(CreateUntrackedNode(file));
            }
            fileWatcher.FileChangedOnDisk += OnFileChangedOnDisk;
            this.BuildProject.Save();
        }

        void OnFileChangedOnDisk(object sender, FileChangedOnDiskEventArgs e)
        {
            switch(e.FileChangeFlag)
            {
                case _VSFILECHANGEFLAGS.VSFILECHG_Time:
                case _VSFILECHANGEFLAGS.VSFILECHG_Size:
                case _VSFILECHANGEFLAGS.VSFILECHG_Add:
                    if (e.ItemID != (uint)VSConstants.VSITEMID.Nil)
                        OnFileDirty(e.ItemID);
                    break;
                case _VSFILECHANGEFLAGS.VSFILECHG_Del:
                    // For some reason VSFILECHG_Del sometimes gets raised on file change
                    if (e.ItemID != (uint)VSConstants.VSITEMID.Nil && !File.Exists(e.FileName))
                        TreeOperations.DeleteSubnode(this, e.FileName);
                    break;
            }
        }

        internal void OnFileDirty(uint id)
        {
            ReparseFileNode((BaseFileNode)this.NodeFromItemId(id));
        }

        private TrackedFileNode CreateTrackedNode(ProjectElement elm)
        {
            var node = new TrackedFileNode(this, elm);
            fileWatcher.ObserveItem(node.AbsoluteFilePath, node.ID);
            if (!node.GetModuleTracking())
                ModuleTracker.DisableTracking(node.AbsoluteFilePath);
            if (!ModuleTracker.IsIncremental)
            {
                ModuleTracker.AddRootModule(node.AbsoluteFilePath);
            }
            else
            {
                HashSet<string> children = ModuleTracker.AddRootModuleIncremental(node.AbsoluteFilePath);
                foreach(string child in children)
                {
                    HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(child));
                    parent.AddChild(CreateUntrackedNode(child));
                }
            }
            return node;
        }

        private UntrackedFileNode CreateUntrackedNode(string path)
        {
            var node = new UntrackedFileNode(this, path);
            fileWatcher.ObserveItem(node.AbsoluteFilePath, node.ID);
            return node;
        }

        // This gets called  by AddIndependentFileNode so it will always create tracked node
        public override FileNode CreateFileNode(ProjectElement item)
        {
            return CreateTrackedNode(item);
        }

        public override FileNode CreateFileNode(string file)
        {
            ProjectElement item = this.AddFileToMsBuild(file);
            return this.CreateFileNode(item);
        }

        protected override ProjectElement AddFileToMsBuild(string file)
        {
            string itemPath = Microsoft.VisualStudio.Shell.PackageUtilities.MakeRelativeIfRooted(file, this.BaseURI);
            System.Diagnostics.Debug.Assert(!Path.IsPathRooted(itemPath), "Cannot add item with full path.");
            return this.CreateMsBuildFileItem(itemPath, "File");
        }

        // This functions adds node with data that comes from parsing the .rsproj file
        protected override HierarchyNode AddIndependentFileNode(Microsoft.Build.Evaluation.ProjectItem item)
        {
            var node = (TrackedFileNode)base.AddIndependentFileNode(item);
            if(node.GetModuleTracking())
            {
                if(node.AbsoluteFilePath.Equals(ModuleTracker.EntryPoint, StringComparison.InvariantCultureIgnoreCase))
                {
                    node.IsEntryPoint = true;
                    containsEntryPoint = true;
                }
            }
            else
            {
                ModuleTracker.DisableTracking(node.AbsoluteFilePath);
            }
            return node;
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new RustProjectNodeProperties(this);
        }

        internal void IncludeFileNode(UntrackedFileNode node)
        {
            string path = node.AbsoluteFilePath;
            ModuleTracker.UpgradeModule(path);
            TreeOperations.ReplaceAndSelect(this, node, () => CreateFileNode(path));
        }

        internal void ExcludeFileNode(BaseFileNode srcNode)
        {
            // Ask mod tracker for a professional opinion
            string fullPath = srcNode.AbsoluteFilePath;
            ModuleRemovalResult downgradeResult = ModuleTracker.DowngradeModule(fullPath);
            if (downgradeResult.IsReferenced)
            {
                TreeOperations.ReplaceAndSelect(this, srcNode, () => CreateUntrackedNode(fullPath));
            }
            else
            {
                foreach (string path in downgradeResult.Orphans)
                {
                    TreeOperations.RemoveSubnode(this, path, false);
                }
            }
        }

        internal void DisableAutoImport(BaseFileNode node)
        {
            var orphans = ModuleTracker.DisableTracking(node.AbsoluteFilePath);
            foreach (string mod in orphans)
            {
                TreeOperations.RemoveSubnode(this, mod, false);
            }
        }

        internal void EnableAutoImport(BaseFileNode node)
        {
            var newMods = ModuleTracker.EnableTracking(node.AbsoluteFilePath);
            foreach (string mod in newMods)
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(mod));
                parent.AddChild(CreateUntrackedNode(mod));
            }
        }

        internal void ReparseFileNode(BaseFileNode n)
        {
            var diff = ModuleTracker.Reparse(n.AbsoluteFilePath);
            foreach(string mod in diff.Removed)
            {
                TreeOperations.RemoveSubnode(this, mod, false);
            }
            foreach (string mod in diff.Added)
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(mod));
                parent.AddChild(CreateUntrackedNode(mod));
            }
        }

        public override int SaveItem(VSSAVEFLAGS saveFlag, string silentSaveAsName, uint itemid, IntPtr docData, out int cancelled)
        {
            BaseFileNode node = this.NodeFromItemId(itemid) as BaseFileNode;
            if(node != null)
            {
                try
                {
                    fileWatcher.IgnoreItemChanges(node.AbsoluteFilePath, true);
                    int result = base.SaveItem(saveFlag, silentSaveAsName, itemid, docData, out cancelled);
                    if(result == VSConstants.S_OK)
                        ReparseFileNode(node);
                    return result;
                }
                catch
                {
                    throw;
                }
                finally
                {
                    fileWatcher.IgnoreItemChanges(node.AbsoluteFilePath, false);
                }
            }
            return base.SaveItem(saveFlag, silentSaveAsName, itemid, docData, out cancelled);
        }

        protected override bool IsItemTypeFileType(string type)
        {
            return String.Equals("file", type, StringComparison.OrdinalIgnoreCase);
        }

#region Disable "Add references..."
        protected override ReferenceContainerNode CreateReferenceContainerNode()
        {
            return null;
        }

        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        { 
            if (cmdGroup == VsMenus.guidStandardCommandSet2K && (VsCommands2K)cmd == VsCommands2K.ADDCOMPONENTS
                || cmdGroup == VSConstants.CMDSETID.StandardCommandSet12_guid && (VSConstants.VSStd12CmdID)cmd == VSConstants.VSStd12CmdID.AddReferenceProjectOnly)
            {
                result |= QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                return (int)VSConstants.S_OK;
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        public override int AddProjectReference()
        {
            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        public override IReferenceContainer GetReferenceContainer()
        {
            return null;
        }

        public override int AddComponent(VSADDCOMPOPERATION dwAddCompOperation, uint cComponents, IntPtr[] rgpcsdComponents, IntPtr hwndDialog, VSADDCOMPRESULT[] pResult)
        {
            return VSConstants.E_NOTIMPL;
        }
#endregion
    }
}
