using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace YDock.View.Layout
{
    public class AnchorSidePanel : Panel
    {
        ///<summary>This flag indicates whether it is necessary to compensate for the width of the child element when Arrange</summary>
        private bool _needCompensate;

        #region Override members

        /// <summary>
        ///     In the measurement phase, if the total width of the child elements exceeds the available width, the width of the
        ///     child elements is sorted, and the excess width is sequentially cut.
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            var visibleChildren = InternalChildren.Cast<FrameworkElement>().Where(element => element.Visibility != Visibility.Collapsed).ToList();

            var height = 0.0;
            var width = 0.0;
            foreach (var child in visibleChildren)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                width += child.DesiredSize.Width + child.Margin.Left + child.Margin.Right;
                height = Math.Max(height, child.DesiredSize.Height);
            }

            //More than available width
            if (width > availableSize.Width)
            {
                _needCompensate = true;
                //Exceeded part
                var exceed = width - availableSize.Width;
                //Sort elements by width from large to small, and extra lengths are cropped from the longest element
                visibleChildren.Sort(new ElementComparer<FrameworkElement>((a, b) =>
                {
                    if (Math.Abs(a.DesiredSize.Width - b.DesiredSize.Width) < double.Epsilon) return 0;
                    if (a.DesiredSize.Width > b.DesiredSize.Width) return -1;

                    return 1;
                }));

                //The number of elements whose current width is the maximum width
                var currentCnt = 0;
                //Current maximum width
                var currentWidth = visibleChildren[0].DesiredSize.Width;
                //If there are multiple elements in the maximum width, they will be cropped together.
                foreach (var child in visibleChildren)
                {
                    if (Math.Abs(child.DesiredSize.Width - currentWidth) < double.Epsilon)
                    {
                        currentCnt++;
                    }
                    else
                    {
                        break;
                    }
                }

                while (exceed > 0)
                {
                    //Indicates that all child elements are all the same width after clipping.
                    if (currentCnt == visibleChildren.Count)
                    {
                        if (currentCnt * currentWidth >= exceed)
                        {
                            currentWidth -= exceed / currentCnt;
                        }
                        else
                        {
                            currentWidth = 0;
                        }

                        exceed = 0;
                    }
                    else
                    {
                        //Obtain the width of the second wide element as the target value of the current crop
                        var nextLen = visibleChildren[currentCnt].DesiredSize.Width;
                        if ((currentWidth - nextLen) * currentCnt >= exceed)
                        {
                            currentWidth -= exceed / currentCnt;
                            exceed = 0;
                        }
                        else
                        {
                            exceed -= (currentWidth - nextLen) * currentCnt;
                            for (var i = currentCnt; i < visibleChildren.Count; i++)
                            {
                                if (Math.Abs(visibleChildren[currentCnt].DesiredSize.Width - nextLen) < double.Epsilon)
                                {
                                    currentCnt++;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            currentWidth = nextLen;
                        }
                    }
                }

                //After cropping, re-measure the elements with the changed width
                for (var i = 0; i < currentCnt; i++)
                {
                    visibleChildren[i].Measure(new Size(currentWidth, height));
                }
            }

            return new Size(Math.Min(width, availableSize.Width), height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var visibleChildren = InternalChildren.OfType<FrameworkElement>()
                                                  .Where(element => element.Visibility != Visibility.Collapsed)
                                                  .ToList();

            var wholeLength = visibleChildren.Sum(a => a.DesiredSize.Width);
            double delta = 0;

            //Due to TextBlock's TextTrimming="CharacterEllipsis",
            //since the character clipping will cause the DesiredSize Width to become smaller than the actual size,
            //compensate here.
            if (wholeLength < finalSize.Width && _needCompensate)
            {
                _needCompensate = false;
                delta = (finalSize.Width - wholeLength) / visibleChildren.Count;
            }

            var offset = 0.0;
            foreach (var child in visibleChildren)
            {
                child.Arrange(new Rect(new Point(offset, 0), new Size(child.DesiredSize.Width + delta, finalSize.Height)));
                offset += child.DesiredSize.Width + delta;
            }

            return finalSize;
        }

        #endregion
    }
}