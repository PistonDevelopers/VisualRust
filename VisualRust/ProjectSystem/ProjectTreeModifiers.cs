using System;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.ProjectSystem;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.Designers;
using Microsoft.VisualStudio.ProjectSystem.Designers;
#endif

namespace VisualRust.ProjectSystem
{
    static class ProjectTreeIcons
    {
        public static ProjectImageMoniker GetProjectIcon()
        {
            return ProjectIconProvider.ProjectNodeImage.ToProjectSystemType();
        }

        public static ProjectImageMoniker GetChildIcon(string fileName)
        {
            if (fileName == "cargo.toml")
            {
                return ProjectIconProvider.CargoManifestNodeImage.ToProjectSystemType();
            }
            else
            {
                string ext = Path.GetExtension(fileName);
                if (ext == ".rs")
                {
                    return ProjectIconProvider.RustFileNodeImage.ToProjectSystemType();
                }
                else if (ext == ".md")
                {
                    return KnownMonikers.MarkdownFile.ToProjectSystemType();
                }
            }
            return null;
        }
    }


    [AppliesTo(VisualRustPackage.UniqueCapability)]
#if VS14
    [Export(typeof(IProjectTreeModifier))]
    internal sealed class ProjectTreeModifier : IProjectTreeModifier
    {
        public IProjectTree ApplyModifications(IProjectTree tree, IProjectTreeProvider projectTreeProvider)
        {
            if (tree != null)
            {
                if (tree.Capabilities.Contains(ProjectTreeCapabilities.ProjectRoot))
                {
                    tree = tree.SetIcon(ProjectTreeIcons.GetProjectIcon());
                }
                else if (tree.Capabilities.Contains(ProjectTreeCapabilities.FileOnDisk))
                {
                    string name = Path.GetFileName(tree.FilePath).ToLowerInvariant();
                    ProjectImageMoniker icon = ProjectTreeIcons.GetChildIcon(name);
                    if (icon != null)
                        tree.SetIcon(icon);
                }
            }
            return tree;
        }
    }
#else
    [Export(typeof(IProjectTreePropertiesProvider))]
    internal sealed class ProjectTreePropertiesProvider : IProjectTreePropertiesProvider
    {
        public void CalculatePropertyValues(IProjectTreeCustomizablePropertyContext propertyContext, IProjectTreeCustomizablePropertyValues propertyValues)
        {
            if (propertyValues.Flags.Contains(ProjectTreeFlags.Common.ProjectRoot))
            {
                propertyValues.Icon = ProjectTreeIcons.GetProjectIcon();
            }
            else if (propertyValues.Flags.Contains(ProjectTreeFlags.Common.FileOnDisk))
            {
                string name = propertyContext.ItemName.ToLowerInvariant();
                ProjectImageMoniker icon = ProjectTreeIcons.GetChildIcon(name);
                if (icon != null)
                    propertyValues.Icon = icon;
            }
        }
    }
#endif
}
