﻿using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using YDock.Enum;
using YDock.Interface;
using YDock.Model;
using YDock.Model.Layout;
using YDock.View;
using YDock.View.Control;
using YDock.View.Layout;
using YDock.View.Render;
using YDock.View.Window;

namespace YDock
{
    public class DragManager
    {
        #region Constants

        internal const int Active = 0x2000;
        internal const int Bottom = 0x0008;
        internal const int Center = 0x0010;
        internal const int Head = 0x0040;
        internal const int LEFT = 0x0001;
        internal const int NONE = 0x0000;
        internal const int Right = 0x0004;
        internal const int Split = 0x1000;
        internal const int Tab = 0x0020;
        internal const int Top = 0x0002;

        #endregion

        internal BaseFloatWindow _dragWnd;
        private IDragTarget _dragTarget;
        private bool _isDragging;

        private Point _mouseP;
        private Size _rootSize;

        #region Constructors

        internal DragManager(DockManager dockManager)
        {
            DockManager = dockManager;
        }

        #endregion

        #region Properties

        public DockManager DockManager { get; }

        public bool IsDragging
        {
            get { return _isDragging; }
            set
            {
                if (_isDragging != value)
                {
                    _isDragging = value;
                    OnDragStatusChanged(new DragStatusChangedEventArgs(value));
                }
            }
        }

        public bool IsDragOverRoot { get; set; }

        internal DragItem DragItem { get; set; }

        internal IDragTarget DragTarget
        {
            get { return _dragTarget; }
            set
            {
                if (_dragTarget != value)
                {
                    _dragTarget?.HideDropWindow();
                    _dragTarget = value;
                    _dragTarget?.ShowDropWindow();
                }
                else if (_dragTarget != null && !IsDragOverRoot)
                {
                    if (DragItem.DragMode == DragMode.Document && _dragTarget.Mode == DragMode.Anchor) return;
                    _dragTarget.Update(_mouseP);
                }
            }
        }

        #endregion

        #region Events

        public event DragStatusChanged OnDragStatusChanged = delegate { };

        #endregion

        #region Members

        internal void DoDragDrop()
        {
            if (DockManager.LayoutRootPanel.RootGroupPanel?.DropMode != DropMode.None)
            {
                DockManager.LayoutRootPanel.RootGroupPanel?.OnDrop(DragItem);
            }
            else if (DragTarget?.DropMode != DropMode.None)
            {
                DragTarget?.OnDrop(DragItem);
            }

            IsDragging = false;

            AfterDrag();
        }

        internal void IntoDragAction(DragItem dragItem, bool _isInvokeByFloatWnd = false)
        {
            DockManager.UpdateWindowZOrder();

            DragItem = dragItem;
            //被浮动窗口调用则不需要调用BeforeDrag()
            if (_isInvokeByFloatWnd)
            {
                IsDragging = true;
                if (_dragWnd == null)
                {
                    _dragWnd = DragItem.RelativeObj as BaseFloatWindow;
                }
            }
            else
            {
                BeforeDrag();
            }

            //初始化最外层的_rootTarget
            _rootSize = DockManager.LayoutRootPanel.RootGroupPanel.TransformActualSizeToAncestor();

            if (!_isInvokeByFloatWnd && _dragWnd is DocumentGroupWindow)
            {
                _dragWnd.Recreate();
            }
        }

        internal void OnMouseMove()
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                DockManager.LayoutRootPanel.RootGroupPanel?.HideDropWindow();
                DragTarget = null;
                return;
            }

