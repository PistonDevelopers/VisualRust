using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project.Forms
{
    [ComVisible(true)]
    [Guid(Constants.DebugPropertyPage)]
    public class DebugPropertyPage : BasePropertyPage
    {
        private readonly DebugPropertyControl control;

        public DebugPropertyPage()
        {
            control = new DebugPropertyControl(isDirty => IsDirty = isDirty);
        }

        public override System.Windows.Forms.Control Control
        {
            get { return control; }
        }

        public override void Apply()
        {
            control.ApplyConfig(Configs);
            IsDirty = false;
        }

        public override void LoadSettings()
        {
            Loading = true;
            try {
                control.LoadSettings(Configs);
            } finally {
                Loading = false;
            }
        }

        public override string Name
        {
            get { return "Debug"; }
        }
    }
}
