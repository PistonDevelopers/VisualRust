using System.Windows;

namespace VisualRust.Project.Controls
{
    public interface IPanelLayout
    {
        double ShrinkX { get; }

        Size Arrange(Size available);
        Size Measure(Size fullSize);
    }
}