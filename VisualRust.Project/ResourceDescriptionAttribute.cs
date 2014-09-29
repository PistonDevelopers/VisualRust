using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    class ResourceDescriptionAttribute : DescriptionAttribute
    {
        string key;
        public ResourceDescriptionAttribute(string key)
        {
            this.key = key;
        }

        public override string Description
        {
            get
            {
                return Properties.Resources.ResourceManager.GetString(this.key);
            }
        }
    }
}
