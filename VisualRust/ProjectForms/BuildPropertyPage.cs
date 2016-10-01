//using Microsoft.VisualStudioTools.Project;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace VisualRust.Project.Forms
//{
//    [ComVisible(true)]
//    [System.Runtime.InteropServices.Guid(Constants.BuildPropertyPage)]
//    public class BuildPropertyPage : BasePropertyPage
//    {
//        private readonly BuildPropertyControl control;

//        public BuildPropertyPage()
//        {
//            control = new BuildPropertyControl(isDirty => IsDirty = isDirty);
//        }

//        public override System.Windows.Forms.Control Control
//        {
//            get { return control; }
//        }

//        public override void Apply()
//        {
//            control.ApplyConfig(Configs);
//            IsDirty = false;
//        }

//        public override void LoadSettings()
//        {
//            Loading = true;
//            try {
//                control.LoadSettings(Configs);
//            } finally {
//                Loading = false;
//            }
//        }

//        public override string Name
//        {
//            get { return "Build"; }
//        }
//    }
//}
