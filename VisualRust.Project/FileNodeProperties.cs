using Microsoft.VisualStudioTools.Project;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace VisualRust.Project
{
    [ComVisible(true)]
    abstract public class FileNodePropertiesBase : NodeProperties
    {
        [SRCategoryAttribute(SR.Misc)]
        [SRDisplayName(SR.FileName)]
        [SRDescriptionAttribute(SR.FileNameDescription)]
        public string FileName
        {
            get
            {
                return this.HierarchyNode.Caption;
            }
            set
            {
                this.HierarchyNode.SetEditLabel(value);
            }
        }

        [SRCategoryAttribute(SR.Misc)]
        [SRDisplayName(SR.FullPath)]
        [SRDescriptionAttribute(SR.FullPathDescription)]
        public string FullPath
        {
            get
            {
                return this.HierarchyNode.Url;
            }
        }

        [Browsable(false)]
        public string URL
        {
            get { return this.HierarchyNode.Url; }
        }

        [Browsable(false)]
        public string Extension
        {
            get
            {
                return Path.GetExtension(this.HierarchyNode.Caption);
            }
        }

        internal FileNodePropertiesBase(HierarchyNode node)
            : base(node)
        { }

        public override string GetClassName()
        {
            return SR.GetString(SR.FileProperties, CultureInfo.CurrentUICulture);
        }
    }

    [ComVisible(true)]
    public class FileNodeProperties : FileNodePropertiesBase
    {
        [CategoryAttribute("Advanced")]
        [ResourceDisplayName("TrackModules")]
        [ResourceDescription("TrackModulesDescription")]
        [DefaultValue(true)]
        public bool TrackModules
        {
            get { return ((TrackedFileNode)this.Node).GetModuleTracking(); }
            set { ((TrackedFileNode)this.Node).SetModuleTracking(value); }
        }

        internal FileNodeProperties(TrackedFileNode node)
            : base(node)
        { }

        public override string GetClassName()
        {
            return "File Properties";
        }
    }

    [ComVisible(true)]
    public class ReferencedFileNodeProperties : FileNodePropertiesBase
    {
        [CategoryAttribute("Advanced")]
        [ResourceDisplayName("TrackModules")]
        [ResourceDescription("TrackModulesReferencedDescription")]
        public bool TrackModules
        {
            get { return true; }
        }

        internal ReferencedFileNodeProperties(UntrackedFileNode node)
            : base(node)
        { }

        public override string GetClassName()
        {
            return "File Properties";
        }
    }

    [ComVisible(true)]
    public class ExcludedFileNodeProperties : FileNodePropertiesBase
    {
        [CategoryAttribute("Advanced")]
        [ResourceDisplayName("TrackModules")]
        [ResourceDescription("TrackModulesReferencedDescription")]
        public bool TrackModules
        {
            get { return false; }
        }

        internal ExcludedFileNodeProperties(TrackedFileNode node)
            : base(node)
        { }

        public override string GetClassName()
        {
            return "File Properties";
        }
    }
}
