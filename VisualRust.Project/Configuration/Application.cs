using System;
using System.Collections.Generic;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project.Configuration
{
    partial class Application
    {
        private System.String crateName;
        public System.String CrateName { get { return crateName; }  set { crateName = value; } }
        private VisualRust.Shared.BuildOutputType outputType;
        public VisualRust.Shared.BuildOutputType OutputType { get { return outputType; }  set { outputType = value; } }

        public bool IsEqual(Application obj)
        {
            return true
                && EqualityComparer<System.String>.Default.Equals(CrateName, obj.CrateName)
                && EqualityComparer<VisualRust.Shared.BuildOutputType>.Default.Equals(OutputType, obj.OutputType)
            ;
        }

        public Application Clone()
        {
            return new Application
            {
                CrateName = this.CrateName,
                OutputType = this.OutputType,
            };
        }

        public static Application LoadFrom(CommonProjectNode proj)
        {
            var x = new Application();
            Utils.FromString(proj.GetUnevaluatedProperty("CrateName"), out x.crateName);
            x.OutputType = OutputTypeFromString(proj.GetUnevaluatedProperty("PlatformTarget"));
            return x;
        }

        public void SaveTo(CommonProjectNode proj)
        {
            proj.SetProjectProperty("CrateName", CrateName.ToString());
            proj.SetProjectProperty("PlatformTarget", OutputTypeToString(OutputType));
        }
    }
}

