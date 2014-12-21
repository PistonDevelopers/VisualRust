using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    /*
     * RustProjectNode was starting to bloat with all kind of
     * subnode manipulation functions.
     * Most of them has been split here for readability.
     */
    static class TreeOperations
    {
         // Mass delete all sbunodes that were orphaned by the 
         // removal of a subnode with path `rootPath`
        public static void DeleteSubnode(RustProjectNode root, string srcpath)
        {
            if (root == null)
                throw new ArgumentNullException("root");
            if (String.IsNullOrEmpty(srcpath))
                throw new ArgumentException("srcpath");

            var forRemoval = root.ModuleTracker.DeleteModule(srcpath);
            foreach (string path in forRemoval.Orphans)
            {
                TreeOperations.RemoveSubnode(root, path, (!forRemoval.IsReferenced) && path.Equals(srcpath, StringComparison.InvariantCultureIgnoreCase));
            }
            if (forRemoval.IsReferenced)
            {
                // TODO: Mark node of deleted file as a zombie
            }
        }

        public static bool RemoveSubnode(RustProjectNode root, BaseFileNode node, bool deleteFromStorage)
        {
            if (root == null)
                throw new ArgumentNullException("root");
            if (node == null)
                throw new ArgumentNullException("node");
            root.ModuleTracker.DeleteModule(node.AbsoluteFilePath);
            node.Remove(deleteFromStorage);
            return true;
        }

        public static bool RemoveSubnode(RustProjectNode root, string path, bool deleteFromStorage)
        {
            if (root == null)
                throw new ArgumentNullException("root");
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("path");
            uint item;
            root.ParseCanonicalName(path, out item);
            if (item != (uint)VSConstants.VSITEMID.Nil)
            {
                HierarchyNode node = root.NodeFromItemId(item);
                if (node != null)
                {
                    TreeOperations.RemoveSubnode(root, (BaseFileNode)node, deleteFromStorage);
                    return true;
                }
            }
            return false;
        }

        private static HierarchyNode ReplaceAndSelectCore(RustProjectNode root, BaseFileNode old, Func<HierarchyNode> newN, HierarchyNode parent)
        {
            TreeOperations.RemoveSubnode(root, old, false);
            HierarchyNode newNode = newN();
            parent.AddChild(newNode);
            return newNode;
        }

        public static void ReplaceAndSelect(RustProjectNode root, BaseFileNode old, Func<HierarchyNode> newN)
        {
            if (root == null)
                throw new ArgumentNullException("root");
            if (old == null)
                throw new ArgumentNullException("old");
            if (newN == null)
                throw new ArgumentNullException("newN");
            HierarchyNode parent = old.Parent;
            HierarchyNode newNode;
            if(parent is UntrackedFolderNode)
            {
                using(((UntrackedFolderNode)parent).SuspendChildrenTracking())
                { 
                    newNode = ReplaceAndSelectCore(root, old, newN, parent);
                    ((UntrackedFolderNode)parent).OnChildReplaced(old, newNode);
                }
            }
            else
            {
                newNode = ReplaceAndSelectCore(root, old, newN, parent);
            }
            // Adjust UI
            IVsUIHierarchyWindow uiWindow = UIHierarchyUtilities.GetUIHierarchyWindow(root.ProjectMgr.Site, HierarchyNode.SolutionExplorer);
            if (uiWindow != null)
            {
                ErrorHandler.ThrowOnFailure(uiWindow.ExpandItem(root.ProjectMgr.InteropSafeIVsUIHierarchy, newNode.ID, EXPANDFLAGS.EXPF_SelectItem));
            }
        }
    }
}
