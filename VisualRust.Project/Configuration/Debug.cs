using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project.Configuration
{
    partial class Debug
    {
        public event EventHandler Changed;
        private StartAction? startActionQ;
        public StartAction? StartActionQ
        {
            get { return startActionQ; }
            set
            {
                startActionQ = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        } 
        private System.String externalProgram;
        public System.String ExternalProgram
        {
            get { return externalProgram; }
            set
            {
                externalProgram = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        } 
        private System.String commandLineArgs;
        public System.String CommandLineArgs
        {
            get { return commandLineArgs; }
            set
            {
                commandLineArgs = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        } 
        private System.String workingDir;
        public System.String WorkingDir
        {
            get { return workingDir; }
            set
            {
                workingDir = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        } 
        private System.String debuggerScript;
        public System.String DebuggerScript
        {
            get { return debuggerScript; }
            set
            {
                debuggerScript = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        } 

        public bool HasChangedFrom(Debug obj)
        {
            return false
            || (!EqualityComparer<StartAction?>.Default.Equals(StartActionQ, obj.StartActionQ))
            || (!EqualityComparer<System.String>.Default.Equals(ExternalProgram, obj.ExternalProgram))
            || (!EqualityComparer<System.String>.Default.Equals(CommandLineArgs, obj.CommandLineArgs))
            || (!EqualityComparer<System.String>.Default.Equals(WorkingDir, obj.WorkingDir))
            || (!EqualityComparer<System.String>.Default.Equals(DebuggerScript, obj.DebuggerScript))
            ;
        }

        public Debug Clone()
        {
            return new Debug
            {
                StartActionQ = this.StartActionQ,
                ExternalProgram = this.ExternalProgram,
                CommandLineArgs = this.CommandLineArgs,
                WorkingDir = this.WorkingDir,
                DebuggerScript = this.DebuggerScript,
            };
        }

        public static Debug LoadFrom(ProjectConfig[] configs)
        {
            return configs.Select(LoadFromForConfig).Aggregate((prev, cur) =>
            {
                if(prev.StartActionQ != null && !EqualityComparer<StartAction?>.Default.Equals(prev.StartActionQ, cur.StartActionQ))
                    prev.StartActionQ = null;
                if(prev.ExternalProgram != null && !EqualityComparer<System.String>.Default.Equals(prev.ExternalProgram, cur.ExternalProgram))
                    prev.ExternalProgram = null;
                if(prev.CommandLineArgs != null && !EqualityComparer<System.String>.Default.Equals(prev.CommandLineArgs, cur.CommandLineArgs))
                    prev.CommandLineArgs = null;
                if(prev.WorkingDir != null && !EqualityComparer<System.String>.Default.Equals(prev.WorkingDir, cur.WorkingDir))
                    prev.WorkingDir = null;
                if(prev.DebuggerScript != null && !EqualityComparer<System.String>.Default.Equals(prev.DebuggerScript, cur.DebuggerScript))
                    prev.DebuggerScript = null;
                return prev;
            });
        }

        private static Debug LoadFromForConfig(ProjectConfig cfg)
        {
            var x = new Debug();
            x.StartActionQ = StartActionQFromString(cfg.GetConfigurationProperty("StartAction", false));
            Utils.FromString(cfg.GetConfigurationProperty("StartProgram", false), out x.externalProgram);
            Utils.FromString(cfg.GetConfigurationProperty("StartArguments", false), out x.commandLineArgs);
            Utils.FromString(cfg.GetConfigurationProperty("StartWorkingDirectory", false), out x.workingDir);
            Utils.FromString(cfg.GetConfigurationProperty("StartDebuggerScript", false), out x.debuggerScript);
            return x;
        }

        public void SaveTo(ProjectConfig[] configs)
        {
            foreach(ProjectConfig cfg in configs)
            {
                if(StartActionQ != null)
                    cfg.SetConfigurationProperty("StartAction", StartActionQToString(StartActionQ));
                if(ExternalProgram != null)
                    cfg.SetConfigurationProperty("StartProgram", ExternalProgram.ToString());
                if(CommandLineArgs != null)
                    cfg.SetConfigurationProperty("StartArguments", CommandLineArgs.ToString());
                if(WorkingDir != null)
                    cfg.SetConfigurationProperty("StartWorkingDirectory", WorkingDir.ToString());
                if(DebuggerScript != null)
                    cfg.SetConfigurationProperty("StartDebuggerScript", DebuggerScript.ToString());
            }
        }
    }
}

