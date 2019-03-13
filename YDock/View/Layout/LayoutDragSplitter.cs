using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace YDock.View.Layout
{
    public class LayoutDragSplitter : Thumb
    {
        #region Constructors

        static LayoutDragSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutDragSplitter), new FrameworkPropertyMetadata(typeof(LayoutDragSplitter)));
            BackgroundProperty.OverrideMetadata(typeof(LayoutDragSplitter), new FrameworkPropertyMetadata(Brushes.Transparent));
            IsHitTestVisibleProperty.OverrideMetadata(typeof(LayoutDragSplitter), new FrameworkPropertyMetadata(true, null));
        }

        #endregion
    }
}