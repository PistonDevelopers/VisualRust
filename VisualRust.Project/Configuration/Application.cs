using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project.Configuration
{
    partial class Application
    {
        public event EventHandler Changed;
        private VisualRust.Shared.BuildOutputType outputType;
        public VisualRust.Shared.BuildOutputType OutputType
        {
            get { return outputType; }
            set
            {
                outputType = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        } 
        private System.String crateName;
        public System.String CrateName
        {
            get { return crateName; }
            set
            {
                crateName = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        } 
        private System.Boolean buildDylib;
        public System.Boolean BuildDylib
        {
            get { return buildDylib; }
            set
            {
                buildDylib = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        } 
        private System.Boolean buildRlib;
        public System.Boolean BuildRlib
        {
            get { return buildRlib; }
            set
            {
                buildRlib = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        } 
        private System.Boolean buildStaticlib;
        public System.Boolean BuildStaticlib
        {
            get { return buildStaticlib; }
            set
            {
                buildStaticlib = value;
                var temp = Changed;
                if(temp != null)
                    temp(this, new EventArgs());
            }
        } 

        public bool HasChangedFrom(Application obj)
        {
            return false
            || (!EqualityComparer<VisualRust.Shared.BuildOutputType>.Default.Equals(OutputType, obj.OutputType))
            || (!EqualityComparer<System.String>.Default.Equals(CrateName, obj.CrateName))
            || (!EqualityComparer<System.Boolean>.Default.Equals(BuildDylib, obj.BuildDylib))
            || (!EqualityComparer<System.Boolean>.Default.Equals(BuildRlib, obj.BuildRlib))
            || (!EqualityComparer<System.Boolean>.Default.Equals(BuildStaticlib, obj.BuildStaticlib))
            ;
        }

        public Application Clone()
        {
            return new Application
            {
                OutputType = this.OutputType,
                CrateName = this.CrateName,
                BuildDylib = this.BuildDylib,
                BuildRlib = this.BuildRlib,
                BuildStaticlib = this.BuildStaticlib,
            };
        }


        public static Application LoadFrom(CommonProjectNode proj)
        {
            var x = new Application();
            x.OutputType = OutputTypeFromString(proj.GetUnevaluatedProperty("OutputType"));
            Utils.FromString(proj.GetUnevaluatedProperty("CrateName"), out x.crateName);
            Utils.FromString(proj.GetUnevaluatedProperty("BuildDylib"), out x.buildDylib);
            Utils.FromString(proj.GetUnevaluatedProperty("BuildRlib"), out x.buildRlib);
            Utils.FromString(proj.GetUnevaluatedProperty("BuildStaticlib"), out x.buildStaticlib);
            return x;
        }

        public void SaveTo(CommonProjectNode proj)
        {
            proj.SetProjectProperty("OutputType", OutputTypeToString(OutputType));
            proj.SetProjectProperty("CrateName", CrateName.ToString());
            proj.SetProjectProperty("BuildDylib", BuildDylib.ToString());
            proj.SetProjectProperty("BuildRlib", BuildRlib.ToString());
            proj.SetProjectProperty("BuildStaticlib", BuildStaticlib.ToString());
        }
    }
}

