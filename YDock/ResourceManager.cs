using System.Collections.Generic;
using System.Windows.Media;

namespace YDock
{
    public static class ResourceManager
    {
        #region Constants

        public static readonly Pen ActiveDashPen;
        public static readonly Pen BlueDashPen;
        public static readonly Pen BorderPen;
        public static readonly Pen DisActiveDashPen;
        public static readonly SolidColorBrush DropRectBrush;
        public static readonly Pen DropRectPen;
        public static readonly Pen DropRectPenHeavy;
        public static readonly Pen RectBorderPen;
        public static readonly SolidColorBrush RectBrush;
        public static readonly SolidColorBrush SplitterBrushHorizontal;
        public static readonly SolidColorBrush SplitterBrushVertical;
        public static readonly SolidColorBrush WindowBorderBrush;

        #endregion

        #region Constructors

        static ResourceManager()
        {
            SplitterBrushVertical = new SolidColorBrush(new Color
            {
                R = 0xEE,
                G = 0xEE,
                B = 0xF2,
                A = 0xFF
            });

            SplitterBrushHorizontal = new SolidColorBrush(new Color
            {
                R = 0xCC,
                G = 0xCE,
                B = 0xDB,
                A = 0xFF
            });

            DropRectBrush = new SolidColorBrush(new Color
            {
                R = 0x00,
                G = 0x7A,
                B = 0xCC,
                A = 0xFF
            });

            RectBrush = new SolidColorBrush(new Color
            {
                R = 0x1D,
                G = 0x7A,
                B = 0xEE,
                A = 0xFF
            });

            WindowBorderBrush = new SolidColorBrush(new Color
            {
                R = 0x9B,
                G = 0x9F,
                B = 0xB9,
                A = 0xFF
            });

            BorderPen = new Pen
            {
                Brush = Brushes.Gray,
                Thickness = 1
            };

            RectBorderPen = new Pen
            {
                Brush = Brushes.LightGray,
                Thickness = 6
            };

            DropRectPenHeavy = new Pen
            {
                Brush = DropRectBrush,
                Thickness = 3
            };

            DropRectPen = new Pen
            {
                Brush = DropRectBrush,
                Thickness = 1
            };

            ActiveDashPen = new Pen
            {
                Brush = Brushes.White,
                Thickness = 0.8,
                DashCap = PenLineCap.Flat,
                DashStyle = new DashStyle(new List<double>
                                              { 1, 4 },
                                          0)
            };

            DisActiveDashPen = new Pen
            {
                Brush = Brushes.Black,
                Thickness = 0.8,
                DashCap = PenLineCap.Flat,
                DashStyle = new DashStyle(new List<double>
                                              { 1, 4 },
                                          0)
            };

            BlueDashPen = new Pen
            {
                Brush = Brushes.Blue,
                Thickness = 0.8,
                DashCap = PenLineCap.Flat,
                DashStyle = new DashStyle(new List<double>
                                              { 1, 3 },
                                          0)
            };
        }

        #endregion
    }
}