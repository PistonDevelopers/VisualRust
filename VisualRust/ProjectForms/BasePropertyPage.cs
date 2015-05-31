using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project.Forms
{
    [ComVisible(true)]
    public abstract class BasePropertyPage : CommonPropertyPage
    {
        internal ProjectConfig[] Configs { get; private set; }

        public override void SetObjects(uint cObjects, object[] ppunk)
        {
            if(ppunk != null)
                Configs = ppunk.OfType<ProjectConfig>().ToArray();
            else
                Configs = new ProjectConfig[0];
            base.SetObjects(cObjects, ppunk);
        }
    }
}
