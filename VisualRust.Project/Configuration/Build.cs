using System;
using System.Collections.Generic;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project.Configuration
{
    partial class Build
    {
        private System.Boolean useDefaultTarget;
        public System.Boolean UseDefaultTarget { get { return useDefaultTarget; }  set { useDefaultTarget = value; } }
        private System.String platformTarget;
        public System.String PlatformTarget { get { return platformTarget; }  set { platformTarget = value; } }

        public bool IsEqual(Build obj)
        {
            return true
                && EqualityComparer<System.Boolean>.Default.Equals(UseDefaultTarget, obj.UseDefaultTarget)
                && EqualityComparer<System.String>.Default.Equals(PlatformTarget, obj.PlatformTarget)
            ;
        }

        public Build Clone()
        {
            return new Build
            {
                UseDefaultTarget = this.UseDefaultTarget,
                PlatformTarget = this.PlatformTarget,
            };
        }

        public static Build LoadFrom(CommonProjectNode proj)
        {
            var x = new Build();
            x.UseDefaultTarget = LoadUseDefaultTarget(proj);
            Utils.FromString(proj.GetUnevaluatedProperty("PlatformTarget"), out x.platformTarget);
            return x;
        }

        public void SaveTo(CommonProjectNode proj)
        {
            SaveUseDefaultTarget();
            proj.SetProjectProperty("PlatformTarget", PlatformTarget.ToString());
        }
    }
}

