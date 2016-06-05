using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace VisualRust.Project.Controls
{
    public class ChildWindow : Window
    {
        public ChildWindow()
        {
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            UseLayoutRounding = true;
            Title = "Visual Rust";
        }

        public ChildWindow(Window owner) : this()
        {
            Owner = owner;
        }
    }
}
