using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace VisualRust.Project
{
    public class RustProjectFactory : ProjectFactory
    {
        public RustProjectFactory(ProjectPackage pkg) : base(pkg) { }

        protected override ProjectNode CreateProject()
        {
            RustProjectNode project = new RustProjectNode(this.Package);
            project.SetSite((IOleServiceProvider)((IServiceProvider)this.Package).GetService(typeof(IOleServiceProvider)));
            return project;
        }
    }
}
