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
    public class BaseGroupControl : TabControl,
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

        private DropWindow _dragWnd;

        private double _floatLeft;

        private double _floatTop;

        private bool _isDraggingFromDock;

        private ILayoutGroup _model;
        private Point _mouseP;

        #region Constructors

        internal BaseGroupControl(ILayoutGroup model, double desiredWidth = Constants.DockDefaultWidthLength, double desiredHeight = Constants.DockDefaultHeightLength)
        {
            Model = model;
            SetBinding(ItemsSourceProperty, new Binding("Model.Children_CanSelect") { Source = this });
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
            get { return (_model as LayoutGroup).Children_CanSelect.Count(); }
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
                if (DockViewParent is LayoutGroupPanel)
                {
                    return (DockViewParent as LayoutGroupPanel).Count;
                }

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

        public virtual DragMode Mode { get; }

        public DockManager DockManager
        {
            get { return _model.DockManager; }
        }

        public DropMode DropMode { get; set; } = DropMode.None;

        public virtual void OnDrop(DragItem source)
        {
            IDockView child;
            if (source.RelativeObj is BaseFloatWindow)
            {
                child = (source.RelativeObj as BaseFloatWindow).Child;
                (source.RelativeObj as BaseFloatWindow).DetachChild(child);
            }
            else
            {
                child = source.RelativeObj as IDockView;
            }

            DockManager.FormatChildSize(child as ILayoutSize, new Size(ActualWidth, ActualHeight));
            DockManager.ChangeDockMode(child, (Model as ILayoutGroup).Mode);
            DockManager.ChangeSide(child, Model.Side);
            //取消AttachObj信息
            DockManager.ClearAttachObj(child);

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
            if (_dragWnd != null)
            {
                DropMode = DropMode.None;
                _dragWnd.IsOpen = false;
                _dragWnd = null;
            }
        }

        public void HideDropWindow()
        {
            _dragWnd?.Hide();
        }

        public void ShowDropWindow()
        {
            if (_dragWnd == null)
            {
                CreateDropWindow();
            }

            var p = this.PointToScreenDPIWithoutFlowDirection(new Point());
            if (this is LayoutDocumentGroupControl)
            {
                if (DockViewParent == null)
                {
                    _dragWnd.DropPanel.InnerRect = new Rect(0, 0, ActualWidth, ActualHeight);
                    DockHelper.UpdateLocation(_dragWnd, p.X, p.Y, ActualWidth, ActualHeight);
                }
                else
                {
                    var panel = DockViewParent as LayoutGroupDocumentPanel;
                    var p1 = panel.PointToScreenDPIWithoutFlowDirection(new Point());
                    _dragWnd.DropPanel.InnerRect = new Rect(p.X - p1.X, p.Y - p1.Y, ActualWidth, ActualHeight);
                    DockHelper.UpdateLocation(_dragWnd, p1.X, p1.Y, panel.ActualWidth, panel.ActualHeight);
                }
            }
            else
            {
                DockHelper.UpdateLocation(_dragWnd, p.X, p.Y, ActualWidth, ActualHeight);
            }

            if (!_dragWnd.IsOpen) _dragWnd.IsOpen = true;
            _dragWnd.Show();
        }

        public void Update(Point mouseP)
        {
            _dragWnd?.Update(mouseP);
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
                if (_floatLeft != value)
                {
                    _floatLeft = value;
                    if (_model != null)
                    {
                        foreach (var item in _model.Children)
                        {
                            item.FloatLeft = value;
                        }
                    }
                }
            }
        }

        public double FloatTop
        {
            get { return _floatTop; }
            set
            {
                if (_floatTop != value)
                {
                    _floatTop = value;
                    if (_model != null)
                    {
                        foreach (var item in _model.Children)
                        {
                            item.FloatTop = value;
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public IDockModel Model
        {
            get { return _model; }
            set
            {
                if (_model != value)
                {
                    if (_model != null)
                    {
                        (_model as LayoutGroup).View = null;
                        if (_model.DockManager != null)
                        {
                            _model.DockManager.DragManager.OnDragStatusChanged -= OnDragStatusChanged;
                        }
                    }

                    _model = value as ILayoutGroup;
                    if (_model != null)
                    {
                        (_model as LayoutGroup).View = this;
                        if (_model.DockManager != null)
                        {
                            _model.DockManager.DragManager.OnDragStatusChanged += OnDragStatusChanged;
                        }
                    }
                }
            }
        }

        public IDockView DockViewParent
        {
            get { return Parent as IDockView; }
        }

        public bool TryDeatchFromParent(bool isDispose = true)
        {
            if (Parent != null)
            {
                if (DockViewParent is ILayoutPanel)
                {
                    if ((DockViewParent as ILayoutPanel).IsDocumentPanel)
                    {
                        var ctrl = this as LayoutDocumentGroupControl;
                        var panel = DockViewParent as LayoutGroupDocumentPanel;
                        if (panel.Children.Count > 1 || DockManager.MainWindow != System.Windows.Window.GetWindow(panel))
                        {
                            panel.DetachChild(this);
                            if (isDispose)
                            {
                                Dispose();
                            }

                            return true;
                        }

                        DockManager.RaiseDocumentToEmpty();
                        return false;
                    }
                }

                var parent = Parent;
                (parent as ILayoutViewParent).DetachChild(this);
                if (parent is System.Windows.Window) (parent as System.Windows.Window).Close();
                DesiredHeight = ActualHeight;
                DesiredWidth = ActualWidth;
                if (isDispose)
                {
                    Dispose();
                }
            }

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

        public void HitTest(Point mouseP, ActiveRectDropVisual activeRect)
        {
            _mouseP = mouseP;
            _activeRect = activeRect;
            _activeRect.Flag = DragManager.NONE;
            var p = this.PointToScreenDPIWithoutFlowDirection(new Point());
            p = new Point(mouseP.X - p.X, mouseP.Y - p.Y);
            VisualTreeHelper.HitTest(this, _HitFilter, _HitRessult, new PointHitTestParameters(p));
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
            if (_dragWnd == null)
            {
                _dragWnd = new DropWindow(this);
            }
        }

        internal void UpdateChildrenBounds(Panel parent)
        {
            _childrenBounds = new List<Rect>();
            double hoffset = 0;
            foreach (TabItem child in parent.Children)
            {
                var childSize = child.TransformActualSizeToAncestor();
                _childrenBounds.Add(new Rect(new Point(hoffset, 0), childSize));
                hoffset += childSize.Width;
            }
        }

        private void _AttachDockView(UIElement view, LayoutGroup target)
        {
            if (view is LayoutGroupPanel)
            {
                foreach (UIElement item in (view as LayoutGroupPanel).Children)
                {
                    _AttachDockView(item, target);
                }
            }

            if (view is BaseGroupControl)
            {
                var model = (view as BaseGroupControl).Model as LayoutGroup;
                var _children = new List<IDockElement>(model.Children.Reverse());
                model.Dispose();
                foreach (var item in _children)
                {
                    target.Attach(item, _index);
                }
            }

            if (view is IDisposable)
            {
                (view as IDisposable).Dispose();
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

                _activeRect.Flag = DragManager.HEAD;
                _activeRect.Rect = new Rect(0, 0, 60, 20);
                _index = -1;
            }
            else if (potentialHitTestTarget is AnchorSidePanel
                     || potentialHitTestTarget is DocumentPanel)
            {
                UpdateChildrenBounds(potentialHitTestTarget as Panel);
                var p = (potentialHitTestTarget as Panel).PointToScreenDPIWithoutFlowDirection(new Point());
                var index = _childrenBounds.FindIndex(new Point(_mouseP.X - p.X, _mouseP.Y - p.Y));
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

                _activeRect.Flag = DragManager.HEAD;
                _activeRect.Rect = new Rect(rect.X, 0, this is AnchorSideGroupControl ? 60 : 120, rect.Height);

                return HitTestFilterBehavior.Stop;
            }

            return HitTestFilterBehavior.Continue;
        }

        private HitTestResultBehavior _HitRessult(HitTestResult result)
        {
            return HitTestResultBehavior.Stop;
        }

        #endregion
    }
}