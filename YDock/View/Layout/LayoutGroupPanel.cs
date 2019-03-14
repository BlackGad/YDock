using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using YDock.Enum;
using YDock.Interface;
using YDock.Model.Layout;
using YDock.View.Control;
using YDock.View.Window;

namespace YDock.View.Layout
{
    /// <summary>
    ///     The core class for layout and resize region
    /// </summary>
    public class LayoutGroupPanel : Panel,
                                    ILayoutPanel,
                                    IDragTarget
    {
        private double _dragBound1;
        private double _dragBound2;
        private Popup _dragPopup;

        private DropWindow _dragWnd;
        private Point _pointToScreen;

        private DockSide _side;

        #region Constructors

        internal LayoutGroupPanel(DockSide side = DockSide.None)
        {
            _side = side;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Represents recursion in the Children of the Panel containing <see cref="LayoutDocumentGroupControl" />
        /// </summary>
        public bool ContainDocument
        {
            get { return _side == DockSide.None; }
        }

        public Direction Direction { get; set; } = Direction.None;

        public bool IsEmpty
        {
            get { return Children.Count == 0; }
        }

        public bool IsRootPanel
        {
            get { return DockViewParent is LayoutRootPanel; }
        }

        #endregion

        #region Override members

        protected override Size MeasureOverride(Size availableSize)
        {
            if (InternalChildren.Count == 0) return availableSize;
            if (IsAnchorPanel || IsDocumentPanel)
            {
                return MeasureOverrideFull(availableSize);
            }

            return MeasureOverrideSplit(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (InternalChildren.Count == 0) return finalSize;
            if (IsAnchorPanel || IsDocumentPanel)
            {
                return ArrangeOverrideFull(finalSize);
            }

            return ArrangeOverrideSplit(finalSize);
        }

        #endregion

        #region IDragTarget Members

        public DragMode Mode
        {
            get { return DragMode.RootPanel; }
        }

        public DropMode DropMode { get; set; } = DropMode.None;

        public void OnDrop(DragItem source)
        {
            IDockView child;
            if (source.RelativeObj is BaseFloatWindow window)
            {
                child = window.Child;
                window.DetachChild(child);
            }
            else
            {
                child = source.RelativeObj as IDockView;
            }

            DockManager.ChangeDockMode(child, DockMode.Normal);
            DockManager.FormatChildSize(child as ILayoutSize, new Size(ActualWidth, ActualHeight));
            //Cancel AttachObj information
            DockManager.ClearAttachObj(child);

            switch (DropMode)
            {
                case DropMode.Left:
                    DockManager.ChangeSide(child, DockSide.Left);
                    AttachChild(child, AttachMode.Left, 0);
                    break;
                case DropMode.Top:
                    DockManager.ChangeSide(child, DockSide.Top);
                    AttachChild(child, AttachMode.Top, 0);
                    break;
                case DropMode.Right:
                    DockManager.ChangeSide(child, DockSide.Right);
                    AttachChild(child, AttachMode.Right, Count);
                    break;
                case DropMode.Bottom:
                    DockManager.ChangeSide(child, DockSide.Bottom);
                    AttachChild(child, AttachMode.Bottom, Count);
                    break;
            }

            if (source.RelativeObj is BaseFloatWindow floatWindow)
            {
                floatWindow.Close();
            }
        }

        public void CloseDropWindow()
        {
            if (_dragWnd != null)
            {
                DropMode = DropMode.None;
                _dragWnd.Close();
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
            DockHelper.UpdateLocation(_dragWnd, p.X, p.Y, ActualWidth, ActualHeight);
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
                default:
                    DropMode = DropMode.None;
                    break;
            }

            var item = new DragItem(source, DockMode.Normal, DragMode.None, new Point(), Rect.Empty, Size.Empty);
            OnDrop(item);
            DropMode = DropMode.None;
        }

        #endregion

        #region ILayoutPanel Members

        /// <summary>
        ///     Indicates that the Children of the Panel except <see cref="LayoutDragSplitter" /> is
        ///     <see cref="LayoutDocumentGroupControl" />
        /// </summary>
        public bool IsDocumentPanel
        {
            get { return this is LayoutGroupDocumentPanel; }
        }

        /// <summary>
        ///     Indicates that the Children of the Panel except <see cref="LayoutDragSplitter" /> is
        ///     <see cref="AnchorSideGroupControl" />
        /// </summary>
        public bool IsAnchorPanel { get; internal set; }

        public int Count
        {
            get { return Children.Count; }
        }

        public DockManager DockManager
        {
            get
            {
                var parent = DockViewParent;
                while (parent?.DockViewParent != null)
                {
                    if (parent.DockViewParent is DockManager manager)
                    {
                        return manager;
                    }

                    parent = parent.DockViewParent;
                }

                return null;
            }
        }

        public double DesiredWidth { get; set; }

        public double DesiredHeight { get; set; }

        public double FloatLeft
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public double FloatTop
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public DockSide Side
        {
            get { return _side; }
            internal set
            {
                if (_side != value)
                {
                    _side = value;
                    foreach (var child in Children)
                    {
                        if (child is LayoutGroupPanel layoutGroupPanel)
                        {
                            layoutGroupPanel.Side = value;
                        }

                        if (child is BaseGroupControl control &&
                            control.Model is BaseLayoutGroup layoutGroup)
                        {
                            layoutGroup.Side = value;
                        }
                    }
                }
            }
        }

        public IDockModel Model
        {
            get { return null; }
        }

        public IDockView DockViewParent
        {
            get { return Parent == null ? null : Parent as IDockView; }
        }

        public virtual void AttachChild(IDockView child, AttachMode mode, int index)
        {
            if (index < 0 || index > Count) throw new ArgumentOutOfRangeException("index");
            if (!AssertMode(mode)) throw new ArgumentException("mode is illegal!");

            var flag = true;

            switch (Direction)
            {
                case Direction.None:
                    if (mode == AttachMode.Left || mode == AttachMode.Right)
                    {
                        Direction = Direction.Horizontal;
                    }
                    else
                    {
                        Direction = Direction.Vertical;
                    }

                    break;
                case Direction.Horizontal:
                    if (mode == AttachMode.Left || mode == AttachMode.Right)
                    {
                        if (child is LayoutGroupPanel panel && !panel.IsDocumentPanel)
                        {
                            flag = false;
                            AttachSubPanel(panel, index);
                        }
                    }
                    else
                    {
                        if (IsRootPanel)
                        {
                            flag = false;
                            AttachToRootPanel(child, mode);
                        }
                        else
                        {
                            throw new ArgumentException("mode is illegal!");
                        }
                    }

                    break;
                case Direction.Vertical:
                    if (mode == AttachMode.Top || mode == AttachMode.Bottom)
                    {
                        if (child is LayoutGroupPanel panel && !panel.IsDocumentPanel)
                        {
                            flag = false;
                            AttachSubPanel(panel, index);
                        }
                    }
                    else
                    {
                        if (IsRootPanel)
                        {
                            flag = false;
                            AttachToRootPanel(child, mode);
                        }
                        else
                        {
                            throw new ArgumentException("mode is illegal!");
                        }
                    }

                    break;
            }

            if (flag)
            {
                AttachChild(child, index);
            }
        }

        public virtual void DetachChild(IDockView child, bool force = true)
        {
            if (!Children.Contains(child as UIElement)) return;

            InternalDetachChild(child);
            if (!force) return;

            if (Children.Count < 2)
            {
                Direction = Direction.None;
                IsAnchorPanel = false;
            }

            //If the element is empty and not a RootPanel, recursively remove it from the Parent
            if (Children.Count == 0 && !IsRootPanel)
            {
                ((ILayoutViewParent)Parent).DetachChild(this);
                Dispose();
            }

            //If there is only one element, the child element level is decremented by one.
            if (Children.Count != 1) return;

            child = Children[0] as IDockView;
            var parent = (ILayoutViewParent)Parent;
            var index = parent.IndexOf(this);
            //Remove yourself from the parent container
            parent.DetachChild(this, false);
            ProtectedDispose();
            //Add your own child elements from the parent container
            if (parent is LayoutGroupPanel panel)
            {
                switch (panel.Direction)
                {
                    case Direction.None:
                        panel.AttachChild(child, AttachMode.None, Math.Max(index - 1, 0));
                        break;
                    case Direction.Horizontal:
                        panel.AttachChild(child, AttachMode.Left, Math.Max(index - 1, 0));
                        break;
                    case Direction.Vertical:
                        panel.AttachChild(child, AttachMode.Top, Math.Max(index - 1, 0));
                        break;
                }
            }
            else
            {
                parent.AttachChild(child, AttachMode.None, Math.Max(index - 1, 0));
            }
        }

        public int IndexOf(IDockView child)
        {
            if (Children.Contains(child as UIElement))
            {
                return Children.IndexOf(child as UIElement);
            }

            return -1;
        }

        public event EventHandler Disposed = delegate { };

        public void Dispose()
        {
            foreach (var child in Children)
            {
                if (child is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            Children.Clear();
            Disposed(this, new EventArgs());
        }

        #endregion

        #region Event handlers

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            var delta = Direction == Direction.Horizontal ? _dragPopup.HorizontalOffset - _pointToScreen.X : _dragPopup.VerticalOffset - _pointToScreen.Y;
            var index = Children.IndexOf(sender as UIElement);
            var span1 = GetMinLength(Children[index - 1]);
            var span2 = GetMinLength(Children[index + 1]);
            if (Math.Abs(delta) > double.Epsilon)
            {
                if (Direction == Direction.Horizontal)
                {
                    if (_dragPopup.HorizontalOffset >= _dragBound1 + span1 && _dragPopup.HorizontalOffset <= _dragBound2 - span2)
                    {
                        ((ILayoutSize)Children[index - 1]).DesiredWidth += delta;
                        ((ILayoutSize)Children[index + 1]).DesiredWidth -= delta;
                    }
                    else
                    {
                        if (delta > 0)
                        {
                            ((ILayoutSize)Children[index - 1]).DesiredWidth += _dragBound2 - span2 - _pointToScreen.X;
                            ((ILayoutSize)Children[index + 1]).DesiredWidth -= _dragBound2 - span2 - _pointToScreen.X;
                        }
                        else
                        {
                            ((ILayoutSize)Children[index - 1]).DesiredWidth += _dragBound1 + span1 - _pointToScreen.X;
                            ((ILayoutSize)Children[index + 1]).DesiredWidth -= _dragBound1 + span1 - _pointToScreen.X;
                        }
                    }
                }
                else
                {
                    if (_dragPopup.VerticalOffset >= _dragBound1 + span1 && _dragPopup.VerticalOffset <= _dragBound2 - span2)
                    {
                        ((ILayoutSize)Children[index - 1]).DesiredHeight += delta;
                        ((ILayoutSize)Children[index + 1]).DesiredHeight -= delta;
                    }
                    else
                    {
                        if (delta > 0)
                        {
                            ((ILayoutSize)Children[index - 1]).DesiredHeight += _dragBound2 - span2 - _pointToScreen.Y;
                            ((ILayoutSize)Children[index + 1]).DesiredHeight -= _dragBound2 - span2 - _pointToScreen.Y;
                        }
                        else
                        {
                            ((ILayoutSize)Children[index - 1]).DesiredHeight += _dragBound1 + span1 - _pointToScreen.Y;
                            ((ILayoutSize)Children[index + 1]).DesiredHeight -= _dragBound1 + span1 - _pointToScreen.Y;
                        }
                    }
                }
            }

            DisposeDragPopup();

            InvalidateMeasure();
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Direction == Direction.Horizontal)
            {
                if (Math.Abs(e.HorizontalChange) > double.Epsilon)
                {
                    var newPos = _pointToScreen.X + e.HorizontalChange;
                    if (_dragBound1 + Constants.SideLength >= _dragBound2 - Constants.SideLength) return;
                    if (newPos >= _dragBound1 + Constants.SideLength && newPos <= _dragBound2 - Constants.SideLength)
                    {
                        _dragPopup.HorizontalOffset = newPos;
                    }
                    else
                    {
                        if (e.HorizontalChange > 0)
                        {
                            _dragPopup.HorizontalOffset = _dragBound2 - Constants.SideLength;
                        }
                        else
                        {
                            _dragPopup.HorizontalOffset = _dragBound1 + Constants.SideLength;
                        }
                    }
                }
                else
                {
                    _dragPopup.HorizontalOffset = _pointToScreen.X;
                }

                return;
            }

            if (Math.Abs(e.VerticalChange) > double.Epsilon)
            {
                var newPos = _pointToScreen.Y + e.VerticalChange;
                if (_dragBound1 + Constants.SideLength >= _dragBound2 - Constants.SideLength) return;
                if (newPos >= _dragBound1 + Constants.SideLength && newPos <= _dragBound2 - Constants.SideLength)
                {
                    _dragPopup.VerticalOffset = newPos;
                }
                else
                {
                    if (e.VerticalChange > 0)
                    {
                        _dragPopup.VerticalOffset = _dragBound2 - Constants.SideLength;
                    }
                    else
                    {
                        _dragPopup.VerticalOffset = _dragBound1 + Constants.SideLength;
                    }
                }
            }
            else
            {
                _dragPopup.VerticalOffset = _pointToScreen.Y;
            }
        }

        private void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            ComputeDragBounds(sender as LayoutDragSplitter, ref _dragBound1, ref _dragBound2);
            CreateDragPopup(sender as LayoutDragSplitter);
        }

        #endregion

        #region Members

        public void CreateDropWindow()
        {
            if (_dragWnd == null)
            {
                _dragWnd = new DropWindow(this);
            }
        }

        public XElement GenerateLayout()
        {
            var element = new XElement("Panel");
            element.SetAttributeValue("IsDocument", IsDocumentPanel);
            element.SetAttributeValue("Side", _side);
            element.SetAttributeValue("Direction", Direction);
            foreach (var child in Children)
            {
                if (child is BaseGroupControl control)
                {
                    element.Add(control.GenerateLayout());
                }
                else if (child is LayoutGroupPanel panel)
                {
                    element.Add(panel.GenerateLayout());
                }
            }

            return element;
        }

        protected void AttachSubPanel(LayoutGroupPanel subPanel, int index)
        {
            var children = new List<UIElement>();
            foreach (UIElement element in subPanel.Children)
            {
                children.Add(element);
            }

            children.Reverse();
            subPanel.Children.Clear();
            foreach (var element in children)
            {
                if (element is IDockView view)
                {
                    AttachChild(view, index);
                }
            }
        }

        protected void ProtectedDispose()
        {
            Children.Clear();
            Disposed(this, new EventArgs());
        }

        internal virtual bool AssertMode(AttachMode mode)
        {
            return mode == AttachMode.Left
                   || mode == AttachMode.Top
                   || mode == AttachMode.Right
                   || mode == AttachMode.Bottom;
        }

        internal void AttachChild(IDockView child, int index)
        {
            Children.Insert(index, (UIElement)child);
            if (Children.Count > 1)
            {
                switch (Direction)
                {
                    case Direction.Horizontal:
                        if (index % 2 == 0)
                        {
                            Children.Insert(index + 1, CreateSplitter(Cursors.SizeWE));
                        }
                        else
                        {
                            Children.Insert(index, CreateSplitter(Cursors.SizeWE));
                        }

                        break;
                    case Direction.Vertical:
                        if (index % 2 == 0)
                        {
                            Children.Insert(index + 1, CreateSplitter(Cursors.SizeNS));
                        }
                        else
                        {
                            Children.Insert(index, CreateSplitter(Cursors.SizeNS));
                        }

                        break;
                }
            }
        }

        internal void AttachToRootPanel(IDockView child, AttachMode mode)
        {
            var parent = (ILayoutViewParent)Parent;
            parent.DetachChild(this);
            var parentPanel = new LayoutGroupPanel
            {
                Direction = mode == AttachMode.Left || mode == AttachMode.Right ? Direction.Horizontal : Direction.Vertical
            };
            parent.AttachChild(parentPanel, AttachMode.None, 0);
            if (mode == AttachMode.Left || mode == AttachMode.Top)
            {
                parentPanel.AttachChild(this, 0);
                parentPanel.AttachChild(child, 0);
            }
            else
            {
                parentPanel.AttachChild(child, 0);
                parentPanel.AttachChild(this, 0);
            }
        }

        internal LayoutDragSplitter CreateSplitter(Cursor cursor)
        {
            var splitter = new LayoutDragSplitter
            {
                Cursor = cursor,
                Background = Direction == Direction.Horizontal ? ResourceManager.SplitterBrushVertical : ResourceManager.SplitterBrushHorizontal
            };
            splitter.DragStarted += OnDragStarted;
            splitter.DragDelta += OnDragDelta;
            splitter.DragCompleted += OnDragCompleted;
            return splitter;
        }

        internal void DestroySplitter(LayoutDragSplitter splitter)
        {
            splitter.DragStarted -= OnDragStarted;
            splitter.DragDelta -= OnDragDelta;
            splitter.DragCompleted -= OnDragCompleted;
        }

        internal void InternalDetachChild(IDockView child)
        {
            if (Children.Contains(child as UIElement))
            {
                var index = Children.IndexOf(child as UIElement);
                if (index > 0)
                {
                    Children.RemoveAt(index);
                    //Remove the corresponding Splitter
                    if (Children.Count > 0)
                    {
                        DestroySplitter(Children[index - 1] as LayoutDragSplitter);
                        Children.RemoveAt(index - 1);
                    }
                }
                else
                {
                    Children.RemoveAt(0);
                    //Remove the corresponding Splitter
                    if (Children.Count > 0)
                    {
                        DestroySplitter(Children[0] as LayoutDragSplitter);
                        Children.RemoveAt(0);
                    }
                }
            }
        }

        private Size ArrangeOverrideFull(Size finalSize)
        {
            var layoutGroups = new List<ILayoutSize>();
            for (var i = 0; i < InternalChildren.Count; i += 2)
            {
                layoutGroups.Add(InternalChildren[i] as ILayoutSize);
            }

            double wholeLength;
            double availableLength;
            var stars = new List<double>();
            switch (Direction)
            {
                case Direction.Horizontal:
                    wholeLength = layoutGroups.Sum(group => group.DesiredWidth);
                    foreach (var group in layoutGroups)
                    {
                        stars.Add(group.DesiredWidth / wholeLength);
                    }

                    availableLength = Math.Max(finalSize.Width - Constants.SplitterSpan * (layoutGroups.Count - 1), 0);
                    //When the minimum available space in the children is greater than SideLength,
                    //the space is allocated according to the actual length ratio of each child.
                    if (availableLength * stars.Min() >= Constants.SideLength)
                    {
                        return ArrangeUniverse(finalSize);
                    }
                    else
                    {
                        var deceed = wholeLength - availableLength;
                        if (deceed >= 0 && availableLength - layoutGroups.Count * Constants.SideLength > 0)
                        {
                            double offset = 0;
                            for (var i = 0; i < InternalChildren.Count; i += 2)
                            {
                                var children = layoutGroups[i / 2].DesiredWidth;
                                if (deceed > 0)
                                {
                                    if (children - deceed > Constants.SideLength)
                                    {
                                        InternalChildren[i].Arrange(new Rect(new Point(offset, 0), new Size(children - deceed, finalSize.Height)));
                                        offset += children - deceed;
                                        deceed = 0;
                                    }
                                    else
                                    {
                                        InternalChildren[i].Arrange(new Rect(new Point(offset, 0), new Size(Constants.SideLength, finalSize.Height)));
                                        offset += Constants.SideLength;
                                        deceed -= children - Constants.SideLength;
                                    }
                                }
                                else
                                {
                                    InternalChildren[i].Arrange(new Rect(new Point(offset, 0), new Size(children, finalSize.Height)));
                                    offset += children;
                                }

                                if (i + 1 < InternalChildren.Count)
                                {
                                    InternalChildren[i + 1].Arrange(new Rect(new Point(offset, 0), new Size(Constants.SplitterSpan, finalSize.Height)));
                                    offset += Constants.SplitterSpan;
                                }
                            }
                        }
                        else //Indicates that the available space is smaller than the minimum required space, so the cut is performed.
                        {
                            return ClipToBoundsArrange(finalSize);
                        }
                    }

                    break;
                case Direction.Vertical:
                    wholeLength = layoutGroups.Sum(group => group.DesiredHeight);
                    foreach (var group in layoutGroups)
                    {
                        stars.Add(group.DesiredHeight / wholeLength);
                    }

                    availableLength = Math.Max(finalSize.Height - Constants.SplitterSpan * (layoutGroups.Count - 1), 0);
                    //When the minimum available space in the children is greater than SideLength,
                    //the space is allocated according to the actual length ratio of each child.
                    if (availableLength * stars.Min() >= Constants.SideLength)
                    {
                        return ArrangeUniverse(finalSize);
                    }
                    else
                    {
                        var deceed = wholeLength - availableLength;
                        if (deceed >= 0 && availableLength - layoutGroups.Count * Constants.SideLength > 0)
                        {
                            double offset = 0;
                            for (var i = 0; i < InternalChildren.Count; i += 2)
                            {
                                var childLength = layoutGroups[i / 2].DesiredHeight;
                                if (deceed > 0)
                                {
                                    if (childLength - deceed > Constants.SideLength)
                                    {
                                        InternalChildren[i].Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, childLength - deceed)));
                                        offset += childLength - deceed;
                                        deceed = 0;
                                    }
                                    else
                                    {
                                        InternalChildren[i].Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, Constants.SideLength)));
                                        offset += Constants.SideLength;
                                        deceed -= childLength - Constants.SideLength;
                                    }
                                }
                                else
                                {
                                    InternalChildren[i].Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, childLength)));
                                    offset += childLength;
                                }

                                if (i + 1 < InternalChildren.Count)
                                {
                                    InternalChildren[i + 1].Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, Constants.SplitterSpan)));
                                    offset += Constants.SplitterSpan;
                                }
                            }
                        }
                        else //Indicates that the available space is smaller than the minimum required space, so the cut is performed.
                        {
                            return ClipToBoundsArrange(finalSize);
                        }
                    }

                    break;
                case Direction.None:
                    InternalChildren[0].Arrange(new Rect(new Point(), finalSize));
                    break;
            }

            return finalSize;
        }

        private Size ArrangeOverrideSplit(Size finalSize)
        {
            ILayoutSize childSize;
            //The wholeLength here means that there is no need to adjust the minimum total length of the child type to the size of the LayoutDocumentGroupControl.
            double wholeLength = 0;
            switch (Direction)
            {
                case Direction.Horizontal:
                    foreach (var child in InternalChildren)
                    {
                        if (child is LayoutDragSplitter)
                        {
                            wholeLength += Constants.SplitterSpan;
                        }
                        else
                        {
                            if (IsDocumentChild(child))
                            {
                                wholeLength += GetMinLength(child);
                            }
                            else
                            {
                                wholeLength += ((ILayoutSize)child).DesiredWidth;
                            }
                        }
                    }

                    //Automatically adjust the child type to the size of the LayoutDocumentGroupControl to fit the layout
                    if (wholeLength <= finalSize.Width)
                    {
                        return ArrangeUniverse(finalSize);
                    }
                    else //Otherwise reduce the size of other children to fit the layout
                    {
                        //Calculate the minimum length of all children
                        if (GetMinLength(this) <= finalSize.Width)
                        {
                            double offset = 0;
                            var exceed = wholeLength - finalSize.Width;
                            foreach (FrameworkElement child in InternalChildren)
                            {
                                if (IsDocumentChild(child))
                                {
                                    child.Arrange(new Rect(new Point(offset, 0), new Size(GetMinLength(child), finalSize.Height)));
                                    offset += GetMinLength(child);
                                }
                                else
                                {
                                    if (child is LayoutDragSplitter)
                                    {
                                        child.Arrange(new Rect(new Point(offset, 0), new Size(Constants.SplitterSpan, finalSize.Height)));
                                        offset += Constants.SplitterSpan;
                                    }
                                    else
                                    {
                                        childSize = (ILayoutSize)child;
                                        if (exceed > 0)
                                        {
                                            if (childSize.DesiredWidth - exceed >= Constants.SideLength)
                                            {
                                                child.Arrange(new Rect(new Point(offset, 0), new Size(childSize.DesiredWidth - exceed, finalSize.Height)));
                                                offset += childSize.DesiredWidth - exceed;
                                                exceed = 0;
                                            }
                                            else
                                            {
                                                exceed -= childSize.DesiredWidth - Constants.SideLength;
                                                child.Arrange(new Rect(new Point(offset, 0), new Size(Constants.SideLength, finalSize.Height)));
                                                offset += Constants.SideLength;
                                            }
                                        }
                                        else
                                        {
                                            child.Arrange(new Rect(new Point(offset, 0), new Size(childSize.DesiredWidth, finalSize.Height)));
                                            offset += childSize.DesiredWidth;
                                        }
                                    }
                                }
                            }
                        }
                        else //Indicates that the available space is smaller than the minimum required space, so the cut is performed.
                        {
                            ClipToBoundsArrange(finalSize);
                        }
                    }

                    break;
                case Direction.Vertical:
                    foreach (var child in InternalChildren)
                    {
                        if (child is LayoutDragSplitter)
                        {
                            wholeLength += Constants.SplitterSpan;
                        }
                        else
                        {
                            if (IsDocumentChild(child))
                            {
                                wholeLength += GetMinLength(child);
                            }
                            else
                            {
                                wholeLength += ((ILayoutSize)child).DesiredHeight;
                            }
                        }
                    }

                    //Automatically adjust the child type to the size of the LayoutDocumentGroupControl to fit the layout
                    if (wholeLength <= finalSize.Height)
                    {
                        return ArrangeUniverse(finalSize);
                    }
                    else //Otherwise reduce the size of other children to fit the layout
                    {
                        //Calculate the minimum length of all children and
                        if (GetMinLength(this) <= finalSize.Height)
                        {
                            double offset = 0;
                            var exceed = wholeLength - finalSize.Height;
                            foreach (FrameworkElement child in InternalChildren)
                            {
                                if (IsDocumentChild(child))
                                {
                                    child.Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, GetMinLength(child))));
                                    offset += GetMinLength(child);
                                }
                                else
                                {
                                    if (child is LayoutDragSplitter)
                                    {
                                        child.Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, Constants.SplitterSpan)));
                                        offset += Constants.SplitterSpan;
                                    }
                                    else
                                    {
                                        childSize = (ILayoutSize)child;
                                        if (exceed > 0)
                                        {
                                            if (childSize.DesiredHeight - exceed >= Constants.SideLength)
                                            {
                                                child.Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, childSize.DesiredHeight - exceed)));
                                                offset += childSize.DesiredHeight - exceed;
                                                exceed = 0;
                                            }
                                            else
                                            {
                                                exceed -= childSize.DesiredHeight - Constants.SideLength;
                                                child.Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, Constants.SideLength)));
                                                offset += Constants.SideLength;
                                            }
                                        }
                                        else
                                        {
                                            child.Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, childSize.DesiredHeight)));
                                            offset += childSize.DesiredHeight;
                                        }
                                    }
                                }
                            }
                        }
                        else //Indicates that the available space is smaller than the minimum required space, so the cut is performed.
                        {
                            ClipToBoundsArrange(finalSize);
                        }
                    }

                    break;
            }

            return finalSize;
        }

        private Size ArrangeUniverse(Size finalSize)
        {
            double offset = 0;
            switch (Direction)
            {
                case Direction.Horizontal:
                    foreach (FrameworkElement child in InternalChildren)
                    {
                        if (child is LayoutDragSplitter)
                        {
                            child.Arrange(new Rect(new Point(offset, 0), new Size(Constants.SplitterSpan, finalSize.Height)));
                            offset += Constants.SplitterSpan;
                        }
                        else
                        {
                            child.Arrange(new Rect(new Point(offset, 0), new Size(((ILayoutSize)child).DesiredWidth, finalSize.Height)));
                            offset += ((ILayoutSize)child).DesiredWidth;
                        }
                    }

                    break;
                case Direction.Vertical:
                    foreach (FrameworkElement child in InternalChildren)
                    {
                        if (child is LayoutDragSplitter)
                        {
                            child.Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, Constants.SplitterSpan)));
                            offset += Constants.SplitterSpan;
                        }
                        else
                        {
                            child.Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, ((ILayoutSize)child).DesiredHeight)));
                            offset += ((ILayoutSize)child).DesiredHeight;
                        }
                    }

                    break;
            }

            return finalSize;
        }

        private Size ClipToBoundsArrange(Size finalSize)
        {
            double avaLength, offset = 0, minLength;
            switch (Direction)
            {
                case Direction.Horizontal:
                    avaLength = finalSize.Width;

                    foreach (FrameworkElement child in InternalChildren)
                    {
                        if (child is LayoutDragSplitter)
                        {
                            if (avaLength >= Constants.SplitterSpan)
                            {
                                child.Arrange(new Rect(new Point(offset, 0), new Size(Constants.SplitterSpan, finalSize.Height)));
                                offset += Constants.SplitterSpan;
                                avaLength -= Constants.SplitterSpan;
                            }
                            else
                            {
                                child.Arrange(new Rect(new Point(offset, 0), new Size(avaLength, finalSize.Height)));
                                offset += avaLength;
                                avaLength = 0;
                            }
                        }
                        else
                        {
                            minLength = GetMinLength(child);
                            if (avaLength >= minLength)
                            {
                                child.Arrange(new Rect(new Point(offset, 0), new Size(minLength, finalSize.Height)));
                                offset += minLength;
                                avaLength -= minLength;
                            }
                            else
                            {
                                child.Arrange(new Rect(new Point(offset, 0), new Size(avaLength, finalSize.Height)));
                                offset += avaLength;
                                avaLength = 0;
                            }
                        }
                    }

                    break;
                case Direction.Vertical:
                    avaLength = finalSize.Height;

                    foreach (FrameworkElement child in InternalChildren)
                    {
                        if (child is LayoutDragSplitter)
                        {
                            if (avaLength >= Constants.SplitterSpan)
                            {
                                child.Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, Constants.SplitterSpan)));
                                offset += Constants.SplitterSpan;
                                avaLength -= Constants.SplitterSpan;
                            }
                            else
                            {
                                child.Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, avaLength)));
                                offset += avaLength;
                                avaLength = 0;
                            }
                        }
                        else
                        {
                            minLength = GetMinLength(child);
                            if (avaLength >= minLength)
                            {
                                child.Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, minLength)));
                                offset += minLength;
                                avaLength -= minLength;
                            }
                            else
                            {
                                child.Arrange(new Rect(new Point(0, offset), new Size(finalSize.Width, avaLength)));
                                offset += avaLength;
                                avaLength = 0;
                            }
                        }
                    }

                    break;
            }

            return finalSize;
        }

        private Size ClipToBoundsMeasure(Size availableSize)
        {
            double minLength;
            switch (Direction)
            {
                case Direction.Horizontal:
                    var availableLength = availableSize.Width;
                    for (var i = 0; i < InternalChildren.Count; i += 2)
                    {
                        minLength = GetMinLength(InternalChildren[i]);
                        if (availableLength >= minLength)
                        {
                            InternalChildren[i].Measure(new Size(minLength, availableSize.Height));
                            availableLength -= minLength;
                        }
                        else
                        {
                            InternalChildren[i].Measure(new Size(availableLength, availableSize.Height));
                            availableLength = 0;
                        }

                        if (i + 1 < InternalChildren.Count)
                        {
                            if (availableLength >= Constants.SplitterSpan)
                            {
                                InternalChildren[i + 1].Measure(new Size(Constants.SplitterSpan, availableSize.Height));
                                availableLength -= Constants.SplitterSpan;
                            }
                            else
                            {
                                InternalChildren[i + 1].Measure(new Size(availableLength, availableSize.Height));
                                availableLength = 0;
                            }
                        }
                    }

                    break;
                case Direction.Vertical:
                    availableLength = availableSize.Height;
                    for (var i = 0; i < InternalChildren.Count; i += 2)
                    {
                        minLength = GetMinLength(InternalChildren[i]);
                        if (availableLength >= minLength)
                        {
                            InternalChildren[i].Measure(new Size(availableSize.Width, minLength));
                            availableLength -= minLength;
                        }
                        else
                        {
                            InternalChildren[i].Measure(new Size(availableSize.Width, availableLength));
                            availableLength = 0;
                        }

                        if (i + 1 < InternalChildren.Count)
                        {
                            if (availableLength >= Constants.SplitterSpan)
                            {
                                InternalChildren[i + 1].Measure(new Size(availableSize.Width, Constants.SplitterSpan));
                                availableLength -= Constants.SplitterSpan;
                            }
                            else
                            {
                                InternalChildren[i + 1].Measure(new Size(availableSize.Width, availableLength));
                                availableLength = 0;
                            }
                        }
                    }

                    break;
            }

            return availableSize;
        }

        /// <summary>
        ///     Calculate the upper and lower boundary values when dragging
        /// </summary>
        /// <param name="splitter">Dragged object</param>
        /// <param name="x1">Lower bound</param>
        /// <param name="x2">Upper bound</param>
        private void ComputeDragBounds(LayoutDragSplitter splitter, ref double x1, ref double x2)
        {
            var index = Children.IndexOf(splitter);
            if (index > 1)
            {
                var splitterX1 = Children[index - 2];
                var pToScreen = this.PointToScreenDPIWithoutFlowDirection(new Point());
                var transform = splitterX1.TransformToAncestor(this);
                var pToInterPanel = transform.Transform(new Point(0, 0));
                pToScreen.X += pToInterPanel.X;
                pToScreen.Y += pToInterPanel.Y;
                if (Direction == Direction.Horizontal)
                {
                    x1 = pToScreen.X + Constants.SplitterSpan;
                }
                else
                {
                    x1 = pToScreen.Y + Constants.SplitterSpan;
                }
            }
            else
            {
                var pToScreen = this.PointToScreenDPIWithoutFlowDirection(new Point());
                if (Direction == Direction.Horizontal)
                {
                    x1 = pToScreen.X;
                }
                else
                {
                    x1 = pToScreen.Y;
                }
            }

            if (index < Children.Count - 2)
            {
                var splitterX2 = Children[index + 2];
                var pToScreen = this.PointToScreenDPIWithoutFlowDirection(new Point());
                var transform = splitterX2.TransformToAncestor(this);
                var pToInterPanel = transform.Transform(new Point(0, 0));
                pToScreen.X += pToInterPanel.X;
                pToScreen.Y += pToInterPanel.Y;
                if (Direction == Direction.Horizontal)
                {
                    x2 = pToScreen.X - Constants.SplitterSpan;
                }
                else
                {
                    x2 = pToScreen.Y - Constants.SplitterSpan;
                }
            }
            else
            {
                var pToScreen = this.PointToScreenDPIWithoutFlowDirection(new Point());
                if (Direction == Direction.Horizontal)
                {
                    x2 = pToScreen.X + ActualWidth - Constants.SplitterSpan;
                }
                else
                {
                    x2 = pToScreen.Y + ActualHeight - Constants.SplitterSpan;
                }
            }
        }

        private void CreateDragPopup(LayoutDragSplitter splitter)
        {
            _pointToScreen = this.PointToScreenDPIWithoutFlowDirection(new Point());
            var transform = splitter.TransformToAncestor(this);
            var pToInterPanel = transform.Transform(new Point(0, 0));
            _pointToScreen.X += pToInterPanel.X;
            _pointToScreen.Y += pToInterPanel.Y;

            var index = Children.IndexOf(splitter);
            switch (Direction)
            {
                case Direction.Horizontal:
                    ((ILayoutSize)Children[index - 1]).DesiredWidth = ((FrameworkElement)Children[index - 1]).ActualWidth;
                    ((ILayoutSize)Children[index + 1]).DesiredWidth = ((FrameworkElement)Children[index + 1]).ActualWidth;
                    break;
                case Direction.Vertical:
                    ((ILayoutSize)Children[index - 1]).DesiredHeight = ((FrameworkElement)Children[index - 1]).ActualHeight;
                    ((ILayoutSize)Children[index + 1]).DesiredHeight = ((FrameworkElement)Children[index + 1]).ActualHeight;
                    break;
            }

            _dragPopup = new Popup
            {
                Child = new Rectangle
                {
                    Height = splitter.ActualHeight,
                    Width = splitter.ActualWidth,
                    Fill = Brushes.Black,
                    Opacity = Constants.DragOpacity,
                    IsHitTestVisible = false
                },
                Placement = PlacementMode.Absolute,
                HorizontalOffset = _pointToScreen.X,
                VerticalOffset = _pointToScreen.Y,
                AllowsTransparency = true
            };

            DockHelper.ComputeSpliterLocation(_dragPopup, _pointToScreen, new Size(splitter.ActualWidth, splitter.ActualHeight));
            _dragPopup.IsOpen = true;
        }

        private void DisposeDragPopup()
        {
            _dragPopup.IsOpen = false;
            _dragPopup = null;
        }

        /// <summary>
        ///     Calculate the minimum length of obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private double GetMinLength(object obj)
        {
            if (obj is LayoutGroupPanel child && child.ContainDocument)
            {
                if (child.Direction == Direction)
                {
                    double length = 0;
                    foreach (var childElement in child.Children)
                    {
                        length += GetMinLength(childElement);
                    }

                    return length;
                }

                //If the direction is different, find the value with the smallest minimum length among the child elements.
                double max = 0;
                foreach (var childElement in child.Children)
                {
                    var value = GetMinLength(childElement);
                    if (max < value)
                    {
                        max = value;
                    }
                }

                return max;
            }

            if (obj is LayoutDragSplitter)
            {
                return Constants.SplitterSpan;
            }

            return Constants.SideLength;
        }

        /// <summary>
        ///     Determine if Child contains a document (document area size is determined by Auto)
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        private bool IsDocumentChild(object child)
        {
            return child is LayoutDocumentGroupControl || child is LayoutGroupPanel panel && panel.ContainDocument;
        }

        /// <summary>
        ///     Children are all called for the same <see cref="ILayoutGroupControl" />
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        private Size MeasureOverrideFull(Size availableSize)
        {
            var layoutGroups = new List<ILayoutSize>();
            for (var i = 0; i < InternalChildren.Count; i += 2)
            {
                layoutGroups.Add(InternalChildren[i] as ILayoutSize);
            }

            double wholeLength;
            double availableLength;
            var stars = new List<double>();
            switch (Direction)
            {
                case Direction.Horizontal:
                    wholeLength = layoutGroups.Sum(group => group.DesiredWidth);
                    foreach (var group in layoutGroups)
                    {
                        stars.Add(group.DesiredWidth / wholeLength);
                    }

                    availableLength = Math.Max(availableSize.Width - Constants.SplitterSpan * (layoutGroups.Count - 1), 0);
                    //When the minimum available space in the children is greater than SideLength,
                    //the space is allocated according to the actual length ratio of each child.
                    if (availableLength * stars.Min() >= Constants.SideLength)
                    {
                        for (var i = 0; i < InternalChildren.Count; i += 2)
                        {
                            var layoutSize = (ILayoutSize)InternalChildren[i];
                            layoutSize.DesiredWidth = stars[i / 2] * availableLength;
                            InternalChildren[i].Measure(new Size(layoutSize.DesiredWidth, availableSize.Height));
                            if (i + 1 < InternalChildren.Count)
                            {
                                InternalChildren[i + 1].Measure(new Size(Constants.SplitterSpan, availableSize.Height));
                            }
                        }
                    }
                    else
                    {
                        var deceed = wholeLength - availableLength;
                        if (deceed >= 0 && availableLength - layoutGroups.Count * Constants.SideLength > 0)
                        {
                            for (var i = 0; i < InternalChildren.Count; i += 2)
                            {
                                var childLength = layoutGroups[i / 2].DesiredWidth;
                                if (deceed > 0)
                                {
                                    if (childLength - deceed > Constants.SideLength)
                                    {
                                        InternalChildren[i].Measure(new Size(childLength - deceed, availableSize.Height));
                                        deceed = 0;
                                    }
                                    else
                                    {
                                        InternalChildren[i].Measure(new Size(Constants.SideLength, availableSize.Height));
                                        deceed -= childLength - Constants.SideLength;
                                    }
                                }
                                else
                                {
                                    InternalChildren[i].Measure(new Size(childLength, availableSize.Height));
                                }

                                if (i + 1 < InternalChildren.Count)
                                {
                                    InternalChildren[i + 1].Measure(new Size(Constants.SplitterSpan, availableSize.Height));
                                }
                            }
                        }
                        else //Indicates that the available space is smaller than the minimum required space, so the cut is performed.
                        {
                            return ClipToBoundsMeasure(availableSize);
                        }
                    }

                    break;
                case Direction.Vertical:
                    wholeLength = layoutGroups.Sum(group => group.DesiredHeight);
                    foreach (var group in layoutGroups)
                    {
                        stars.Add(group.DesiredHeight / wholeLength);
                    }

                    availableLength = Math.Max(availableSize.Height - Constants.SplitterSpan * (layoutGroups.Count - 1), 0);
                    //When the minimum available space in the children is greater than SideLength,
                    //the space is allocated according to the actual length ratio of each child.
                    if (availableLength * stars.Min() >= Constants.SideLength)
                    {
                        for (var i = 0; i < InternalChildren.Count; i += 2)
                        {
                            var internalChild = (ILayoutSize)InternalChildren[i];
                            internalChild.DesiredHeight = stars[i / 2] * availableLength;
                            InternalChildren[i].Measure(new Size(availableSize.Width, internalChild.DesiredHeight));
                            if (i + 1 < InternalChildren.Count)
                            {
                                InternalChildren[i + 1].Measure(new Size(availableSize.Width, Constants.SplitterSpan));
                            }
                        }
                    }
                    else
                    {
                        var deceed = wholeLength - availableLength;
                        if (deceed >= 0 && availableLength - layoutGroups.Count * Constants.SideLength > 0)
                        {
                            for (var i = 0; i < InternalChildren.Count; i += 2)
                            {
                                var childLength = layoutGroups[i / 2].DesiredHeight;
                                if (deceed > 0)
                                {
                                    if (childLength - deceed > Constants.SideLength)
                                    {
                                        InternalChildren[i].Measure(new Size(availableSize.Width, childLength - deceed));
                                        deceed = 0;
                                    }
                                    else
                                    {
                                        InternalChildren[i].Measure(new Size(availableSize.Width, Constants.SideLength));
                                        deceed -= childLength - Constants.SideLength;
                                    }
                                }
                                else
                                {
                                    InternalChildren[i].Measure(new Size(availableSize.Width, childLength));
                                }

                                if (i + 1 < InternalChildren.Count)
                                {
                                    InternalChildren[i + 1].Measure(new Size(availableSize.Width, Constants.SplitterSpan));
                                }
                            }
                        }
                        else //Indicates that the available space is smaller than the minimum required space, so the cut is performed.
                        {
                            return ClipToBoundsMeasure(availableSize);
                        }
                    }

                    break;
                case Direction.None:
                    InternalChildren[0].Measure(availableSize);
                    break;
            }

            return availableSize;
        }

        private Size MeasureOverrideSplit(Size availableSize)
        {
            ILayoutSize childSize;
            var documentChild = default(FrameworkElement);
            //The wholeLength here means that there is no need to adjust the minimum total length of the child type to the size of the LayoutDocumentGroupControl.
            double wholeLength = 0;
            switch (Direction)
            {
                case Direction.Horizontal:
                    foreach (var child in InternalChildren)
                    {
                        if (child is LayoutDragSplitter)
                        {
                            wholeLength += Constants.SplitterSpan;
                        }
                        else
                        {
                            if (IsDocumentChild(child))
                            {
                                wholeLength += GetMinLength(child);
                            }
                            else
                            {
                                wholeLength += ((ILayoutSize)child).DesiredWidth;
                            }
                        }
                    }

                    //Automatically adjust the child type to the size of the LayoutDocumentGroupControl to fit the layout
                    if (wholeLength <= availableSize.Width)
                    {
                        double useLength = 0;
                        foreach (FrameworkElement child in InternalChildren)
                        {
                            if (child is LayoutDragSplitter)
                            {
                                useLength += Constants.SplitterSpan;
                                child.Measure(new Size(Constants.SplitterSpan, availableSize.Height));
                            }
                            else
                            {
                                if (documentChild == null && IsDocumentChild(child))
                                {
                                    documentChild = child;
                                }
                                else
                                {
                                    useLength += ((ILayoutSize)child).DesiredWidth;
                                    child.Measure(new Size(((ILayoutSize)child).DesiredWidth, availableSize.Height));
                                }
                            }
                        }

                        var layoutSize = (ILayoutSize)documentChild;
                        if (layoutSize != null)
                        {
                            layoutSize.DesiredWidth = availableSize.Width - useLength;
                            layoutSize.DesiredHeight = availableSize.Height;
                            documentChild.Measure(new Size(layoutSize.DesiredWidth, layoutSize.DesiredHeight));
                        }
                    }
                    else //Otherwise reduce the size of other children to fit the layout
                    {
                        //Calculate the minimum length of all children
                        if (GetMinLength(this) <= availableSize.Width)
                        {
                            var exceed = wholeLength - availableSize.Width;
                            foreach (FrameworkElement child in InternalChildren)
                            {
                                if (IsDocumentChild(child))
                                {
                                    child.Measure(new Size(GetMinLength(child), availableSize.Height));
                                }
                                else
                                {
                                    if (child is LayoutDragSplitter)
                                    {
                                        child.Measure(new Size(Constants.SplitterSpan, availableSize.Height));
                                    }
                                    else
                                    {
                                        childSize = (ILayoutSize)child;
                                        if (exceed > 0)
                                        {
                                            if (childSize.DesiredWidth - exceed >= Constants.SideLength)
                                            {
                                                child.Measure(new Size(childSize.DesiredWidth - exceed, availableSize.Height));
                                                exceed = 0;
                                            }
                                            else
                                            {
                                                exceed -= childSize.DesiredWidth - Constants.SideLength;
                                                child.Measure(new Size(Constants.SideLength, availableSize.Height));
                                            }
                                        }
                                        else
                                        {
                                            child.Measure(new Size(childSize.DesiredWidth, availableSize.Height));
                                        }
                                    }
                                }
                            }
                        }
                        else //Indicates that the available space is smaller than the minimum required space, so the cut is performed.
                        {
                            return ClipToBoundsMeasure(availableSize);
                        }
                    }

                    break;
                case Direction.Vertical:
                    foreach (var child in InternalChildren)
                    {
                        if (child is LayoutDragSplitter)
                        {
                            wholeLength += Constants.SplitterSpan;
                        }
                        else
                        {
                            if (IsDocumentChild(child))
                            {
                                wholeLength += GetMinLength(child);
                            }
                            else
                            {
                                wholeLength += ((ILayoutSize)child).DesiredHeight;
                            }
                        }
                    }

                    //Automatically adjust the child type to the size of the LayoutDocumentGroupControl to fit the layout
                    if (wholeLength <= availableSize.Height)
                    {
                        double useLength = 0;
                        foreach (FrameworkElement child in InternalChildren)
                        {
                            if (child is LayoutDragSplitter)
                            {
                                useLength += Constants.SplitterSpan;
                                child.Measure(new Size(availableSize.Width, Constants.SplitterSpan));
                            }
                            else
                            {
                                if (documentChild == null && IsDocumentChild(child))
                                {
                                    documentChild = child;
                                }
                                else
                                {
                                    var layoutSize = (ILayoutSize)child;
                                    useLength += layoutSize.DesiredHeight;
                                    child.Measure(new Size(availableSize.Width, layoutSize.DesiredHeight));
                                }
                            }
                        }

                        var size = (ILayoutSize)documentChild;
                        if (size != null)
                        {
                            size.DesiredWidth = availableSize.Width;
                            size.DesiredHeight = availableSize.Height - useLength;
                        }

                        if (documentChild != null)
                        {
                            documentChild.Measure(new Size(availableSize.Width, availableSize.Height - useLength));
                        }
                    }
                    else //Otherwise reduce the size of other children to fit the layout
                    {
                        //Calculate the minimum length of all children
                        if (GetMinLength(this) <= availableSize.Height)
                        {
                            var exceed = wholeLength - availableSize.Height;
                            foreach (FrameworkElement child in InternalChildren)
                            {
                                if (IsDocumentChild(child))
                                {
                                    child.Measure(new Size(availableSize.Width, GetMinLength(child)));
                                }
                                else
                                {
                                    if (child is LayoutDragSplitter)
                                    {
                                        child.Measure(new Size(availableSize.Width, Constants.SplitterSpan));
                                    }
                                    else
                                    {
                                        childSize = (ILayoutSize)child;
                                        if (exceed > 0)
                                        {
                                            if (childSize.DesiredHeight - exceed >= Constants.SideLength)
                                            {
                                                child.Measure(new Size(availableSize.Width, childSize.DesiredHeight - exceed));
                                                exceed = 0;
                                            }
                                            else
                                            {
                                                exceed -= childSize.DesiredHeight - Constants.SideLength;
                                                child.Measure(new Size(availableSize.Width, Constants.SideLength));
                                            }
                                        }
                                        else
                                        {
                                            child.Measure(new Size(availableSize.Width, childSize.DesiredHeight));
                                        }
                                    }
                                }
                            }
                        }
                        else //Indicates that the available space is smaller than the minimum required space, so the cut is performed.
                        {
                            return ClipToBoundsMeasure(availableSize);
                        }
                    }

                    break;
            }

            return availableSize;
        }

        #endregion
    }
}