using Microsoft.VisualStudio.Project;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace VisualRust.Project
{
    [CLSCompliant(false), ComVisible(true)]
    abstract public class FileNodePropertiesBase : NodeProperties
    {
        [SRCategoryAttribute(SR.Misc)]
        [LocDisplayName(SR.FileName)]
        [SRDescriptionAttribute(SR.FileNameDescription)]
        public string FileName
        {
            get
            {
                return this.Node.Caption;
            }
            set
            {
                this.Node.SetEditLabel(value);
            }
        }

        [SRCategoryAttribute(SR.Misc)]
        [LocDisplayName(SR.FullPath)]
        [SRDescriptionAttribute(SR.FullPathDescription)]
        public string FullPath
        {
            get
            {
                return this.Node.Url;
            }
        }

        [Browsable(false)]
        public string URL
        {
            get { return this.Node.Url; }
        }

        [Browsable(false)]
        public string Extension
        {
            get
            {
                return Path.GetExtension(this.Node.Caption);
            }
        }

        public FileNodePropertiesBase(HierarchyNode node)
            : base(node)
        { }

        public override string GetClassName()
        {
            return SR.GetString(SR.FileProperties, CultureInfo.CurrentUICulture);
        }
    }

    [CLSCompliant(false), ComVisible(true)]
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
            return "VisualRust.Project.TrackedFileNodeProperties";
        }
    }

    [CLSCompliant(false), ComVisible(true)]
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
            return "VisualRust.Project.UntrackedFileNodeProperties";
        }
    }
}
