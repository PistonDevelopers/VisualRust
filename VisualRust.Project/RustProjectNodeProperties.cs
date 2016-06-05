using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VisualRust.Cargo;

namespace VisualRust.Project
{
    [CLSCompliant(false), ComVisible(true)]
    public class RustProjectNodeProperties : ProjectNodeProperties
    {
        public ManifestFile Manifest { get { return ((RustProjectNode)this.Node).Manifest; } }

        internal RustProjectNodeProperties(CommonProjectNode node)
            : base(node)
        { }

        [Browsable(false)]
        public __VSPROJOUTPUTTYPE OutputType
        {
            get
            {
                string type = this.Node.ProjectMgr.GetProjectProperty("OutputType");
                if(type.Equals("library", StringComparison.OrdinalIgnoreCase))
                {
                    return __VSPROJOUTPUTTYPE.VSPROJ_OUTPUTTYPE_LIBRARY;
                }
                else if (type.Equals("winexe", StringComparison.OrdinalIgnoreCase))
                {
                    return __VSPROJOUTPUTTYPE.VSPROJ_OUTPUTTYPE_WINEXE;
                }
                return __VSPROJOUTPUTTYPE.VSPROJ_OUTPUTTYPE_EXE;
            }
            set
            {
                string val;
                switch (value)
                {
                    case __VSPROJOUTPUTTYPE.VSPROJ_OUTPUTTYPE_WINEXE:
                        val = "winexe";
                        break;
                    case __VSPROJOUTPUTTYPE.VSPROJ_OUTPUTTYPE_LIBRARY:
                        val = "library";
                        break;
                    default:
                        val = "exe";
                        break;
                }
                this.Node.ProjectMgr.SetProjectProperty("OutputType", val);
            }
        }

        [Browsable(false)]
        public string URL
        {
            get { return Node.ProjectMgr.Url; }
        }
    }
}
