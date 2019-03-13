using System.Windows;
using System.Windows.Controls;
using YDock.Interface;

namespace YDock.View
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
            var ele = DataContext as IDockElement;
            if (ele.Container is ILayoutGroup)
            {
                ele.Container.ShowWithActive(ele);
            }
        }

        #endregion
    }
}