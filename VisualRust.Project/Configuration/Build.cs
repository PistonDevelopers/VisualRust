using System;
using System.Collections.Generic;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project.Configuration
{
    partial class Build
    {
        private System.String platformTarget;
        public System.String PlatformTarget { get { return platformTarget; }  set { platformTarget = value; } }
        private VisualRust.Shared.OptimizationLevel optimizationLevel;
        public VisualRust.Shared.OptimizationLevel OptimizationLevel { get { return optimizationLevel; }  set { optimizationLevel = value; } }
        private System.Boolean lTO;
        public System.Boolean LTO { get { return lTO; }  set { lTO = value; } }
        private System.Boolean emitDebug;
        public System.Boolean EmitDebug { get { return emitDebug; }  set { emitDebug = value; } }

        public bool IsEqual(Build obj)
        {
            return true
                && EqualityComparer<System.String>.Default.Equals(PlatformTarget, obj.PlatformTarget)
                && EqualityComparer<VisualRust.Shared.OptimizationLevel>.Default.Equals(OptimizationLevel, obj.OptimizationLevel)
                && EqualityComparer<System.Boolean>.Default.Equals(LTO, obj.LTO)
                && EqualityComparer<System.Boolean>.Default.Equals(EmitDebug, obj.EmitDebug)
            ;
        }

        public Build Clone()
        {
            return new Build
            {
                PlatformTarget = this.PlatformTarget,
                OptimizationLevel = this.OptimizationLevel,
                LTO = this.LTO,
                EmitDebug = this.EmitDebug,
            };
        }

        public static Build LoadFrom(CommonProjectNode proj)
        {
            var x = new Build();
            x.PlatformTarget = PlatformTargetFromString(proj.GetUnevaluatedProperty("PlatformTarget"));
            x.OptimizationLevel = OptimizationLevelFromString(proj.GetUnevaluatedProperty("OptimizationLevel"));
            Utils.FromString(proj.GetUnevaluatedProperty("LinkTimeOptimization"), out x.lTO);
            Utils.FromString(proj.GetUnevaluatedProperty("DebugSymbols"), out x.emitDebug);
            return x;
        }

        public void SaveTo(CommonProjectNode proj)
        {
            proj.SetProjectProperty("PlatformTarget", PlatformTargetToString(PlatformTarget));
            proj.SetProjectProperty("OptimizationLevel", OptimizationLevelToString(OptimizationLevel));
            proj.SetProjectProperty("LinkTimeOptimization", LTO.ToString());
            proj.SetProjectProperty("DebugSymbols", EmitDebug.ToString());
        }
    }
}

