using Microsoft.VisualStudio.Project;
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
     * # If any of its subnodes changes from untracked to tracked, it
     *   changes to tracked (note that the reverse operation, change from 
     *   tracked to untracked, does nothing!)
     * # If all its nodes are removed from the project, it gets removed too
     */
    class UntrackedFolderNode : FolderNode
    {
        public new RustProjectNode ProjectMgr { get; private set; }

        public UntrackedFolderNode(RustProjectNode node, string relativePath)
            : base(node, relativePath, null)
        {
            ProjectMgr = node;
        }

        public override object GetIconHandle(bool open)
        {
            return ProjectMgr.RustImageHandler.GetIconHandle((int)(open ? IconIndex.UntrackedFolderOpen : IconIndex.UntrackedFolder));
        }
    }
}
