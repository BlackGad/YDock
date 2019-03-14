using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Linq;
using YDock.Enum;
using YDock.Interface;
using YDock.Model.Element;
using YDock.Model.Layout;
using YDock.View.Layout;
using YDock.View.Render;
using YDock.View.Window;

namespace YDock.View.Control
{
    public abstract class BaseGroupControl : TabControl,
                                             ILayoutGroupControl,
                                             IDragTarget
    {
        internal IList<Rect> _childrenBounds;
        internal IDockElement _dragItem;
        internal int _index;
        internal Point _mouseDown;
        internal bool _mouseInside = true;
        internal Rect _rect;
        internal bool canUpdate = true;
        private ActiveRectDropVisual _activeRect;

        private double _desiredHeight;
        private double _desiredWidth;
        private DropWindow _dragWindow;
        private double _floatLeft;
        private double _floatTop;

        private bool _isDraggingFromDock;

        private ILayoutGroup _model;
        private Point _position;

        #region Constructors

        internal BaseGroupControl(ILayoutGroup model, double desiredWidth = Constants.DockDefaultWidthLength, double desiredHeight = Constants.DockDefaultHeightLength)
        {
            Model = model;
            SetBinding(ItemsSourceProperty, new Binding("Model.SelectableChildren") { Source = this });
            if (model.Children.Any())
            {
                DesiredWidth = model.Children.First().DesiredWidth;
                DesiredHeight = model.Children.First().DesiredHeight;
            }
            else
            {
                DesiredWidth = desiredWidth;
                DesiredHeight = desiredHeight;
            }
        }

        #endregion

        #region Properties

        public int ChildrenCount
        {
            get { return ((LayoutGroup)_model).SelectableChildren.Count; }
        }

        public bool IsDraggingFromDock
        {
            get { return _isDraggingFromDock; }
            set
            {
                if (_isDraggingFromDock != value)
                {
                    _isDraggingFromDock = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("IsDraggingFromDock"));
                }
            }
        }

        public int ParentChildrenCount
        {
            get
            {
                if (DockViewParent is LayoutGroupPanel panel) return panel.Count;
                return 1;
            }
        }

        #endregion

        #region Override members

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            var activeItem = default(DockElement);
            if (e.RemovedItems != null)
            {
                foreach (DockElement item in e.RemovedItems)
                {
                    if (item.IsActive)
                    {
                        activeItem = item;
                    }

                    item.IsVisible = false;
                }
            }

            if (e.AddedItems != null)
            {
                foreach (DockElement item in e.AddedItems)
                {
                    item.IsVisible = true;
                }
            }