            var flag = false;
            _mouseP = DockHelper.GetMousePosition(DockManager);
            foreach (var wnd in DockManager.FloatWindows)
            {
                if (wnd != _dragWnd
                    && wnd.RestoreBounds.Contains(_mouseP)
                    && !(wnd is DocumentGroupWindow && DragItem.DragMode == DragMode.Anchor)
                    && !(wnd is AnchorGroupWindow && DragItem.DragMode == DragMode.Document))
                {
                    if (wnd is DocumentGroupWindow)
                    {
                        if (DockManager.IsBehindToMainWindow(wnd))
                        {
                            continue;
                        }
                    }

                    if (wnd != DockManager.FloatWindows.First())
                    {
                        DockManager.MoveFloatTo(wnd);
                        Application.Current.Dispatcher.InvokeAsync(() =>
                                                                   {
                                                                       Win32Helper.BringWindowToTop(wnd.Handle);
                                                                       Win32Helper.BringWindowToTop(_dragWnd.Handle);
                                                                   },
                                                                   DispatcherPriority.Background);
                    }

                    wnd.HitTest(_mouseP);

                    DockManager.LayoutRootPanel.RootGroupPanel.HideDropWindow();
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                var p = DockHelper.GetMousePositionRelativeTo(DockManager.LayoutRootPanel.RootGroupPanel);
                if (p.X >= 0 && p.Y >= 0
                             && p.X <= _rootSize.Width
                             && p.Y <= _rootSize.Height)
                {
                    if (DragItem.DragMode != DragMode.Document)
                    {
                        DockManager.LayoutRootPanel.RootGroupPanel?.ShowDropWindow();
                        DockManager.LayoutRootPanel.RootGroupPanel?.Update(_mouseP);
                    }

                    VisualTreeHelper.HitTest(DockManager.LayoutRootPanel.RootGroupPanel, _HitFilter, HitResult, new PointHitTestParameters(p));
                }
                else
                {
                    if (DragItem.DragMode != DragMode.Document)
                    {
                        DockManager.LayoutRootPanel.RootGroupPanel?.HideDropWindow();
                    }

                    DragTarget = null;
                }
            }
        }

        private void _DestroyDragItem()
        {
            DragItem.Dispose();
            DragItem = null;
            DragTarget = null;
        }

        private HitTestFilterBehavior _HitFilter(DependencyObject potentialHitTestTarget)
        {
            if (potentialHitTestTarget is BaseGroupControl)
            {
                //设置DragTarget，以实时显示TargetWnd
                DragTarget = potentialHitTestTarget as IDragTarget;
                return HitTestFilterBehavior.Stop;
            }

            return HitTestFilterBehavior.Continue;
        }

        private HitTestResultBehavior HitResult(HitTestResult result)
        {
            DragTarget = null;
            return HitTestResultBehavior.Stop;
        }

