using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace VisualRust.Project.Controls
{
    /*
     * To na untrained eye this may seem like a madness, but it's the only way to have a ListBoxItem
     * with some content visually outside of it _without_ redefining its Template
     */
    public class ButtonListBoxItem : ListBoxItem
    {
        ButtonListBoxItemPanel panel;

        public ButtonListBoxItem()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var presenter = FindVisualChild<ContentPresenter>(this);
            if (presenter != null)
            {
                presenter.ApplyTemplate();
                panel = this.ContentTemplate.FindName("PART_Panel", presenter) as ButtonListBoxItemPanel;
                if (panel != null)
                    panel.Parent = this;
            }
        }

        static T FindVisualChild<T>(DependencyObject curr) where T : DependencyObject
        {
            int length = VisualTreeHelper.GetChildrenCount(curr);
            for (int i = 0; i < length; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(curr, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            if (panel != null)
            {
                this.RenderTransform = new TranslateTransform(-(panel.Layout.ShrinkX / 2), 0);
                return base.ArrangeOverride(new Size(arrangeBounds.Width - panel.Layout.ShrinkX, arrangeBounds.Height));
            }
            else
            {
                return base.ArrangeOverride(arrangeBounds);
            }
        }

        static void MeasureContentChild(UIElement uie, ref Size available, ref Size desired)
        {
            uie.Measure(available);
            available = new Size(available.Width - uie.DesiredSize.Width, available.Height);
            desired = new Size(desired.Width + uie.DesiredSize.Width, Math.Max(desired.Height, uie.DesiredSize.Height));
        }

        static Size ArrangeContentChild(UIElement uie, Size available, double contentWidth)
        {
            Size desired = uie.DesiredSize;
            uie.Arrange(new Rect(new Point(contentWidth, 0), new Size(desired.Width, available.Height)));
            return desired;
        }

        public class StackLayout : IPanelLayout
        {
            readonly IList<UIElement> uies;

            public StackLayout(IList<UIElement> uies)
            {
                this.uies = uies;
            }

            public double ShrinkX { get { return 0; } }

            public Size Measure(Size fullSize)
            {
                Size available = fullSize;
                Size desired = new Size();
                for (int i = 0; i < uies.Count; i++)
                {
                    MeasureContentChild(uies[i], ref available, ref desired);
                }
                return desired;
            }

            public Size Arrange(Size available)
            {
                double contentWidth = 0;
                for (int i = 0; i < uies.Count - 1; i++)
                {
                    Size arranged = ArrangeContentChild(uies[i], available, contentWidth);
                    contentWidth += arranged.Width;
                }
                return available;
            }
        }

        public class SpecialLayout : IPanelLayout
        {
            readonly IList<UIElement> uies;
            readonly double topSpace;
            readonly double bottomSpace;
            readonly double leftSpace;
            readonly double rightSpace;
            readonly double separator;

            public double ShrinkX { get; private set; }

            public SpecialLayout(IList<UIElement> uies, double topSpace, double bottomSpace, double leftSpace, double rightSpace, double separator)
            {
                this.leftSpace = leftSpace;
                this.rightSpace = rightSpace;
                this.uies = uies;
                this.separator = separator;
                this.bottomSpace = bottomSpace;
                this.topSpace = topSpace;
                this.separator = separator;
            }

            public Size Measure(Size fullSize)
            {
                Size available = fullSize;
                Size desired = new Size();
                for (int i = 0; i < uies.Count - 1; i++)
                {
                    MeasureContentChild(uies[i], ref available, ref desired);
                }
                MeasureButton(uies[uies.Count - 1], available, ref desired);
                ShrinkX = uies[uies.Count - 1].DesiredSize.Width + separator;
                return desired;
            }

            // When reporting measured width we skip the button, because border around ListBoxItem
            // draws with Math.Max(availableSize.Width, DesiredSize.Width)
            void MeasureButton(UIElement uie, Size available, ref Size desired)
            {
                uie.Measure(available);
                desired = new Size(desired.Width, Math.Max(desired.Height, uie.DesiredSize.Height - bottomSpace - topSpace));
            }

            public Size Arrange(Size available)
            {
                double contentWidth = 0;
                for (int i = 0; i < uies.Count - 1; i++)
                {
                    Size arranged = ArrangeContentChild(uies[i], available, contentWidth);
                    contentWidth += arranged.Width;
                }
                ArrangeButton(available);
                return available;
            }

            private void ArrangeButton(Size available)
            {
                Size button = uies[uies.Count - 1].DesiredSize;
                uies[uies.Count - 1].Arrange(
                    new Rect(
                        new Point(available.Width + separator + rightSpace, -topSpace),
                        new Size(button.Width, available.Height + topSpace + bottomSpace)));
            }
        }
    }

    class ButtonListBoxItemPanel : Panel
    {
        public static readonly DependencyProperty UseBaseLayoutProperty =
            DependencyProperty.Register(
                "UseBaseLayout",
                typeof(bool),
                typeof(ButtonListBoxItemPanel),
                new FrameworkPropertyMetadata { AffectsMeasure = true, AffectsArrange = true });

        public bool UseBaseLayout
        {
            get { return (bool)GetValue(UseBaseLayoutProperty); }
            set { SetValue(UseBaseLayoutProperty, value); }
        }

        public new ButtonListBoxItem Parent { get; set; }
        internal IPanelLayout Layout { get; private set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            IList<UIElement> children = Enumerable.Range(0, VisualChildrenCount).Select(i => GetVisualChild(i)).Cast<UIElement>().ToList();
            if (UseBaseLayout)
            {
                this.Layout = new ButtonListBoxItem.StackLayout(children);
            }
            else
            {
                this.Layout = new ButtonListBoxItem.SpecialLayout(
                    children,
                    Parent.BorderThickness.Top + Parent.Padding.Top,
                    Parent.BorderThickness.Bottom + Parent.Padding.Bottom,
                    Parent.BorderThickness.Left + Parent.Padding.Left,
                    Parent.BorderThickness.Right + Parent.Padding.Right,
                    1);
            }
            return Layout.Measure(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return Layout.Arrange(finalSize);
        }
    }
}