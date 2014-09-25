using Microsoft.VisualStudio.Project;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace VisualRust.Project
{
    [CLSCompliant(false), ComVisible(true)]
    public class FileNodeProperties : NodeProperties
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
        public string Extension
        {
            get
            {
                return Path.GetExtension(this.Node.Caption);
            }
        }

        public FileNodeProperties(HierarchyNode node)
            : base(node)
        { }

        public override string GetClassName()
        {
            return SR.GetString(SR.FileProperties, CultureInfo.CurrentUICulture);
        }
    }
}
