using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VisualRust.Cargo;

namespace VisualRust.Project.Controls
{
    public partial class OutputPage : DockPanel
    {
        public OutputPage()
        {
            InitializeComponent();
        }
    }

    public class ItemTemplateSelector: DataTemplateSelector
    {
        public DataTemplate Benchmark { get; set; }
        public DataTemplate Binary { get; set; }
        public DataTemplate Example { get; set; }
        public DataTemplate Library { get; set; }
        public DataTemplate Test { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            IOutputTargetViewModel vm = item as IOutputTargetViewModel;
            if(item == null)
                return base.SelectTemplate(item, container);
            switch (vm.Type)
            {
                case OutputTargetType.Benchmark:
                   return Benchmark;
                case OutputTargetType.Binary:
                   return Binary;
                case OutputTargetType.Example:
                   return Example;
                case OutputTargetType.Library:
                   return Library;
                case OutputTargetType.Test:
                   return Test;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
