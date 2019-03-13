using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace YDock.View
{
    public class CustomContextMenu : ContextMenu
    {
        #region Constructors

        static CustomContextMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomContextMenu), new FrameworkPropertyMetadata(typeof(CustomContextMenu)));
        }

        #endregion

        #region Override members

        protected override void OnOpened(RoutedEventArgs e)
        {
            BindingOperations.GetBindingExpression(this, ItemsSourceProperty).UpdateTarget();

            base.OnOpened(e);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CustomMenuItem();
        }

        #endregion
    }
}