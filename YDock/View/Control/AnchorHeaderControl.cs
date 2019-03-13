using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using YDock.Enum;
using YDock.Global.Commands;
using YDock.Interface;
using YDock.Model.Element;
using YDock.View.Meun;

namespace YDock.View.Control
{
    public class AnchorHeaderControl : System.Windows.Controls.Control,
                                       IDisposable
    {
        private Point _mouseDown;

        private ToggleButton ctb;
        private DockMenu menu;

        #region Constructors

        static AnchorHeaderControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnchorHeaderControl), new FrameworkPropertyMetadata(default(AnchorHeaderControl)));
        }

        #endregion

        #region Override members

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _mouseDown = e.GetPosition(this);
            if (!IsMouseCaptured)
            {
                CaptureMouse();
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            ContextMenu = new DockMenu(DataContext as DockElement);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {
                if ((e.GetPosition(this) - _mouseDown).Length > Math.Max(SystemParameters.MinimumHorizontalDragDistance, SystemParameters.MinimumVerticalDragDistance))
                {
                    ReleaseMouseCapture();
                    var element = DataContext as IDockElement;
                    if (!element.DockManager.DragManager.IsDragging)
                    {
                        if (element.Mode == DockMode.DockBar)
                        {
                            element.DockManager.DragManager.IntoDragAction(new DragItem(element,
                                                                                    element.Mode,
                                                                                    DragMode.Anchor,
                                                                                    _mouseDown,
                                                                                    Rect.Empty,
                                                                                    new Size(element.DesiredWidth, element.DesiredHeight)));
                        }
                        else
                        {
                            element.DockManager.DragManager.IntoDragAction(new DragItem(element.Container,
                                                                                    element.Mode,
                                                                                    DragMode.Anchor,
                                                                                    _mouseDown,
                                                                                    Rect.Empty,
                                                                                    new Size(element.DesiredWidth, element.DesiredHeight)));
                        }
                    }
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == "DataContext")
            {
                if (menu != null)
                {
                    menu.Dispose();
                    menu = null;
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ctb = (ToggleButton)GetTemplateChild("PART_DropMenu");
            ctb.PreviewMouseLeftButtonUp += OnMenuOpen;
        }

        protected override void OnInitialized(EventArgs e)
        {
            CommandBindings.Add(new CommandBinding(GlobalCommands.SwitchAutoHideStatusCommand, OnCommandExecute, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(GlobalCommands.HideStatusCommand, OnCommandExecute, OnCommandCanExecute));
            base.OnInitialized(e);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            DataContext = null;
            ContextMenu = null;
            ctb.PreviewMouseLeftButtonUp -= OnMenuOpen;
        }

        #endregion

        #region Event handlers

        private void OnCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OnCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var element = DataContext as DockElement;
            if (e.Command == GlobalCommands.HideStatusCommand)
            {
                element?.Hide();
            }

            if (e.Command == GlobalCommands.SwitchAutoHideStatusCommand)
            {
                element?.SwitchAutoHideStatus();
            }
        }

        private void OnMenuOpen(object sender, MouseButtonEventArgs e)
        {
            if (menu == null)
            {
                _ApplyMenu();
            }

            menu.IsOpen = true;
        }

        #endregion

        #region Members

        private void _ApplyMenu()
        {
            var element = DataContext as DockElement;
            menu = new DockMenu(element);
            menu.PlacementTarget = ctb;
            menu.Placement = PlacementMode.Bottom;
            ctb.ContextMenu = menu;
        }

        #endregion
    }
}