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
using YDock.Model.Element;
using YDock.Model.Layout;
using YDock.View.Control;
using YDock.View.Layout;

namespace YDock.View.Window
{
    public abstract class BaseFloatWindow : System.Windows.Window,
                                            ILayoutViewParent
    {
        protected DockManager _dockManager;

        protected double _heightEceeed;
        protected HwndSource _hwndSrc;
        protected HwndSourceHook _hwndSrcHook;

        protected bool _isDragging;

        protected bool _needReCreate;

        protected double _widthEceeed;

        #region Constructors

        protected BaseFloatWindow(DockManager dockManager, bool needReCreate = false)
        {
            _dockManager = dockManager;
            MinWidth = 150;
            MinHeight = 60;
            _widthEceeed = 0;
            _heightEceeed = 0;
            NeedReCreate = needReCreate;
            //AllowsTransparency = true;
            //WindowStyle = WindowStyle.None;
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
            get { return _hwndSrc.Handle; }
        }

        internal double HeightEceeed
        {
            get { return _heightEceeed; }
        }

        internal bool NeedReCreate
        {
            get { return _needReCreate; }
            set { _needReCreate = value; }
        }

        internal double WidthEceeed
        {
            get { return _widthEceeed; }
        }

        #endregion

        #region Override members

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (Mouse.LeftButton == MouseButtonState.Pressed && (DockManager.DragManager._dragWnd == null || DockManager.DragManager._dragWnd == this))
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
            if (child is IDisposable)
            {
                child.Dispose();
            }
        }

        #endregion

        #region ILayoutViewParent Members

        public virtual void DetachChild(IDockView child, bool force = true)
        {
            if (child == Content)
            {
                DockManager.RemoveFloatWindow(this);
                SaveSize();
                if (child is BaseGroupControl)
                {
                    (child as BaseGroupControl).IsDraggingFromDock = false;
                }

                Content = null;
                if (force)
                {
                    _dockManager = null;
                }
            }
        }

        public virtual void AttachChild(IDockView child, AttachMode mode, int index)
        {
            if (Content != child)
            {
                Content = child;
                DockManager.AddFloatWindow(this);
                Height = (child as ILayoutSize).DesiredHeight + _heightEceeed;
                Width = (child as ILayoutSize).DesiredWidth + _widthEceeed;
            }
        }

        public int IndexOf(IDockView child)
        {
            if (child == Child)
            {
                return 0;
            }

            return -1;
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

            _hwndSrc = PresentationSource.FromDependencyObject(this) as HwndSource;
            _hwndSrcHook = FilterMessage;
            _hwndSrc.AddHook(_hwndSrcHook);
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

            if (_hwndSrc != null)
            {
                _hwndSrc.RemoveHook(_hwndSrcHook);
                _hwndSrc.Dispose();
                _hwndSrc = null;
            }
        }

        #endregion

        #region Members

        public XElement GenerateLayout()
        {
            var element = new XElement("FloatWindow");
            if (Child is BaseGroupControl)
            {
                element.Add((Child as BaseGroupControl).GenerateLayout());
            }
            else if (Child is LayoutGroupPanel)
            {
                element.Add((Child as LayoutGroupPanel).GenerateLayout());
            }

            return element;
        }

        public void HitTest(Point p)
        {
            var p1 = (Content as FrameworkElement).PointToScreenDPIWithoutFlowDirection(new Point());
            VisualTreeHelper.HitTest(Content as FrameworkElement, _HitFilter, _HitRessult, new PointHitTestParameters(new Point(p.X - p1.X, p.Y - p1.Y)));
        }

        public virtual void Recreate()
        {
        }

        public void SaveSize()
        {
            //保存Size信息
            if (Content is ILayoutSize)
            {
                var _child = Content as ILayoutSize;
                _child.DesiredWidth = Math.Max(ActualWidth - _widthEceeed, Constants.SideLength);
                _child.DesiredHeight = Math.Max(ActualHeight - _heightEceeed, Constants.SideLength);
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
                case Win32Helper.WM_EXITSIZEMOVE:
                    if (DockManager.DragManager.IsDragging)
                    {
                        DockManager.DragManager.DoDragDrop();
                        _isDragging = false;
                    }

                    _UpdateLocation(Child);
                    break;
            }

            return IntPtr.Zero;
        }

        private HitTestFilterBehavior _HitFilter(DependencyObject potentialHitTestTarget)
        {
            if (potentialHitTestTarget is BaseGroupControl)
            {
                //设置DragTarget，以实时显示TargetWnd
                DockManager.DragManager.DragTarget = potentialHitTestTarget as IDragTarget;
                return HitTestFilterBehavior.Stop;
            }

            return HitTestFilterBehavior.Continue;
        }

        private HitTestResultBehavior _HitRessult(HitTestResult result)
        {
            DockManager.DragManager.DragTarget = null;
            return HitTestResultBehavior.Stop;
        }

        private void _UpdateLocation(object obj)
        {
            if (obj != null)
            {
                if (obj is LayoutGroupPanel)
                {
                    foreach (var child in (obj as LayoutGroupPanel).Children)
                    {
                        _UpdateLocation(child as IDockView);
                    }
                }

                if (obj is BaseGroupControl)
                {
                    var size = obj as ILayoutSize;
                    size.FloatLeft = Left;
                    size.FloatTop = Top;
                }

                if (obj is BaseLayoutGroup)
                {
                    foreach (DockElement item in (obj as BaseLayoutGroup).Children)
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