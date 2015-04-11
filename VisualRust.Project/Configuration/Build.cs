using System;
using System.Collections.Generic;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project.Configuration
{
    partial class Build
    {
        public event EventHandler Changed;
        private System.Boolean lTO;
        public System.Boolean LTO
        {
            get { return lTO; }
            set
            {
                lTO = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        }
        private System.Boolean emitDebug;
        public System.Boolean EmitDebug
        {
            get { return emitDebug; }
            set
            {
                emitDebug = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        }
        private VisualRust.Shared.OptimizationLevel optimizationLevel;
        public VisualRust.Shared.OptimizationLevel OptimizationLevel
        {
            get { return optimizationLevel; }
            set
            {
                optimizationLevel = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        }
        private System.String platformTarget;
        public System.String PlatformTarget
        {
            get { return platformTarget; }
            set
            {
                platformTarget = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        }

        public bool HasChangedFrom(Build obj)
        {
            return false
            || (!EqualityComparer<System.Boolean>.Default.Equals(LTO, obj.LTO))
            || (!EqualityComparer<System.Boolean>.Default.Equals(EmitDebug, obj.EmitDebug))
            || (!EqualityComparer<VisualRust.Shared.OptimizationLevel>.Default.Equals(OptimizationLevel, obj.OptimizationLevel))
            || (!EqualityComparer<System.String>.Default.Equals(PlatformTarget, obj.PlatformTarget))
            ;
        }

        public Build Clone()
        {
            return new Build
            {
                LTO = this.LTO,
                EmitDebug = this.EmitDebug,
                OptimizationLevel = this.OptimizationLevel,
                PlatformTarget = this.PlatformTarget,
            };
        }

        public static Build LoadFrom(CommonProjectNode proj)
        {
            var x = new Build();
            Utils.FromString(proj.GetUnevaluatedProperty("LinkTimeOptimization"), out x.lTO);
            Utils.FromString(proj.GetUnevaluatedProperty("DebugSymbols"), out x.emitDebug);
            x.OptimizationLevel = OptimizationLevelFromString(proj.GetUnevaluatedProperty("OptimizationLevel"));
            x.PlatformTarget = PlatformTargetFromString(proj.GetUnevaluatedProperty("PlatformTarget"));
            return x;
        }

        public void SaveTo(CommonProjectNode proj)
        {
            proj.SetProjectProperty("LinkTimeOptimization", LTO.ToString());
            proj.SetProjectProperty("DebugSymbols", EmitDebug.ToString());
            proj.SetProjectProperty("OptimizationLevel", OptimizationLevelToString(OptimizationLevel));
            proj.SetProjectProperty("PlatformTarget", PlatformTargetToString(PlatformTarget));
        }
    }
}

