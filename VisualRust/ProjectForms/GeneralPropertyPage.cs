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
//    [System.Runtime.InteropServices.Guid(Constants.ApplicationPropertyPage)]
//    public class ApplicationPropertyPage : BasePropertyPage
//    {
//        private readonly ApplicationPropertyControl control;

//        public ApplicationPropertyPage()
//        {
//            control = new ApplicationPropertyControl(isDirty => IsDirty = isDirty);
//        }

//        public override System.Windows.Forms.Control Control
//        {
//            get { return control; }
//        }

//        public override void Apply()
//        {
//            control.ApplyConfig(this.Project);
//            IsDirty = false;
//        }

//        public override void LoadSettings()
//        {
//            Loading = true;
//            try {
//                control.LoadSettings(this.Project);
//            } finally {
//                Loading = false;
//            }
//        }

//        public override string Name
//        {
//            get { return "General"; }
//        }
//    }
//}
