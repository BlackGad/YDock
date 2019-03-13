using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using YDock.Commands;
using YDock.Enum;
using YDock.Interface;
using YDock.Model;

namespace YDock.View
{
    public class AnchorHeaderControl : Control,
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
                    var ele = DataContext as IDockElement;
                    if (!ele.DockManager.DragManager.IsDragging)
                    {
                        if (ele.Mode == DockMode.DockBar)
                        {
                            ele.DockManager.DragManager.IntoDragAction(new DragItem(ele,
                                                                                    ele.Mode,
                                                                                    DragMode.Anchor,
                                                                                    _mouseDown,
                                                                                    Rect.Empty,
                                                                                    new Size(ele.DesiredWidth, ele.DesiredHeight)));
                        }
                        else
                        {
                            ele.DockManager.DragManager.IntoDragAction(new DragItem(ele.Container,
                                                                                    ele.Mode,
                                                                                    DragMode.Anchor,
                                                                                    _mouseDown,
                                                                                    Rect.Empty,
                                                                                    new Size(ele.DesiredWidth, ele.DesiredHeight)));
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
            var ele = DataContext as DockElement;
            if (e.Command == GlobalCommands.HideStatusCommand)
            {
                ele?.Hide();
            }

            if (e.Command == GlobalCommands.SwitchAutoHideStatusCommand)
            {
                ele?.SwitchAutoHideStatus();
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
            var ele = DataContext as DockElement;
            menu = new DockMenu(ele);
            menu.PlacementTarget = ctb;
            menu.Placement = PlacementMode.Bottom;
            ctb.ContextMenu = menu;
        }

        #endregion
    }
}