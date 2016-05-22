using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VisualRust.Project.Controls
{
    class AlwaysEnabledButton : Button
    {
        static AlwaysEnabledButton()
        {
            IsEnabledProperty.OverrideMetadata(
                typeof(AlwaysEnabledButton),
                new FrameworkPropertyMetadata(true, null, CoerceIsEnabled));
            IsHitTestVisibleProperty.OverrideMetadata(
                typeof(AlwaysEnabledButton),
                new FrameworkPropertyMetadata(true, null, CoerceIsEnabled));
        }

        static object CoerceIsEnabled(DependencyObject source, object value)
        {
            return value;
        }
    }
}
