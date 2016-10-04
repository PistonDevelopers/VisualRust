// TODO: IProjectTreeModifier has been replaced by IProjectTreePropertiesProvider in VS15
//#if VS14
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.Designers;

namespace VisualRust {
    [Export(typeof(IProjectTreeModifier))]
    [AppliesTo(VisualRustPackage.UniqueCapability)]
    internal sealed class ProjectTreeModifier : IProjectTreeModifier {
        public IProjectTree ApplyModifications(IProjectTree tree, IProjectTreeProvider projectTreeProvider) {
            if (tree != null) {
                if (tree.Capabilities.Contains(ProjectTreeCapabilities.ProjectRoot)) {
                    tree = tree.SetIcon(ProjectIconProvider.ProjectNodeImage.ToProjectSystemType());
                } else if (tree.Capabilities.Contains(ProjectTreeCapabilities.FileOnDisk)) {
                    string ext = Path.GetExtension(tree.FilePath).ToLowerInvariant();
                    if (ext == ".rs")
                    {
                        tree = tree.SetIcon(ProjectIconProvider.RustFileNodeImage.ToProjectSystemType());
                    }
                    /*else if (ext == ".md")
                    {
                        tree = tree.SetIcon(KnownMonikers.MarkdownFile.ToProjectSystemType());
                    }*/
                }
            }
            return tree;
        }
    }
}
//#endif
