using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    [CLSCompliant(false), ComVisible(true)]
    public class RustProjectNodeProperties : ProjectNodeProperties
    {
        internal RustProjectNodeProperties(CommonProjectNode node)
            : base(node)
        { }

        [Browsable(false)]
        public VSLangProj.prjOutputType OutputType
        {
            get
            {
                string type = this.Node.ProjectMgr.GetProjectProperty("OutputType");
                if(type.Equals("library", StringComparison.OrdinalIgnoreCase))
                {
                    return VSLangProj.prjOutputType.prjOutputTypeLibrary;
                }
                else if (type.Equals("winexe", StringComparison.OrdinalIgnoreCase))
                {
                    return VSLangProj.prjOutputType.prjOutputTypeWinExe;
                }
                return VSLangProj.prjOutputType.prjOutputTypeExe;
            }
            set
            {
                string val;
                switch (value)
                {
                    case VSLangProj.prjOutputType.prjOutputTypeWinExe:
                        val = "winexe";
                        break;
                    case VSLangProj.prjOutputType.prjOutputTypeLibrary:
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
