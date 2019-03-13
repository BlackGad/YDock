using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace YDock.View.Control
{
    public class CustomToggleButton : ToggleButton,
                                      IDisposable
    {
        #region Property definitions

        public static readonly DependencyProperty DropContextMenuProperty =
            DependencyProperty.Register("DropContextMenu", typeof(CustomContextMenu), typeof(CustomToggleButton));

        #endregion

        #region Constructors

        static CustomToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomToggleButton), new FrameworkPropertyMetadata(typeof(CustomToggleButton)));
        }

        #endregion

        #region Properties

        public CustomContextMenu DropContextMenu
        {
            get { return (CustomContextMenu)GetValue(DropContextMenuProperty); }
            set { SetValue(DropContextMenuProperty, value); }
        }

        #endregion

        #region Override members

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (DropContextMenu != null)
            {
                DropContextMenu.PlacementTarget = this;
                DropContextMenu.Placement = PlacementMode.Bottom;
                DropContextMenu.IsOpen = true;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            DataContext = null;
            DropContextMenu = null;
        }

        #endregion
    }
}