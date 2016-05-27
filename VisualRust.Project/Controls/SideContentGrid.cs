using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace VisualRust.Project.Controls
{
    class SideContentGrid : Panel
    {
        class LayoutProxy : UIElement
        {
            public UIElement Element { get; set; }

            public LayoutProxy(UIElement elm)
            {
                Element = elm;
            }

            protected override Size MeasureCore(Size availableSize)
            {
                Element.Measure(availableSize);
                return Element.DesiredSize;
            }

            protected override void ArrangeCore(Rect finalRect)
            {
                Element.Arrange(finalRect);
            }
        }

        public static readonly DependencyProperty SideContentTemplateProperty =
            DependencyProperty.Register("SideContentTemplate", typeof(DataTemplate), typeof(SideContentGrid));
        public DataTemplate SideContentTemplate
        {
            get { return this.GetValue(SideContentTemplateProperty) as DataTemplate; }
            set { this.SetValue(SideContentTemplateProperty, value); }
        }

        private Grid innerGrid;
        private FrameworkElement[] sideContents = new FrameworkElement[0];
        private bool measuredOnce;
        private bool ignoreVisualChange;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitializeProxyLayout();
        }

        private void InitializeProxyLayout()
        {
            foreach (UIElement uie in sideContents)
                this.RemoveVisualChild(uie);
            innerGrid = new Grid();
            sideContents = new FrameworkElement[InternalChildren.Count];
            innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                OnVisualChildAdded(i);
            }
        }

        private void OnVisualChildAdded(int i)
        {
            innerGrid.RowDefinitions.Add(new RowDefinition());
            FrameworkElement sideChild = AddChild(InternalChildren[i], i);
            sideContents[i] = sideChild;
        }

        protected override Visual GetVisualChild(int index)
        {
            return ((LayoutProxy)innerGrid.Children[index]).Element;
        }

        FrameworkElement AddChild(UIElement elm, int i)
        {
            var proxy = new LayoutProxy(elm);
            Grid.SetColumnSpan(proxy, Grid.GetColumnSpan(elm));
            Grid.SetRow(proxy, i);
            Grid.SetColumn(proxy, 0);
            var sideContentPresenter = new ContentControl { ContentTemplate = SideContentTemplate };
            var binding = new Binding("DataContext") { Source = elm };
            sideContentPresenter.SetBinding(ContentControl.ContentProperty, binding);
            var sideContentWrapper = new LayoutProxy(sideContentPresenter);
            Grid.SetRow(sideContentWrapper, i);
            Grid.SetColumn(sideContentWrapper, 1);
            this.AddVisualChild(sideContentPresenter);
            innerGrid.Children.Add(proxy);
            innerGrid.Children.Add(sideContentWrapper);
            return sideContentPresenter;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            innerGrid.Measure(availableSize);
            this.measuredOnce = true;
            return innerGrid.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            innerGrid.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                // Otherwise UIElements from this.Children get instantiated, which really confuses WPF
                if (!measuredOnce)
                    return 0;
                return innerGrid.Children.Count;
            }
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (measuredOnce && !ignoreVisualChange)
            {
                ignoreVisualChange = true;
                // This could be smarter but nobody is going to have more then a dozen of target outputs
                InitializeProxyLayout();
                ignoreVisualChange = false;
            }
        }
    }

}
