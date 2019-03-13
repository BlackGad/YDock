using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using YDock.View;
using YDock.View.Control;
using YDock.View.Window;

namespace YDock
{
    public class DockHelper
    {
        #region Static members

        public static void ComputeSpliterLocation(Popup spliter, Point location, Size size)
        {
            if (location.X < 0)
            {
                spliter.HorizontalOffset = 0;
                spliter.Width = size.Width + location.X;
            }

            if (location.Y < 0)
            {
                spliter.VerticalOffset = 0;
                spliter.Height = size.Height + location.Y;
            }

            if (location.X + size.Width > SystemParameters.PrimaryScreenWidth)
            {
                spliter.Width = SystemParameters.PrimaryScreenWidth - location.X;
            }

            if (location.Y + size.Height > SystemParameters.PrimaryScreenHeight)
            {
                spliter.Height = SystemParameters.PrimaryScreenHeight - location.Y;
            }
        }

        public static Rect CreateChildRectFromParent(FrameworkElement parent, FrameworkElement child)
        {
            var originP = parent.PointToScreenDPIWithoutFlowDirection(new Point());
            var childP = child.PointToScreenDPIWithoutFlowDirection(new Point());
            return new Rect(new Point(childP.X - originP.X, childP.Y - originP.Y), child.TransformActualSizeToAncestor());
        }

        public static Point GetMousePosition(FrameworkElement relativeTo)
        {
            return relativeTo.TransformToDeviceDPI(Win32Helper.GetMousePosition());
        }

        public static Point GetMousePositionRelativeTo(FrameworkElement relativeTo)
        {
            var mouseP = relativeTo.TransformToDeviceDPI(Win32Helper.GetMousePosition());
            var pToScreen = relativeTo.PointToScreenDPIWithoutFlowDirection(new Point());
            return new Point(mouseP.X - pToScreen.X, mouseP.Y - pToScreen.Y);
        }

        public static T GetTemplateChild<T>(FrameworkTemplate template, FrameworkElement templateParent, string name)
        {
            return (T)template.FindName(name, templateParent);
        }

        public static double GetTextWidth(string text)
        {
            return new FormattedText(text,
                                     CultureInfo.CurrentUICulture,
                                     FlowDirection.LeftToRight,
                                     new Typeface(new FontFamily("微软 雅黑"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                                     12,
                                     Brushes.Black).Width;
        }

        public static void UpdateLocation(DropWindow wnd, double left, double top, double width, double height)
        {
            double hrectoffset = 0, vrectoffset = 0;
            if (wnd.Host is LayoutDocumentGroupControl)
            {
                var dcrt = wnd.Host as LayoutDocumentGroupControl;
                var index = dcrt.IndexOf();
                wnd.Width = width;
                wnd.Height = height;
                if (wnd.DropPanel.InnerRect.Width < wnd.MinWidth)
                {
                    hrectoffset = (wnd.MinWidth - wnd.DropPanel.InnerRect.Width) / 2;
                    if (index == 0)
                    {
                        wnd.Width += hrectoffset;
                        left -= hrectoffset;
                    }

                    if (index == dcrt.ParentChildrenCount - 1)
                    {
                        wnd.Width += hrectoffset;
                    }
                }

                if (wnd.DropPanel.InnerRect.Height < wnd.MinHeight)
                {
                    vrectoffset = (wnd.MinHeight - wnd.DropPanel.InnerRect.Height) / 2;
                    if (index == 0)
                    {
                        wnd.Height += vrectoffset;
                        top -= vrectoffset;
                    }

                    if (index == dcrt.ParentChildrenCount - 1)
                    {
                        wnd.Height += vrectoffset;
                    }
                }

                if (left < 0)
                {
                    wnd.HorizontalOffset = 0;
                    hrectoffset += left;
                }
                else if (left + wnd.Width > SystemParameters.PrimaryScreenWidth)
                {
                    wnd.HorizontalOffset = SystemParameters.PrimaryScreenWidth - wnd.Width;
                    hrectoffset += left + wnd.Width - SystemParameters.PrimaryScreenWidth;
                }
                else
                {
                    wnd.HorizontalOffset = left;
                }

                if (top < 0)
                {
                    wnd.VerticalOffset = 0;
                    vrectoffset += top;
                }
                else if (top + wnd.Height > SystemParameters.PrimaryScreenHeight)
                {
                    wnd.VerticalOffset = SystemParameters.PrimaryScreenHeight - wnd.Height;
                    vrectoffset += top + wnd.Height - SystemParameters.PrimaryScreenHeight;
                }
                else
                {
                    wnd.VerticalOffset = top;
                }

                wnd.DropPanel.OuterRect = new Rect(hrectoffset, vrectoffset, width, height);
            }
            else
            {
                wnd.Width = width;
                wnd.Height = height;
                if (wnd.MinWidth > width)
                {
                    hrectoffset = (wnd.MinWidth - width) / 2;
                    left -= hrectoffset;
                    width = wnd.MinWidth;
                }

                if (wnd.MinHeight > height)
                {
                    vrectoffset = (wnd.MinHeight - height) / 2;
                    top -= vrectoffset;
                    height = wnd.MinHeight;
                }

                if (left < 0)
                {
                    wnd.HorizontalOffset = 0;
                    hrectoffset += left;
                }
                else if (left + width > SystemParameters.PrimaryScreenWidth)
                {
                    wnd.HorizontalOffset = SystemParameters.PrimaryScreenWidth - width;
                    hrectoffset += left + width - SystemParameters.PrimaryScreenWidth;
                }
                else
                {
                    wnd.HorizontalOffset = left;
                }

                if (top < 0)
                {
                    wnd.VerticalOffset = 0;
                    vrectoffset += top;
                }
                else if (top + height > SystemParameters.PrimaryScreenHeight)
                {
                    wnd.VerticalOffset = SystemParameters.PrimaryScreenHeight - height;
                    vrectoffset += top + height - SystemParameters.PrimaryScreenHeight;
                }
                else
                {
                    wnd.VerticalOffset = top;
                }

                wnd.DropPanel.InnerRect = new Rect(hrectoffset, vrectoffset, wnd.Width, wnd.Height);
                wnd.DropPanel.OuterRect = new Rect(0, 0, wnd.Width, wnd.Height);
            }
        }

        #endregion
    }
}