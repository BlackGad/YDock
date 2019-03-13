using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using YDock.Interface;

namespace YDock.View
{
    public class DocumentPanel : Panel
    {
        #region Constructors

        public DocumentPanel()
        {
            FlowDirection = FlowDirection.LeftToRight;
        }

        #endregion

        #region Override members

        protected override Size MeasureOverride(Size availableSize)
        {
            var visibleChildren = InternalChildren.Cast<FrameworkElement>().Where(element => element.Visibility != Visibility.Collapsed);

            var height = 0.0;
            var width = 0.0;
            foreach (var child in visibleChildren)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                width += child.DesiredSize.Width + child.Margin.Left + child.Margin.Right;
                height = Math.Max(height, child.DesiredSize.Height);
            }

            return new Size(Math.Min(width, availableSize.Width), height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var visibleChildren = InternalChildren.Cast<TabItem>().Where(element => element.Visibility != Visibility.Collapsed).ToList();

            var width = 0.0;
            var index = 0;
            for (; index < visibleChildren.Count; index++)
            {
                var element = visibleChildren[index];
                element.Arrange(new Rect(new Point(width, 0), element.DesiredSize));
                width += element.DesiredSize.Width + element.Margin.Left + element.Margin.Right;
                if (width > finalSize.Width)
                {
                    element.Visibility = Visibility.Hidden;
                    if (element.IsSelected)
                    {
                        element.Visibility = Visibility.Visible;
                        break;
                    }
                }
                else
                {
                    element.Visibility = Visibility.Visible;
                }
            }

            if (index > 0 && index < visibleChildren.Count && visibleChildren[index].IsSelected)
            {
                var selecteditem = visibleChildren[index];
                var startindex = index - 1;
                for (; startindex >= 0; startindex--)
                {
                    var item = visibleChildren[startindex];
                    width -= item.DesiredSize.Width + item.Margin.Left + item.Margin.Right;
                    if (width <= finalSize.Width || startindex == 0) break;
                }

                width -= selecteditem.DesiredSize.Width + selecteditem.Margin.Left + selecteditem.Margin.Right;

                var element = selecteditem.Content as IDockElement;
                var tab = element.Container;
                tab.MoveTo(index, startindex);

                for (; startindex < visibleChildren.Count; startindex++)
                {
                    var item = visibleChildren[startindex];
                    item.Arrange(new Rect(new Point(width, 0), item.DesiredSize));
                    width += item.DesiredSize.Width + item.Margin.Left + item.Margin.Right;
                    if (width > finalSize.Width)
                    {
                        item.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
            }

            return finalSize;
        }

        #endregion
    }
}