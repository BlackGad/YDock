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
                                     new Typeface(new FontFamily("Segoe ui"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                                     12,
                                     Brushes.Black).Width;
        }

        public static void UpdateLocation(DropWindow wnd, double left, double top, double width, double height)
        {
            double hRectOffset = 0, vRectOffset = 0;
            if (wnd.Host is LayoutDocumentGroupControl groupControl)
            {
                var index = groupControl.IndexOf();
                wnd.Width = width;
                wnd.Height = height;
                if (wnd.DropPanel.InnerRect.Width < wnd.MinWidth)
                {
                    hRectOffset = (wnd.MinWidth - wnd.DropPanel.InnerRect.Width) / 2;
                    if (index == 0)
                    {
                        wnd.Width += hRectOffset;
                        left -= hRectOffset;
                    }

                    if (index == groupControl.ParentChildrenCount - 1)
                    {
                        wnd.Width += hRectOffset;
                    }
                }

                if (wnd.DropPanel.InnerRect.Height < wnd.MinHeight)
                {
                    vRectOffset = (wnd.MinHeight - wnd.DropPanel.InnerRect.Height) / 2;
                    if (index == 0)
                    {
                        wnd.Height += vRectOffset;
                        top -= vRectOffset;
                    }

                    if (index == groupControl.ParentChildrenCount - 1)
                    {
                        wnd.Height += vRectOffset;
                    }
                }

                if (left < 0)
                {
                    wnd.HorizontalOffset = 0;
                    hRectOffset += left;
                }
                else if (left + wnd.Width > SystemParameters.PrimaryScreenWidth)
                {
                    wnd.HorizontalOffset = SystemParameters.PrimaryScreenWidth - wnd.Width;
                    hRectOffset += left + wnd.Width - SystemParameters.PrimaryScreenWidth;
                }
                else
                {
                    wnd.HorizontalOffset = left;
                }

                if (top < 0)
                {
                    wnd.VerticalOffset = 0;
                    vRectOffset += top;
                }
                else if (top + wnd.Height > SystemParameters.PrimaryScreenHeight)
                {
                    wnd.VerticalOffset = SystemParameters.PrimaryScreenHeight - wnd.Height;
                    vRectOffset += top + wnd.Height - SystemParameters.PrimaryScreenHeight;
                }
                else
                {
                    wnd.VerticalOffset = top;
                }

                wnd.DropPanel.OuterRect = new Rect(hRectOffset, vRectOffset, width, height);
            }
            else
            {
                wnd.Width = width;
                wnd.Height = height;
                if (wnd.MinWidth > width)
                {
                    hRectOffset = (wnd.MinWidth - width) / 2;
                    left -= hRectOffset;
                    width = wnd.MinWidth;
                }

                if (wnd.MinHeight > height)
                {
                    vRectOffset = (wnd.MinHeight - height) / 2;
                    top -= vRectOffset;
                    height = wnd.MinHeight;
                }

                if (left < 0)
                {
                    wnd.HorizontalOffset = 0;
                    hRectOffset += left;
                }
                else if (left + width > SystemParameters.PrimaryScreenWidth)
                {
                    wnd.HorizontalOffset = SystemParameters.PrimaryScreenWidth - width;
                    hRectOffset += left + width - SystemParameters.PrimaryScreenWidth;
                }
                else
                {
                    wnd.HorizontalOffset = left;
                }

                if (top < 0)
                {
                    wnd.VerticalOffset = 0;
                    vRectOffset += top;
                }
                else if (top + height > SystemParameters.PrimaryScreenHeight)
                {
                    wnd.VerticalOffset = SystemParameters.PrimaryScreenHeight - height;
                    vRectOffset += top + height - SystemParameters.PrimaryScreenHeight;
                }
                else
                {
                    wnd.VerticalOffset = top;
                }

                wnd.DropPanel.InnerRect = new Rect(hRectOffset, vRectOffset, wnd.Width, wnd.Height);
                wnd.DropPanel.OuterRect = new Rect(0, 0, wnd.Width, wnd.Height);
            }
        }

        #endregion
    }
}