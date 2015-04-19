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

        public bool HasChangedFrom(Application obj)
        {
            return false
            || (!EqualityComparer<VisualRust.Shared.BuildOutputType>.Default.Equals(OutputType, obj.OutputType))
            || (!EqualityComparer<System.String>.Default.Equals(CrateName, obj.CrateName))
            ;
        }

        public Application Clone()
        {
            return new Application
            {
                OutputType = this.OutputType,
                CrateName = this.CrateName,
            };
        }


        public static Application LoadFrom(CommonProjectNode proj)
        {
            var x = new Application();
            x.OutputType = OutputTypeFromString(proj.GetUnevaluatedProperty("OutputType"));
            Utils.FromString(proj.GetUnevaluatedProperty("CrateName"), out x.crateName);
            return x;
        }

        public void SaveTo(CommonProjectNode proj)
        {
            proj.SetProjectProperty("OutputType", OutputTypeToString(OutputType));
            proj.SetProjectProperty("CrateName", CrateName.ToString());
        }
    }
}

