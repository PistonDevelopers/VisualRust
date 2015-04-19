using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project.Configuration
{
    partial class Build
    {
        public event EventHandler Changed;
        private bool? lTO;
        public bool? LTO
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
        private bool? emitDebug;
        public bool? EmitDebug
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
        private VisualRust.Shared.OptimizationLevel? optimizationLevel;
        public VisualRust.Shared.OptimizationLevel? OptimizationLevel
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
            || (!EqualityComparer<bool?>.Default.Equals(LTO, obj.LTO))
            || (!EqualityComparer<bool?>.Default.Equals(EmitDebug, obj.EmitDebug))
            || (!EqualityComparer<VisualRust.Shared.OptimizationLevel?>.Default.Equals(OptimizationLevel, obj.OptimizationLevel))
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

        public static Build LoadFrom(ProjectConfig[] configs)
        {
            return configs.Select(LoadFromForConfig).Aggregate((prev, cur) =>
            {
                if(prev.LTO != null && !EqualityComparer<bool?>.Default.Equals(prev.LTO, cur.LTO))
                    prev.LTO = null;
                if(prev.EmitDebug != null && !EqualityComparer<bool?>.Default.Equals(prev.EmitDebug, cur.EmitDebug))
                    prev.EmitDebug = null;
                if(prev.OptimizationLevel != null && !EqualityComparer<VisualRust.Shared.OptimizationLevel?>.Default.Equals(prev.OptimizationLevel, cur.OptimizationLevel))
                    prev.OptimizationLevel = null;
                if(prev.PlatformTarget != null && !EqualityComparer<System.String>.Default.Equals(prev.PlatformTarget, cur.PlatformTarget))
                    prev.PlatformTarget = null;
                return prev;
            });
        }

        private static Build LoadFromForConfig(ProjectConfig cfg)
        {
            var x = new Build();
            Utils.FromString(cfg.GetConfigurationProperty("LinkTimeOptimization", false), out x.lTO);
            Utils.FromString(cfg.GetConfigurationProperty("DebugSymbols", false), out x.emitDebug);
            x.OptimizationLevel = OptimizationLevelFromString(cfg.GetConfigurationProperty("OptimizationLevel", false));
            x.PlatformTarget = PlatformTargetFromString(cfg.GetConfigurationProperty("PlatformTarget", false));
            return x;
        }

        public void SaveTo(ProjectConfig[] configs)
        {
            foreach(ProjectConfig cfg in configs)
            {
                if(LTO != null)
                    cfg.SetConfigurationProperty("LinkTimeOptimization", LTO.ToString());
                if(EmitDebug != null)
                    cfg.SetConfigurationProperty("DebugSymbols", EmitDebug.ToString());
                if(OptimizationLevel != null)
                    cfg.SetConfigurationProperty("OptimizationLevel", OptimizationLevelToString(OptimizationLevel));
                if(PlatformTarget != null)
                    cfg.SetConfigurationProperty("PlatformTarget", PlatformTargetToString(PlatformTarget));
            }
        }
    }
}

