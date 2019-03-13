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
            var element = DataContext as IDockElement;
            if (element.Container is ILayoutGroup)
            {
                element.Container.ShowWithActive(element);
            }
        }

        #endregion
    }
}