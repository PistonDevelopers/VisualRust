using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

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
                TreeOperations.RemoveSubnodeFromHierarchy(root, path, (!forRemoval.IsReferenced) && path.Equals(srcpath, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        private static bool RemoveSubnodeFromHierarchy(RustProjectNode root, HierarchyNode node, bool deleteFromStorage)
        {
            node.Remove(deleteFromStorage);
            return true;
        }

        public static bool RemoveSubnodeFromHierarchy(RustProjectNode root, string path, bool deleteFromStorage)
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
                    TreeOperations.RemoveSubnodeFromHierarchy(root, node, deleteFromStorage);
                    return true;
                }
            }
            return false;
        }

        private static HierarchyNode ReplaceCore(RustProjectNode root, HierarchyNode old, Func<HierarchyNode> newN, HierarchyNode parent)
        {
            HierarchyNode newNode = newN();
            while (old.FirstChild != null)
            {
                HierarchyNode current = old.FirstChild;
                root.ProjectMgr.OnItemDeleted(current);
                old.RemoveChild(current);
                current.ID = root.ProjectMgr.ItemIdMap.Add(current);
                newNode.AddChild(current);
            }
            TreeOperations.RemoveSubnodeFromHierarchy(root, old, false);
            parent.AddChild(newNode);
            return newNode;
        }

        public static void Replace(RustProjectNode root, HierarchyNode old, Func<HierarchyNode> newN)
        {
            if (root == null)
                throw new ArgumentNullException("root");
            if (old == null)
                throw new ArgumentNullException("old");
            if (newN == null)
                throw new ArgumentNullException("newN");
            __VSHIERARCHYITEMSTATE visualState = old.GetItemState(__VSHIERARCHYITEMSTATE.HIS_Selected | __VSHIERARCHYITEMSTATE.HIS_Expanded);
            HierarchyNode parent = old.Parent;
            HierarchyNode newNode;
            if(parent is UntrackedFolderNode)
            {
                using(((UntrackedFolderNode)parent).SuspendChildrenTracking())
                { 
                    newNode = ReplaceCore(root, old, newN, parent);
                    ((UntrackedFolderNode)parent).OnChildReplaced(old, newNode);
                }
            }
            else
            {
                newNode = ReplaceCore(root, old, newN, parent);
            }
            if ((visualState & __VSHIERARCHYITEMSTATE.HIS_Expanded) != 0)
                newNode.ExpandItem(EXPANDFLAGS.EXPF_ExpandFolder);
            if ((visualState & __VSHIERARCHYITEMSTATE.HIS_Selected) != 0)
                newNode.ExpandItem(EXPANDFLAGS.EXPF_SelectItem);
        }
    }
}
