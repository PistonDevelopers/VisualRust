 using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using Microsoft.VisualStudioTools;
 using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace VisualRust.Project
{
    /*
     * From the perspective of VisualRust items can be split into five categories:
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
     *   ## Code files that were reached from module roots by automatic tracking
     *   ## Excluded files
     *      Nodes that are shown when you press "Show all files"
     *   Automatically-tracked nodes have priority over excluded files
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
     *   ## Excluded
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
    class RustProjectNode : CommonProjectNode
    {
        private ImageHandler handler;
        private bool containsEntryPoint;
        internal ModuleTracker ModuleTracker { get; private set; }

        public RustProjectNode(CommonProjectPackage package)
            : base(package, Utilities.GetImageList(new System.Drawing.Bitmap(typeof(RustProjectNode).Assembly.GetManifestResourceStream("VisualRust.Project.Resources.IconList.bmp"))))
        {
            this.CanFileNodesHaveChilds = false;
            this.CanProjectDeleteItems = true;
            this.ListenForStartupFileUpdates = false;
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

        public override int ImageIndex
        {
            get { return (int)IconIndex.NoIcon; }
        }

        public override object GetIconHandle(bool open)
        {
            return RustImageHandler.GetIconHandle((int)IconIndex.RustProject);
        }

        protected override void Reload()
        {
            string outputType = GetProjectProperty(ProjectFileConstants.OutputType, false);
            string entryPoint = Path.Combine(Path.GetDirectoryName(this.FileName), outputType == "library" ? @"src\lib.rs" : @"src\main.rs");
            containsEntryPoint = false;
            ModuleTracker = new ModuleTracker(entryPoint);
            base.Reload();
            // This project for some reason doesn't include entrypoint node, add it
            if (!containsEntryPoint)
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(entryPoint), true);
                TrackedFileNode node = (TrackedFileNode)this.CreateFileNode(entryPoint);
                node.IsEntryPoint = true;
                parent.AddChild(node);
            }
            foreach (string file in ModuleTracker.ExtractReachableAndMakeIncremental())
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(file), false);
                parent.AddChild(CreateUntrackedNode(file));
            }
            this.BuildProject.Save();
        }

        internal void OnNodeDirty(uint id)
        {
            BaseFileNode dirtyNode = this.NodeFromItemId(id) as BaseFileNode;
            if (dirtyNode != null && dirtyNode.GetModuleTracking())
                ReparseFileNode(dirtyNode);
        }

        private TrackedFileNode CreateTrackedNode(ProjectElement elm)
        {
            var node = new TrackedFileNode(this, elm);
            if (!ModuleTracker.IsIncremental)
            {
                ModuleTracker.AddRootModule(node.Url);
            }
            else
            {
                HashSet<string> children = ModuleTracker.AddRootModuleIncremental(node.Url);
                foreach(string child in children)
                {
                    HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(child), false);
                    parent.AddChild(CreateUntrackedNode(child));
                }
            }
            return node;
        }

        private UntrackedFileNode CreateUntrackedNode(string path)
        {
            var node = new UntrackedFileNode(this, path);
            return node;
        }

        public override FileNode CreateFileNode(ProjectElement item)
        {
            if (String.IsNullOrEmpty(item.ItemTypeName))
                return base.CreateFileNode(item);
            return CreateTrackedNode(item);
        }

        public override FileNode CreateFileNode(string file)
        {
            ProjectElement item = this.AddFileToMsBuild(file);
            return this.CreateFileNode(item);
        }

        internal override MsBuildProjectElement AddFileToMsBuild(string file)
        {
            string itemPath = Microsoft.VisualStudio.Shell.PackageUtilities.MakeRelativeIfRooted(file, this.BaseURI);
            System.Diagnostics.Debug.Assert(!Path.IsPathRooted(itemPath), "Cannot add item with full path.");
            return this.CreateMsBuildFileItem(itemPath, "File");
        }

        // This functions adds node with data that comes from parsing the .rsproj file
        protected override HierarchyNode AddIndependentFileNode(Microsoft.Build.Evaluation.ProjectItem item, HierarchyNode parent)
        {
            var node = (TrackedFileNode)base.AddIndependentFileNode(item, parent);
            if(node.GetModuleTracking())
            {
                if (node.Url.Equals(ModuleTracker.EntryPoint, StringComparison.InvariantCultureIgnoreCase))
                {
                    node.IsEntryPoint = true;
                    containsEntryPoint = true;
                }
            }
            return node;
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new RustProjectNodeProperties(this);
        }

        internal void IncludeFileNode(UntrackedFileNode node)
        {
            string path = node.Url;
            ModuleTracker.UpgradeModule(path);
            TreeOperations.Replace(this, node, () => CreateFileNode(path));
        }

        internal void ExcludeFileNode(BaseFileNode srcNode)
        {
            // Ask mod tracker for a professional opinion
            string fullPath = srcNode.Url;
            ModuleRemovalResult downgradeResult = ModuleTracker.DowngradeModule(fullPath);
            if (downgradeResult.IsReferenced)
            {
                TreeOperations.Replace(this, srcNode, () => CreateUntrackedNode(fullPath));
            }
            else
            {
                foreach (string path in downgradeResult.Orphans)
                {
                    TreeOperations.RemoveSubnodeFromHierarchy(this, path, false);
                }
            }
        }

        internal void DisableAutoImport(BaseFileNode node)
        {
            var orphans = ModuleTracker.DisableTracking(node.Url);
            foreach (string mod in orphans)
            {
                TreeOperations.RemoveSubnodeFromHierarchy(this, mod, false);
            }
        }

        internal void EnableAutoImport(BaseFileNode node)
        {
            var newMods = ModuleTracker.EnableTracking(node.Url);
            foreach (string mod in newMods)
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(mod), false);
                parent.AddChild(CreateUntrackedNode(mod));
            }
        }

        internal void ReparseFileNode(BaseFileNode n)
        {
            var diff = ModuleTracker.Reparse(n.Url);
            foreach(string mod in diff.Removed)
            {
                TreeOperations.RemoveSubnodeFromHierarchy(this, mod, false);
            }
            foreach (string mod in diff.Added)
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(mod), false);
                parent.AddChild(CreateUntrackedNode(mod));
            }
        }

        public override int SaveItem(VSSAVEFLAGS saveFlag, string silentSaveAsName, uint itemid, IntPtr docData, out int cancelled)
        {
            BaseFileNode node = this.NodeFromItemId(itemid) as BaseFileNode;
            if(node != null)
            {
                int result = base.SaveItem(saveFlag, silentSaveAsName, itemid, docData, out cancelled);
                if (result == VSConstants.S_OK)
                    ReparseFileNode(node);
                return result;
            }
            return base.SaveItem(saveFlag, silentSaveAsName, itemid, docData, out cancelled);
        }

        protected override ProjectElement AddFolderToMsBuild(string folder, bool createOnDisk = true)
        {
            if (!createOnDisk)
                return new VirtualProjectElement(this, folder);
            return base.AddFolderToMsBuild(folder, createOnDisk);
        }

        protected internal override FolderNode CreateFolderNode(ProjectElement element)
        {
            if (element == null)
                throw new ArgumentException("element");
            if (element is AllFilesProjectElement || !String.IsNullOrEmpty(element.ItemTypeName))
                return new CommonFolderNode(this, element);
            else
                return new UntrackedFolderNode(this, element);
        }

        public override CommonFileNode CreateNonCodeFileNode(ProjectElement item)
        {
            return new TrackedFileNode(this, item);
        }

        public override CommonFileNode CreateCodeFileNode(ProjectElement item)
        {
            return new TrackedFileNode(this, item);
        }

        protected override bool IncludeNonMemberItemInProject(HierarchyNode node)
        {
            return base.IncludeNonMemberItemInProject(node)
                || (node is UntrackedFileNode)
                || (node is UntrackedFolderNode);
        }

        internal void OnNodeIncluded(TrackedFileNode node)
        {
            HashSet<string> children = ModuleTracker.AddRootModuleIncremental(node.Url);
            foreach (string child in children)
            {
                HierarchyNode parent = this.CreateFolderNodes(Path.GetDirectoryName(child), false);
                parent.AddChild(CreateUntrackedNode(child));
            }
        }

        protected override ConfigProvider CreateConfigProvider()
        {
            return new RustConfigProvider(this);
        }

#region Disable "Add references..."
        protected override ReferenceContainerNode CreateReferenceContainerNode()
        {
            return null;
        }

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
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

        public override Type GetProjectFactoryType()
        {
            throw new NotImplementedException();
        }

        public override Type GetEditorFactoryType()
        {
            throw new NotImplementedException();
        }

        public override string GetProjectName()
        {
            throw new NotImplementedException();
        }

        public override string GetFormatList()
        {
            throw new NotImplementedException();
        }

        public override Type GetGeneralPropertyPageType()
        {
            return null;
        }

        public override Type GetLibraryManagerType()
        {
            return typeof(object);
        }

        public override IProjectLauncher GetLauncher()
        {
            var defaultLauncher = new DefaultRustLauncher(this);
            return defaultLauncher;
        }

        internal override string IssueTrackerUrl
        {
            get { throw new NotImplementedException(); }
        }

        protected override Guid[] GetConfigurationDependentPropertyPages()
        {
            return new[] {
                new Guid(Constants.BuildPropertyPage)
            };
        }

        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            return new[] {
                new Guid(Constants.ApplicationPropertyPage),
            };
        }
    }
}
