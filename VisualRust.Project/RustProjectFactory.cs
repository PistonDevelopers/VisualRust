using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace VisualRust.Project
{
    [Guid("78C9907C-C22D-4F8D-B13A-49F213FF1631")]
    public class RustProjectFactory : ProjectFactory
    {
        CommonProjectPackage pkg;
        public RustProjectFactory(CommonProjectPackage pkg)
            : base(pkg) 
        {
            this.pkg = pkg;
        }

        internal override ProjectNode CreateProject()
        {
            RustProjectNode project = new RustProjectNode(this.pkg);
            project.SetSite((IOleServiceProvider)((IServiceProvider)this.Package).GetService(typeof(IOleServiceProvider)));
            return project;
        }
    }
}
