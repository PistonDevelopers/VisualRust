using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    /*
     * Sometimes, untracked nodes (automatically imported code files) will
     * materialize in subfolders.
     * Eg. main.rs: "mod foo { mod bar; }" will bring file foo\bar.rs into
     * the project. This means that we need to add a folder to the project.
     * Those temporary folder follow simple logic:
     * # They are created first time untracked file is created inside them
     *   (logic for that is contained inside ProjectNode and RustProjectNode)
     * # If any of its subnodes changes from untracked to tracked, it
     *   changes to tracked (note that the reverse operation, change from 
     *   tracked to untracked, does nothing!)
     * # If tracked node is added it gets changes to tracked
     * # If all its nodes are removed from the project, it gets removed too
     */
    class UntrackedFolderNode : CommonFolderNode
    {

        private class SuspendChildrenTrackingCookie : IDisposable
        {
            private UntrackedFolderNode node;

            public SuspendChildrenTrackingCookie(UntrackedFolderNode node)
            {
                this.node = node;
                this.node.suspendTracking = true;
            }
            public void Dispose() 
            {
                this.node.suspendTracking = false;
            }
        }

        private bool suspendTracking = false;
        private int untrackedChildren = 0;
        public new RustProjectNode ProjectMgr { get; private set; }

        public UntrackedFolderNode(RustProjectNode root, ProjectElement elm)
            : base(root, elm)
        {
            ProjectMgr = root;
        }

        public override int ImageIndex { get { return (int)IconIndex.NoIcon; } }

        public override object GetIconHandle(bool open)
        {
            return ProjectMgr.RustImageHandler.GetIconHandle((int)(open ? IconIndex.UntrackedFolderOpen : IconIndex.UntrackedFolder));
        }

        public override void AddChild(HierarchyNode node)
        {
            base.AddChild(node);
            if (suspendTracking || node.ItemNode.ItemTypeName == null)
                return;
            if (node is UntrackedFileNode || node is UntrackedFolderNode)
                untrackedChildren++;
            else
                ReplaceWithTracked();
        }

        private void ReplaceWithTracked()
        {
            TreeOperations.Replace(this.ProjectMgr, this, () => this.ProjectMgr.CreateFolderNode(this.Url));
        }

        private void DecrementTrackedCount()
        {
            untrackedChildren--;
            if (untrackedChildren == 0)
                this.Remove(false);
        }

        public override void RemoveChild(HierarchyNode node)
        {
            base.RemoveChild(node);
            if (!suspendTracking && (node is UntrackedFileNode || node is UntrackedFolderNode))
            {
                DecrementTrackedCount();
            }
        }

        public IDisposable SuspendChildrenTracking()
        {
            return new SuspendChildrenTrackingCookie(this);
        }

        internal void OnChildReplaced(HierarchyNode old, HierarchyNode @new)
        {
            if (old.Parent != @new.Parent)
                return;
            if(@new is TrackedFileNode || @new is FolderNode)
                ReplaceWithTracked();
        }
    }
}