            if (activeItem != null)
            {
                SelectedItem = activeItem;
            }
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (SelectedContent != null)
            {
                _model.ShowWithActive(SelectedContent as DockElement);
            }
        }

        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            if (SelectedContent != null)
            {
                _model.ShowWithActive(SelectedContent as DockElement);
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DragTabItem(this);
        }

        #endregion

        #region IDragTarget Members

        public abstract DragMode Mode { get; }

        public DockManager DockManager
        {
            get { return _model.DockManager; }
        }

        public DropMode DropMode { get; set; } = DropMode.None;

        public virtual void OnDrop(DragItem source)
        {
            IDockView child;
            if (source.RelativeObject is BaseFloatWindow window)
            {
                child = window.Child;
                window.DetachChild(child);
            }
            else
            {
                child = source.RelativeObject as IDockView;
            }

            DockManager.FormatChildSize(child as ILayoutSize, new Size(ActualWidth, ActualHeight));
            DockManager.ChangeDockMode(child, ((ILayoutGroup)Model).Mode);
            DockManager.ChangeSide(child, Model.Side);
            //Cancel AttachObject information
            DockManager.ClearAttachObject(child);

            var group = Model as LayoutGroup;
            switch (DropMode)
            {
                case DropMode.Header:
                case DropMode.Center:
                    if (DropMode == DropMode.Center)
                    {
                        _index = 0;
                    }

                    _AttachDockView(child as UIElement, group);
                    break;
            }
        }

        public void CloseDropWindow()
        {
            if (_dragWindow != null)
            {
                DropMode = DropMode.None;
                _dragWindow.IsOpen = false;
                _dragWindow = null;
            }
        }

        public void HideDropWindow()
        {
            _dragWindow?.Hide();
        }

        public void ShowDropWindow()
        {
            if (_dragWindow == null)
            {
                CreateDropWindow();
            }

            var p = this.PointToScreenDPIWithoutFlowDirection(new Point());
            if (this is LayoutDocumentGroupControl)
            {
                if (DockViewParent == null)
                {
                    _dragWindow.DropPanel.InnerRect = new Rect(0, 0, ActualWidth, ActualHeight);
                    DockHelper.UpdateLocation(_dragWindow, p.X, p.Y, ActualWidth, ActualHeight);
                }
                else
                {
                    var panel = (LayoutGroupDocumentPanel)DockViewParent;
                    var p1 = panel.PointToScreenDPIWithoutFlowDirection(new Point());
                    _dragWindow.DropPanel.InnerRect = new Rect(p.X - p1.X, p.Y - p1.Y, ActualWidth, ActualHeight);
                    DockHelper.UpdateLocation(_dragWindow, p1.X, p1.Y, panel.ActualWidth, panel.ActualHeight);
                }
            }
            else
            {
                DockHelper.UpdateLocation(_dragWindow, p.X, p.Y, ActualWidth, ActualHeight);
            }

            if (!_dragWindow.IsOpen) _dragWindow.IsOpen = true;
            _dragWindow.Show();
        }

        public void Update(Point mouseP)
        {
            _dragWindow?.Update(mouseP);
        }

        public void AttachWith(IDockView source, AttachMode mode = AttachMode.Center)
        {
            switch (mode)
            {
                case AttachMode.Left:
                    DropMode = DropMode.Left;
                    break;
                case AttachMode.Top:
                    DropMode = DropMode.Top;
                    break;
                case AttachMode.Right:
                    DropMode = DropMode.Right;
                    break;
                case AttachMode.Bottom:
                    DropMode = DropMode.Bottom;
                    break;
                case AttachMode.Left_WithSplit:
                    DropMode = DropMode.Left_WithSplit;
                    break;
                case AttachMode.Top_WithSplit:
                    DropMode = DropMode.Top_WithSplit;
                    break;
                case AttachMode.Right_WithSplit:
                    DropMode = DropMode.Right_WithSplit;
                    break;
                case AttachMode.Bottom_WithSplit:
                    DropMode = DropMode.Bottom_WithSplit;
                    break;
                case AttachMode.Center:
                    DropMode = DropMode.Center;
                    break;
                default:
                    DropMode = DropMode.None;
                    break;
            }

            var item = new DragItem(source, DockMode.Normal, DragMode.None, new Point(), Rect.Empty, Size.Empty);
            OnDrop(item);
            DropMode = DropMode.None;
        }

        #endregion

        #region ILayoutGroupControl Members

        public double DesiredWidth
        {
            get { return _desiredWidth; }
            set
            {
                if (Math.Abs(_desiredWidth - value) > double.Epsilon)
                {
                    _desiredWidth = value;
                    if (_model != null)
                    {
                        foreach (var item in _model.Children)
                        {
                            item.DesiredWidth = value;
                        }
                    }
                }
            }
        }

        public double DesiredHeight
        {
            get { return _desiredHeight; }
            set
            {
                if (Math.Abs(_desiredHeight - value) > double.Epsilon)
                {
                    _desiredHeight = value;
                    if (_model != null)
                    {
                        foreach (var item in _model.Children)
                        {
                            item.DesiredHeight = value;
                        }
                    }
                }
            }
        }

        public double FloatLeft
        {
            get { return _floatLeft; }
            set
            {
                if (!(Math.Abs(_floatLeft - value) > double.Epsilon)) return;

                _floatLeft = value;

                if (_model == null) return;
                foreach (var item in _model.Children)
                {
                    item.FloatLeft = value;
                }
            }
        }

        public double FloatTop
        {
            get { return _floatTop; }
            set
            {
                if (!(Math.Abs(_floatTop - value) > double.Epsilon)) return;

                _floatTop = value;

                if (_model == null) return;
                foreach (var item in _model.Children)
                {
                    item.FloatTop = value;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public IDockModel Model
        {
            get { return _model; }
            set
            {
                if (_model == value) return;

                if (_model != null)
                {
                    ((LayoutGroup)_model).View = null;
                    if (_model.DockManager != null)
                    {
                        _model.DockManager.DragManager.OnDragStatusChanged -= OnDragStatusChanged;
                    }
                }

                _model = value as ILayoutGroup;

                if (_model == null) return;
                ((LayoutGroup)_model).View = this;
                if (_model.DockManager != null)
                {
                    _model.DockManager.DragManager.OnDragStatusChanged += OnDragStatusChanged;
                }
            }
        }

        public IDockView DockViewParent
        {
            get { return Parent as IDockView; }
        }

        public bool TryDetachFromParent(bool isDispose = true)
        {
            var parent = Parent;
            if (parent == null) return true;

            if (DockViewParent is ILayoutPanel layoutPanel)
            {
                if (layoutPanel.IsDocumentPanel)
                {
                    var panel = (LayoutGroupDocumentPanel)DockViewParent;
                    if (panel.Children.Count > 1 || DockManager.MainWindow != System.Windows.Window.GetWindow(panel))
                    {
                        panel.DetachChild(this);

                        if (isDispose) Dispose();
                        return true;
                    }

                    DockManager.RaiseDocumentToEmpty();
                    return false;
                }
            }

            if (parent is ILayoutViewParent layoutViewParent)
            {
                layoutViewParent.DetachChild(this);
            }

            if (parent is System.Windows.Window window)
            {
                window.Close();
            }

            DesiredHeight = ActualHeight;
            DesiredWidth = ActualWidth;

            if (isDispose) Dispose();

            return true;
        }

        public void AttachToParent(ILayoutPanel parent, int index)
        {
            switch (Model.Side)
            {
                case DockSide.Left:
                    parent.AttachChild(this, AttachMode.Left, index);
                    break;
                case DockSide.Right:
                    parent.AttachChild(this, AttachMode.Right, index);
                    break;
                case DockSide.Top:
                    parent.AttachChild(this, AttachMode.Top, index);
                    break;
                case DockSide.Bottom:
                    parent.AttachChild(this, AttachMode.Bottom, index);
                    break;
            }
        }

        public event EventHandler Disposed = delegate { };

        public virtual void Dispose()
        {
            SelectedItem = null;
            BindingOperations.ClearBinding(this, ItemsSourceProperty);
            Items.Clear();
            Model = null;
            _dragItem = null;
            _childrenBounds?.Clear();
            _childrenBounds = null;
            Disposed(this, new EventArgs());
        }

        #endregion

        #region Event handlers

        internal void OnDragStatusChanged(DragStatusChangedEventArgs args)
        {
            if (!args.IsDragging)
            {
                CloseDropWindow();
            }
        }

        #endregion

        #region Members

        public XElement GenerateLayout()
        {
            var element = new XElement("Group");
            element.SetAttributeValue("IsDocument", Mode == DragMode.Document);
            element.SetAttributeValue("Side", _model.Side);
            foreach (var item in _model.Children.Reverse().Where(child => child.CanSelect))
            {
                element.Add(new XElement("Item", item.ID));
            }

            return element;
        }

        public void HitTest(Point position, ActiveRectDropVisual activeRect)
        {
            _position = position;
            _activeRect = activeRect;
            _activeRect.Flag = DragManager.NONE;
            var p = this.PointToScreenDPIWithoutFlowDirection(new Point());
            p = new Point(position.X - p.X, position.Y - p.Y);
            VisualTreeHelper.HitTest(this, _HitFilter, _HitResult, new PointHitTestParameters(p));
            _activeRect = null;
        }

        public int IndexOf()
        {
            if (DockViewParent is LayoutGroupPanel panel)
            {
                return panel.Children.IndexOf(this);
            }

            return -1;
        }

        internal virtual void CreateDropWindow()
        {
            if (_dragWindow == null)
            {
                _dragWindow = new DropWindow(this);
            }
        }

        internal void UpdateChildrenBounds(Panel parent)
        {
            _childrenBounds = new List<Rect>();
            double hOffset = 0;
            foreach (TabItem child in parent.Children)
            {
                var childSize = child.TransformActualSizeToAncestor();
                _childrenBounds.Add(new Rect(new Point(hOffset, 0), childSize));
                hOffset += childSize.Width;
            }
        }

        private void _AttachDockView(UIElement view, LayoutGroup target)
        {
            if (view is LayoutGroupPanel panel)
            {
                foreach (UIElement item in panel.Children)
                {
                    _AttachDockView(item, target);
                }
            }

            if (view is BaseGroupControl control && control.Model is LayoutGroup model)
            {
                var children = new List<IDockElement>(model.Children.Reverse());
                model.Dispose();
                foreach (var item in children)
                {
                    target.Attach(item, _index);
                }
            }

            if (view is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private HitTestFilterBehavior _HitFilter(DependencyObject potentialHitTestTarget)
        {
            canUpdate = true;
            if (potentialHitTestTarget is AnchorHeaderControl && _activeRect.DropPanel.Source.DragMode != DragMode.Document)
            {
                if (DropMode == DropMode.Header && _index == -1)
                {
                    canUpdate = false;
                    return HitTestFilterBehavior.Stop;
                }

                _activeRect.Flag = DragManager.Head;
                _activeRect.Rect = new Rect(0, 0, 60, 20);
                _index = -1;
            }
            else if (potentialHitTestTarget is AnchorSidePanel
                     || potentialHitTestTarget is DocumentPanel)
            {
                UpdateChildrenBounds((Panel)potentialHitTestTarget);
                var p = ((Panel)potentialHitTestTarget).PointToScreenDPIWithoutFlowDirection(new Point());
                var index = _childrenBounds.FindIndex(new Point(_position.X - p.X, _position.Y - p.Y));
                Rect rect;
                if (index < 0)
                {
                    if (_childrenBounds.Count == 0)
                    {
                        rect = new Rect(0, 0, 0, 22);
                    }
                    else
                    {
                        rect = _childrenBounds.Last();
                        rect = new Rect(rect.X + rect.Width, 0, 0, rect.Height);
                    }

                    if (DropMode == DropMode.Header && _index == _childrenBounds.Count)
                    {
                        canUpdate = false;
                        return HitTestFilterBehavior.Stop;
                    }

                    _index = _childrenBounds.Count;
                }
                else
                {
                    rect = _childrenBounds[index];
                    if (DropMode == DropMode.Header && _index == index)
                    {
                        canUpdate = false;
                        return HitTestFilterBehavior.Stop;
                    }

                    _index = index;
                }

                _activeRect.Flag = DragManager.Head;
                _activeRect.Rect = new Rect(rect.X, 0, this is AnchorSideGroupControl ? 60 : 120, rect.Height);

                return HitTestFilterBehavior.Stop;
            }

            return HitTestFilterBehavior.Continue;
        }

        private HitTestResultBehavior _HitResult(HitTestResult result)
        {
            return HitTestResultBehavior.Stop;
        }

        #endregion
    }
}