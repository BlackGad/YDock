using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml.Linq;
using YDock.Enum;
using YDock.Global.Commands;
using YDock.Interface;
using YDock.Model.Layout;
using YDock.View.Control;
using YDock.View.Layout;
// ReSharper disable RedundantAssignment

namespace YDock.View.Window
{
    public abstract class BaseFloatWindow : System.Windows.Window,
                                            ILayoutViewParent
    {
        protected DockManager _dockManager;

        protected double _heightExceed;
        protected HwndSource _hwndSource;
        protected HwndSourceHook _hwndSourceHook;

        protected bool _isDragging;

        protected bool _needReCreate;

        protected double _widthExceed;

        #region Constructors

        protected BaseFloatWindow(DockManager dockManager, bool needReCreate = false)
        {
            _dockManager = dockManager;
            _widthExceed = 0;
            _heightExceed = 0;

            MinWidth = 150;
            MinHeight = 60;
            NeedReCreate = needReCreate;
            ShowActivated = true;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #endregion

        #region Properties

        public virtual DockManager DockManager
        {
            get { return _dockManager; }
            internal set { _dockManager = value; }
        }

        public bool IsDragging
        {
            get { return _isDragging; }
        }

        internal virtual ILayoutViewWithSize Child
        {
            get { return Content == null ? null : Content as ILayoutViewWithSize; }
        }

        internal IntPtr Handle
        {
            get { return _hwndSource.Handle; }
        }

        internal double HeightExceed
        {
            get { return _heightExceed; }
        }

        internal bool NeedReCreate
        {
            get { return _needReCreate; }
            set { _needReCreate = value; }
        }

        internal double WidthExceed
        {
            get { return _widthExceed; }
        }

        #endregion

        #region Override members

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (Mouse.LeftButton == MouseButtonState.Pressed && (DockManager.DragManager._dragWindow == null || DockManager.DragManager._dragWindow == this))
            {
                var windowHandle = new WindowInteropHelper(this).Handle;
                var mousePosition = this.PointToScreenDPI(Mouse.GetPosition(this));
                var lParam = new IntPtr(((int)mousePosition.X & 0xFFFF) | ((int)mousePosition.Y << 16));

                Win32Helper.SendMessage(windowHandle, Win32Helper.WM_NCLBUTTONDOWN, new IntPtr(Win32Helper.HT_CAPTION), lParam);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            CommandBindings.Add(new CommandBinding(GlobalCommands.CloseCommand, OnCloseExecute, OnCloseCanExecute));
            CommandBindings.Add(new CommandBinding(GlobalCommands.RestoreCommand, OnRestoreExecute, OnRestoreCanExecute));
            CommandBindings.Add(new CommandBinding(GlobalCommands.MaximizeCommand, OnMaximizeExecute, OnMaximizeCanExecute));
            base.OnInitialized(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_dockManager == null) return;
            var child = Child;
            DetachChild(Child);
            child?.Dispose();
        }

        #endregion

        #region ILayoutViewParent Members

        public virtual void DetachChild(IDockView child, bool force = true)
        {
            if (child != Content) return;

            DockManager.RemoveFloatWindow(this);
            SaveSize();
            if (child is BaseGroupControl control)
            {
                control.IsDraggingFromDock = false;
            }

            Content = null;
            if (force) _dockManager = null;
        }

        public virtual void AttachChild(IDockView child, AttachMode mode, int index)
        {
            if (Content != child)
            {
                Content = child;
                DockManager.AddFloatWindow(this);
                var layoutSize = (ILayoutSize)child;
                Height = layoutSize.DesiredHeight + _heightExceed;
                Width = layoutSize.DesiredWidth + _widthExceed;
            }
        }

        public int IndexOf(IDockView child)
        {
            return Equals(child, Child) ? 0 : -1;
        }

        #endregion

        #region Event handlers

        protected void OnCloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        protected void OnCloseExecute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            _hwndSource = PresentationSource.FromDependencyObject(this) as HwndSource;
            _hwndSourceHook = FilterMessage;
            _hwndSource?.AddHook(_hwndSourceHook);
        }

        protected virtual void OnMaximizeCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        protected void OnMaximizeExecute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        protected virtual void OnRestoreCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        protected void OnRestoreExecute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= OnUnloaded;

            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(_hwndSourceHook);
                _hwndSource.Dispose();
                _hwndSource = null;
            }
        }

        #endregion

        #region Members

        public XElement GenerateLayout()
        {
            var element = new XElement("FloatWindow");
            if (Child is BaseGroupControl control)
            {
                element.Add(control.GenerateLayout());
            }
            else if (Child is LayoutGroupPanel panel)
            {
                element.Add(panel.GenerateLayout());
            }

            return element;
        }

        public void HitTest(Point p)
        {
            var p1 = (Content as FrameworkElement).PointToScreenDPIWithoutFlowDirection(new Point());
            VisualTreeHelper.HitTest((FrameworkElement)Content, HitFilter, HitResult, new PointHitTestParameters(new Point(p.X - p1.X, p.Y - p1.Y)));
        }

        public virtual void Recreate()
        {
        }

        public void SaveSize()
        {
            //Save Size information
            if (Content is ILayoutSize child)
            {
                child.DesiredWidth = Math.Max(ActualWidth - _widthExceed, Constants.SideLength);
                child.DesiredHeight = Math.Max(ActualHeight - _heightExceed, Constants.SideLength);
            }
        }

        protected virtual IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            switch (msg)
            {
                case Win32Helper.WM_ENTERSIZEMOVE:
                    if (!DockManager.DragManager.IsDragging)
                    {
                        _isDragging = true;
                        if (this is AnchorGroupWindow)
                        {
                            DockManager.DragManager.IntoDragAction(
                                new DragItem(this, DockMode.Float, DragMode.Anchor, new Point(), Rect.Empty, new Size(ActualWidth, ActualHeight)),
                                true);
                        }
                        else
                        {
                            DockManager.DragManager.IntoDragAction(
                                new DragItem(this, DockMode.Float, DragMode.Document, new Point(), Rect.Empty, new Size(ActualWidth, ActualHeight)),
                                true);
                        }
                    }

                    break;
                case Win32Helper.WM_MOVING:
                    if (DockManager.DragManager.IsDragging)
                    {
                        DockManager.DragManager.OnMouseMove();
                    }
                    else
                    {
                        _isDragging = true;
                        var dragMode = DragMode.Document;
                        if (this is AnchorGroupWindow) dragMode = DragMode.Anchor;
                        var dragItem = new DragItem(this, DockMode.Float, dragMode, new Point(), Rect.Empty, new Size(ActualWidth, ActualHeight));
                        DockManager.DragManager.IntoDragAction(dragItem, true);
                    }

                    break;
                case Win32Helper.WM_EXITSIZEMOVE:
                    if (DockManager.DragManager.IsDragging)
                    {
                        DockManager.DragManager.DoDragDrop();
                        _isDragging = false;
                    }

                    UpdateLocation(Child);
                    break;
            }

            return IntPtr.Zero;
        }

        private HitTestFilterBehavior HitFilter(DependencyObject potentialHitTestTarget)
        {
            if (potentialHitTestTarget is BaseGroupControl target)
            {
                //Set DragTarget to display TargetWindow in real time
                DockManager.DragManager.DragTarget = target;

                return HitTestFilterBehavior.Stop;
            }

            return HitTestFilterBehavior.Continue;
        }

        private HitTestResultBehavior HitResult(HitTestResult result)
        {
            DockManager.DragManager.DragTarget = null;
            return HitTestResultBehavior.Stop;
        }

        private void UpdateLocation(object obj)
        {
            if (obj != null)
            {
                if (obj is LayoutGroupPanel panel)
                {
                    foreach (var child in panel.Children)
                    {
                        UpdateLocation(child as IDockView);
                    }
                }

                if (obj is BaseGroupControl)
                {
                    var size = (ILayoutSize)obj;
                    size.FloatLeft = Left;
                    size.FloatTop = Top;
                }

                if (obj is BaseLayoutGroup group)
                {
                    foreach (var item in group.Children)
                    {
                        item.FloatLeft = Left;
                        item.FloatTop = Top;
                    }
                }
            }
        }

        #endregion
    }
}