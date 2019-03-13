using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace YDock.View
{
    public class SingelLinePanel : Panel
    {
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

            if (visibleChildren.Any())
                //不计算最后一个元素的右边距
            {
                width -= visibleChildren.Last().Margin.Right;
            }

            return new Size(Math.Min(width, availableSize.Width), height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var visibleChildren = InternalChildren.Cast<FrameworkElement>().Where(element => element.Visibility != Visibility.Collapsed);

            var offset = 0.0;
            foreach (var child in visibleChildren)
            {
                child.Arrange(new Rect(new Point(offset + child.Margin.Left, 0), child.DesiredSize));
                offset += child.DesiredSize.Width + child.Margin.Left + child.Margin.Right;
            }

            return finalSize;
        }

        #endregion
    }
}