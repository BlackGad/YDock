using System.Windows;
using System.Windows.Controls;
using YDock.Interface;

namespace YDock.View.Control
{
    public class CustomMenuItem : MenuItem
    {
        #region Constructors

        static CustomMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomMenuItem), new FrameworkPropertyMetadata(typeof(CustomMenuItem)));
        }

        #endregion

        #region Override members

        protected override void OnClick()
        {
            base.OnClick();

            if (DataContext is IDockElement element && element.Container is ILayoutGroup group)
            {
                group.ShowWithActive(element);
            }
        }

        #endregion
    }
}