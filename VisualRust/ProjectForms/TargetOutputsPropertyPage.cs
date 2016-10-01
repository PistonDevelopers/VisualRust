//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using VisualRust.Project.Controls;

//namespace VisualRust.Project.Forms
//{
//    [ComVisible(true)]
//    [System.Runtime.InteropServices.Guid(Constants.TargetOutputsPage)]
//    public class TargetOutputsPropertyPage : WpfPropertyPage
//    {
//        public override string Name { get { return "Output Targets"; } }
//        protected override FrameworkElement CreateControl()
//        {
//            return new OutputPage();
//        }

//        protected override IPropertyPageContext CreateDataContext()
//        {
//            return new OutputTargetSectionViewModel(this.Config.Manifest, PickTargetOutputTypeWindow.Start);
//        }
//    }
//}
