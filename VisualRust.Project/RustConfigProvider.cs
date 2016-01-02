using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    class RustConfigProvider : ConfigProvider
    {
        private CommonProjectNode project;

        public RustConfigProvider(CommonProjectNode proj) 
            : base(proj)
        {
            this.project = proj;
        }

        public override int GetPlatformNames(uint celt, string[] names, uint[] actual)
        {
            string[] platforms = GetPropertiesConditionedOn(ProjectFileConstants.Platform);
            if (platforms == null || platforms.Length == 0) {
                platforms = new string[] { Shared.Environment.DefaultTarget };
            }
            return GetPlatforms(celt, names, actual, platforms);
        }

        public override int GetSupportedPlatformNames(uint celt, string[] names, uint[] actual)
        {
            string[] platforms  = 
                new string[] { Shared.Environment.DefaultTarget }
                .Union(Shared.Environment.FindInstalledTargets().Select(tt => tt.ToString()) )
                .ToArray();
            return GetPlatforms(celt, names, actual, platforms);
        }

        protected override ProjectConfig CreateProjectConfiguration(string configName)
        {
            return project.MakeConfiguration(configName);
        }

        public override int GetCfgProviderProperty(int propid, out object var)
        {
          var = false;
          return VSConstants.S_OK;
        }
    }
}
