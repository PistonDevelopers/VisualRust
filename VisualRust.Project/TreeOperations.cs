using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
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
        public static void DeleteSubnode(RustProjectNode root, BaseFileNode srcNode)
        {
            string srcpath = srcNode.AbsoluteFilePath;
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
            root.ModuleTracker.DeleteModule(node.AbsoluteFilePath);
            node.Remove(deleteFromStorage);
            return true;
        }

        public static bool RemoveSubnode(RustProjectNode root, string path, bool deleteFromStorage)
        {
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

        public static void ReplaceAndSelect(RustProjectNode root, BaseFileNode old, Func<HierarchyNode> newN)
        {
            var parent = old.Parent;
            TreeOperations.RemoveSubnode(root, old, false);
            HierarchyNode newNode = newN();
            parent.AddChild(newNode);
            // Adjust UI
            IVsUIHierarchyWindow uiWindow = UIHierarchyUtilities.GetUIHierarchyWindow(root.ProjectMgr.Site, HierarchyNode.SolutionExplorer);
            if (uiWindow != null)
            {
                ErrorHandler.ThrowOnFailure(uiWindow.ExpandItem(root.ProjectMgr.InteropSafeIVsUIHierarchy, newNode.ID, EXPANDFLAGS.EXPF_SelectItem));
            }
        }
    }
}
