using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project
{
    class ResourceDisplayNameAttribute : DisplayNameAttribute
    {
        string key;
        public ResourceDisplayNameAttribute(string key)
        {
            this.key = key;
        }

        public override string DisplayName
        {
            get
            {
                return Properties.Resources.ResourceManager.GetString(this.key);
            }
        }
    }
}