        private void InitDragItem()
        {
            LayoutGroup group;
            IDockElement element;
            var mouseP = DockHelper.GetMousePosition(DockManager);
            switch (DragItem.DockMode)
            {
                case DockMode.Normal:
                    if (DragItem.RelativeObj is ILayoutGroup)
                    {
                        var _layoutGroup = DragItem.RelativeObj as LayoutGroup;

                        #region AttachObj

                        var _parent = _layoutGroup.View.DockViewParent as LayoutGroupPanel;
                        var _mode = _parent.Direction == Direction.Horizontal ? AttachMode.Left : AttachMode.Top;
                        if (_parent.Direction == Direction.None)
                        {
                            _mode = AttachMode.None;
                        }

                        var _index = _parent.IndexOf(_layoutGroup.View);
                        if (_parent.Children.Count - 1 > _index)
                        {
                            _layoutGroup.AttachObj = new AttachObject(_layoutGroup, _parent.Children[_index + 2] as INotifyDisposable, _index, _mode);
                        }
                        else
                        {
                            _layoutGroup.AttachObj = new AttachObject(_layoutGroup, _parent.Children[_index - 2] as INotifyDisposable, _index, _mode);
                        }

                        #endregion

                        //这里移动的一定是AnchorSideGroup，故将其从父级LayoutGroupPanel移走，但不Dispose留着构造浮动窗口
                        if ((_layoutGroup.View as ILayoutGroupControl).TryDetachFromParent(false))
                        {
                            //注意重新设置Mode
                            _layoutGroup.Mode = DockMode.Float;
                            _dragWnd = new AnchorGroupWindow(DockManager)
                            {
                                Left = mouseP.X - DragItem.ClickPos.X - 1,
                                Top = mouseP.Y - DragItem.ClickPos.Y - 1
                            };
                            _dragWnd.AttachChild(_layoutGroup.View, AttachMode.None, 0);
                            _dragWnd.Show();
                        }
                    }
                    else if (DragItem.RelativeObj is IDockElement)
                    {
                        element = DragItem.RelativeObj as IDockElement;

                        #region AttachObj

                        var _parent = (element.Container as LayoutGroup).View as BaseGroupControl;
                        var _index = element.Container.IndexOf(element);

                        #endregion

                        if (element.IsDocument)
                        {
                            group = new LayoutDocumentGroup(DockMode.Float, DockManager);
                        }
                        else
                        {
                            group = new LayoutGroup(element.Side, DockMode.Float, DockManager);
                            group.AttachObj = new AttachObject(group, _parent, _index);
                        }

                        //先从逻辑父级中移除
                        element.Container.Detach(element);
                        //再加入新的逻辑父级
                        group.Attach(element);
                        //创建新的浮动窗口，并初始化位置
                        if (element.IsDocument)
                        {
                            _dragWnd = new DocumentGroupWindow(DockManager);
                            _dragWnd.AttachChild(new LayoutDocumentGroupControl(group), AttachMode.None, 0);
                            _dragWnd.Top = mouseP.Y - DragItem.ClickPos.Y;
                            _dragWnd.Left = mouseP.X - DragItem.ClickPos.X - DragItem.ClickRect.Left - Constants.DocumentWindowPadding;
                        }
                        else
                        {
                            _dragWnd = new AnchorGroupWindow(DockManager) { NeedReCreate = DragItem.DragMode == DragMode.Anchor };
                            _dragWnd.AttachChild(new AnchorSideGroupControl(group) { IsDraggingFromDock = DragItem.DragMode == DragMode.Anchor }, AttachMode.None, 0);
                            if (!_dragWnd.NeedReCreate)
                            {
                                _dragWnd.Top = mouseP.Y - DragItem.ClickPos.Y;
                                _dragWnd.Left = mouseP.X - DragItem.ClickPos.X - DragItem.ClickRect.Left - Constants.DocumentWindowPadding;
                            }
                            else
                            {
                                _dragWnd.Top = mouseP.Y + DragItem.ClickPos.Y - _dragWnd.Height;
                                _dragWnd.Left = mouseP.X - DragItem.ClickPos.X - Constants.DocumentWindowPadding;
                            }
                        }

                        if (_dragWnd is DocumentGroupWindow)
                        {
                            _dragWnd.Recreate();
                        }

                        _dragWnd.Show();
                    }

                    break;
                case DockMode.DockBar:
                    //这里表示从自动隐藏窗口进行的拖动，因此这里移除自动隐藏窗口
                    element = DragItem.RelativeObj as IDockElement;
                    element.Container.Detach(element);
                    //创建新的浮动窗口，并初始化位置
                    group = new LayoutGroup(element.Side, DockMode.Float, DockManager);
                    group.Attach(element);
                    _dragWnd = new AnchorGroupWindow(DockManager)
                    {
                        Left = mouseP.X - DragItem.ClickPos.X - 1,
                        Top = mouseP.Y - DragItem.ClickPos.Y - 1
                    };
                    _dragWnd.AttachChild(new AnchorSideGroupControl(group), AttachMode.None, 0);
                    _dragWnd.Show();
                    break;
                case DockMode.Float:
                    if (DragItem.RelativeObj is IDockElement)
                    {
                        element = DragItem.RelativeObj as IDockElement;
                        var control = element.Container.View as BaseGroupControl;
                        if (control.Items.Count == 1 && control.Parent is BaseFloatWindow)
                        {
                            _dragWnd = control.Parent as BaseFloatWindow;
                            _dragWnd.DetachChild(control);
                            _dragWnd.Close();
                            _dragWnd = new DocumentGroupWindow(DockManager);
                            _dragWnd.AttachChild(control, AttachMode.None, 0);
                            _dragWnd.Top = mouseP.Y - DragItem.ClickPos.Y;
                            _dragWnd.Left = mouseP.X - DragItem.ClickPos.X - DragItem.ClickRect.Left - Constants.DocumentWindowPadding;
                            _dragWnd.Recreate();
                            _dragWnd.Show();
                        }
                        else
                        {
                            #region AttachObj

                            var _parent = (element.Container as LayoutGroup).View as BaseGroupControl;
                            var _index = element.Container.IndexOf(element);

                            #endregion

                            if (element.IsDocument)
                            {
                                group = new LayoutDocumentGroup(DockMode.Float, DockManager);
                            }
                            else
                            {
                                group = new LayoutGroup(element.Side, DockMode.Float, DockManager);
                                group.AttachObj = new AttachObject(group, _parent, _index);
                            }

                            //先从逻辑父级中移除
                            element.Container.Detach(element);
                            //再加入新的逻辑父级
                            group.Attach(element);
                            //创建新的浮动窗口，并初始化位置
                            //这里可知引起drag的时DragTabItem故这里创建临时的DragTabWindow
                            if (element.IsDocument)
                            {
                                _dragWnd = new DocumentGroupWindow(DockManager);
                                _dragWnd.AttachChild(new LayoutDocumentGroupControl(group), AttachMode.None, 0);
                                _dragWnd.Top = mouseP.Y - DragItem.ClickPos.Y;
                                _dragWnd.Left = mouseP.X - DragItem.ClickPos.X - DragItem.ClickRect.Left - Constants.DocumentWindowPadding;
                            }
                            else
                            {
                                _dragWnd = new AnchorGroupWindow(DockManager) { NeedReCreate = DragItem.DragMode == DragMode.Anchor };
                                _dragWnd.AttachChild(new AnchorSideGroupControl(group) { IsDraggingFromDock = DragItem.DragMode == DragMode.Anchor },
                                                     AttachMode.None,
                                                     0);
                                if (!_dragWnd.NeedReCreate)
                                {
                                    _dragWnd.Top = mouseP.Y - DragItem.ClickPos.Y;
                                    _dragWnd.Left = mouseP.X - DragItem.ClickPos.X - DragItem.ClickRect.Left - Constants.DocumentWindowPadding;
                                }
                                else
                                {
                                    _dragWnd.Top = mouseP.Y + DragItem.ClickPos.Y - _dragWnd.Height;
                                    _dragWnd.Left = mouseP.X - DragItem.ClickPos.X - Constants.DocumentWindowPadding;
                                }
                            }

                            if (_dragWnd is DocumentGroupWindow)
                            {
                                _dragWnd.Recreate();
                            }

                            _dragWnd.Show();
                        }
                    }
                    else if (DragItem.RelativeObj is ILayoutGroup)
                    {
                        group = DragItem.RelativeObj as LayoutGroup;
                        //表示此时的浮动窗口为IsSingleMode
                        if (group.View.DockViewParent == null)
                        {
                            _dragWnd = (group.View as BaseGroupControl).Parent as BaseFloatWindow;
                        }
                        else
                        {
                            #region AttachObj

                            var _parent = group.View.DockViewParent as LayoutGroupPanel;
                            var _mode = _parent.Direction == Direction.Horizontal ? AttachMode.Left : AttachMode.Top;
                            if (_parent.Direction == Direction.None)
                            {
                                _mode = AttachMode.None;
                            }

                            var _index = _parent.IndexOf(group.View);
                            if (_parent.Children.Count - 1 > _index)
                            {
                                group.AttachObj = new AttachObject(group, _parent.Children[_index + 2] as INotifyDisposable, _index, _mode);
                            }
                            else
                            {
                                group.AttachObj = new AttachObject(group, _parent.Children[_index - 2] as INotifyDisposable, _index, _mode);
                            }

                            #endregion

                            //这里移动的一定是AnchorSideGroup，故将其从父级LayoutGroupPanel移走，但不Dispose留着构造浮动窗口
                            if ((group.View as ILayoutGroupControl).TryDetachFromParent(false))
                            {
                                _dragWnd = new AnchorGroupWindow(DockManager)
                                {
                                    Left = mouseP.X - DragItem.ClickPos.X - 1,
                                    Top = mouseP.Y - DragItem.ClickPos.Y - 1
                                };
                                _dragWnd.AttachChild(group.View, AttachMode.None, 0);
                                _dragWnd.Show();
                            }
                        }
                    }

                    break;
            }
        }

        private void AfterDrag()
        {
            if (_dragWnd != null && _dragWnd.NeedReCreate)
            {
                _dragWnd.Recreate();
            }

            _dragWnd = null;
            DockManager.LayoutRootPanel.RootGroupPanel.CloseDropWindow();
            _DestroyDragItem();

            IsDragOverRoot = false;
            BaseDropPanel.ActiveVisual = null;
            BaseDropPanel.CurrentRect = null;
            CommandManager.InvalidateRequerySuggested();
        }

        private void BeforeDrag()
        {
            InitDragItem();
        }

        #endregion
    }
}