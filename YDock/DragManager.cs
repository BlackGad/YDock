using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using YDock.Enum;
using YDock.Interface;
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
        internal BaseFloatWindow _dragWindow;
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

        internal void IntoDragAction(DragItem dragItem, bool isInvokeByFloatWindow = false)
        {
            DockManager.UpdateWindowZOrder();

            DragItem = dragItem;
            //Called by a floating window does not need to call BeforeDrag()
            if (isInvokeByFloatWindow)
            {
                IsDragging = true;
                if (_dragWindow == null)
                {
                    _dragWindow = DragItem.RelativeObject as BaseFloatWindow;
                }
            }
            else
            {
                BeforeDrag();
            }

            //Initialize the outermost _rootTarget
            _rootSize = DockManager.LayoutRootPanel.RootGroupPanel.TransformActualSizeToAncestor();

            if (!isInvokeByFloatWindow && _dragWindow is DocumentGroupWindow)
            {
                _dragWindow.Recreate();
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
                if (wnd != _dragWindow
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
                                                                       Win32Helper.BringWindowToTop(_dragWindow.Handle);
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

                    if (DockManager.LayoutRootPanel.RootGroupPanel != null)
                    {
                        VisualTreeHelper.HitTest(DockManager.LayoutRootPanel.RootGroupPanel, _HitFilter, HitResult, new PointHitTestParameters(p));
                    }
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
            if (potentialHitTestTarget is BaseGroupControl target)
            {
                //Set DragTarget to display TargetWindow in real time
                DragTarget = target;
                return HitTestFilterBehavior.Stop;
            }

            return HitTestFilterBehavior.Continue;
        }

        private void AfterDrag()
        {
            if (_dragWindow != null && _dragWindow.NeedReCreate)
            {
                _dragWindow.Recreate();
            }

            _dragWindow = null;
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

        private HitTestResultBehavior HitResult(HitTestResult result)
        {
            DragTarget = null;
            return HitTestResultBehavior.Stop;
        }

        private void InitDragItem()
        {
            LayoutGroup group;
            var mouseP = DockHelper.GetMousePosition(DockManager);
            switch (DragItem.DockMode)
            {
                case DockMode.Normal:
                {
                    if (DragItem.RelativeObject is ILayoutGroup)
                    {
                        var layoutGroup = (LayoutGroup)DragItem.RelativeObject;

                        #region AttachObject

                        var parent = (LayoutGroupPanel)layoutGroup.View.DockViewParent;
                        var mode = parent.Direction == Direction.Horizontal ? AttachMode.Left : AttachMode.Top;
                        if (parent.Direction == Direction.None)
                        {
                            mode = AttachMode.None;
                        }

                        var index = parent.IndexOf(layoutGroup.View);
                        if (parent.Children.Count - 1 > index)
                        {
                            layoutGroup.AttachObject = new AttachObject(layoutGroup, parent.Children[index + 2] as INotifyDisposable, index, mode);
                        }
                        else
                        {
                            layoutGroup.AttachObject = new AttachObject(layoutGroup, parent.Children[index - 2] as INotifyDisposable, index, mode);
                        }

                        #endregion

                        //The move here must be AnchorSideGroup, so remove it from the parent LayoutGroupPanel, but do not dispose to construct the floating window.
                        if (((ILayoutGroupControl)layoutGroup.View).TryDetachFromParent(false))
                        {
                            //Note to reset Mode
                            layoutGroup.Mode = DockMode.Float;
                            _dragWindow = new AnchorGroupWindow(DockManager)
                            {
                                Left = mouseP.X - DragItem.ClickPos.X - 1,
                                Top = mouseP.Y - DragItem.ClickPos.Y - 1
                            };
                            _dragWindow.AttachChild(layoutGroup.View, AttachMode.None, 0);
                            _dragWindow.Show();
                        }
                    }
                    else if (DragItem.RelativeObject is IDockElement element)
                    {
                        #region AttachObject

                        var parent = ((LayoutGroup)element.Container).View as BaseGroupControl;
                        var index = element.Container.IndexOf(element);

                        #endregion

                        if (element.IsDocument)
                        {
                            group = new LayoutDocumentGroup(DockMode.Float, DockManager);
                        }
                        else
                        {
                            group = new LayoutGroup(element.Side, DockMode.Float, DockManager);
                            group.AttachObject = new AttachObject(group, parent, index);
                        }

                        //Remove from the logical parent first
                        element.Container.Detach(element);
                        //Add a new logical parent
                        group.Attach(element);
                        //Create a new floating window and initialize the location
                        if (element.IsDocument)
                        {
                            _dragWindow = new DocumentGroupWindow(DockManager);
                            _dragWindow.AttachChild(new LayoutDocumentGroupControl(group), AttachMode.None, 0);
                            _dragWindow.Top = mouseP.Y - DragItem.ClickPos.Y;
                            _dragWindow.Left = mouseP.X - DragItem.ClickPos.X - DragItem.ClickRect.Left - Constants.DocumentWindowPadding;
                        }
                        else
                        {
                            _dragWindow = new AnchorGroupWindow(DockManager) { NeedReCreate = DragItem.DragMode == DragMode.Anchor };
                            _dragWindow.AttachChild(new AnchorSideGroupControl(group) { IsDraggingFromDock = DragItem.DragMode == DragMode.Anchor }, AttachMode.None, 0);
                            if (!_dragWindow.NeedReCreate)
                            {
                                _dragWindow.Top = mouseP.Y - DragItem.ClickPos.Y;
                                _dragWindow.Left = mouseP.X - DragItem.ClickPos.X - DragItem.ClickRect.Left - Constants.DocumentWindowPadding;
                            }
                            else
                            {
                                _dragWindow.Top = mouseP.Y + DragItem.ClickPos.Y - _dragWindow.Height;
                                _dragWindow.Left = mouseP.X - DragItem.ClickPos.X - Constants.DocumentWindowPadding;
                            }
                        }

                        if (_dragWindow is DocumentGroupWindow)
                        {
                            _dragWindow.Recreate();
                        }

                        _dragWindow.Show();
                    }
                }

                    break;
                case DockMode.DockBar:
                {
                    //This represents the drag from the auto-hide window, so remove the auto-hide window here.
                    var element = (IDockElement)DragItem.RelativeObject;
                    element.Container.Detach(element);
                    //Create a new floating window and initialize the location
                    group = new LayoutGroup(element.Side, DockMode.Float, DockManager);
                    group.Attach(element);
                    _dragWindow = new AnchorGroupWindow(DockManager)
                    {
                        Left = mouseP.X - DragItem.ClickPos.X - 1,
                        Top = mouseP.Y - DragItem.ClickPos.Y - 1
                    };
                    _dragWindow.AttachChild(new AnchorSideGroupControl(group), AttachMode.None, 0);
                    _dragWindow.Show();
                }
                    break;
                case DockMode.Float:
                {
                    if (DragItem.RelativeObject is IDockElement element)
                    {
                        var control = (BaseGroupControl)element.Container.View;
                        if (control.Items.Count == 1 && control.Parent is BaseFloatWindow window)
                        {
                            _dragWindow = window;
                            _dragWindow.DetachChild(control);
                            _dragWindow.Close();
                            _dragWindow = new DocumentGroupWindow(DockManager);
                            _dragWindow.AttachChild(control, AttachMode.None, 0);
                            _dragWindow.Top = mouseP.Y - DragItem.ClickPos.Y;
                            _dragWindow.Left = mouseP.X - DragItem.ClickPos.X - DragItem.ClickRect.Left - Constants.DocumentWindowPadding;
                            _dragWindow.Recreate();
                            _dragWindow.Show();
                        }
                        else
                        {
                            #region AttachObject

                            var parent = ((LayoutGroup)element.Container).View as BaseGroupControl;
                            var index = element.Container.IndexOf(element);

                            #endregion

                            if (element.IsDocument)
                            {
                                group = new LayoutDocumentGroup(DockMode.Float, DockManager);
                            }
                            else
                            {
                                group = new LayoutGroup(element.Side, DockMode.Float, DockManager);
                                group.AttachObject = new AttachObject(group, parent, index);
                            }

                            //Remove from the logical parent first
                            element.Container.Detach(element);
                            //Add a new logical parent
                            group.Attach(element);
                            //Create a new floating window and initialize the location
                            //Here we can see the DragTabItem that caused the drag, so create a temporary DragTabWindow here.
                            if (element.IsDocument)
                            {
                                _dragWindow = new DocumentGroupWindow(DockManager);
                                _dragWindow.AttachChild(new LayoutDocumentGroupControl(group), AttachMode.None, 0);
                                _dragWindow.Top = mouseP.Y - DragItem.ClickPos.Y;
                                _dragWindow.Left = mouseP.X - DragItem.ClickPos.X - DragItem.ClickRect.Left - Constants.DocumentWindowPadding;
                            }
                            else
                            {
                                _dragWindow = new AnchorGroupWindow(DockManager) { NeedReCreate = DragItem.DragMode == DragMode.Anchor };
                                _dragWindow.AttachChild(new AnchorSideGroupControl(group) { IsDraggingFromDock = DragItem.DragMode == DragMode.Anchor },
                                                        AttachMode.None,
                                                        0);
                                if (!_dragWindow.NeedReCreate)
                                {
                                    _dragWindow.Top = mouseP.Y - DragItem.ClickPos.Y;
                                    _dragWindow.Left = mouseP.X - DragItem.ClickPos.X - DragItem.ClickRect.Left - Constants.DocumentWindowPadding;
                                }
                                else
                                {
                                    _dragWindow.Top = mouseP.Y + DragItem.ClickPos.Y - _dragWindow.Height;
                                    _dragWindow.Left = mouseP.X - DragItem.ClickPos.X - Constants.DocumentWindowPadding;
                                }
                            }

                            if (_dragWindow is DocumentGroupWindow)
                            {
                                _dragWindow.Recreate();
                            }

                            _dragWindow.Show();
                        }
                    }
                    else if (DragItem.RelativeObject is ILayoutGroup)
                    {
                        group = (LayoutGroup)DragItem.RelativeObject;
                        //Indicates that the floating window at this time is IsSingleMode
                        if (group.View.DockViewParent == null)
                        {
                            _dragWindow = ((BaseGroupControl)group.View).Parent as BaseFloatWindow;
                        }
                        else
                        {
                            #region AttachObject

                            var parent = (LayoutGroupPanel)group.View.DockViewParent;
                            var mode = parent.Direction == Direction.Horizontal ? AttachMode.Left : AttachMode.Top;
                            if (parent.Direction == Direction.None)
                            {
                                mode = AttachMode.None;
                            }

                            var index = parent.IndexOf(group.View);
                            if (parent.Children.Count - 1 > index)
                            {
                                group.AttachObject = new AttachObject(group, parent.Children[index + 2] as INotifyDisposable, index, mode);
                            }
                            else
                            {
                                group.AttachObject = new AttachObject(group, parent.Children[index - 2] as INotifyDisposable, index, mode);
                            }

                            #endregion

                            //The move here must be AnchorSideGroup,
                            //so remove it from the parent LayoutGroupPanel,
                            //but do not dispose to construct the floating window.
                            if (((ILayoutGroupControl)group.View).TryDetachFromParent(false))
                            {
                                _dragWindow = new AnchorGroupWindow(DockManager)
                                {
                                    Left = mouseP.X - DragItem.ClickPos.X - 1,
                                    Top = mouseP.Y - DragItem.ClickPos.Y - 1
                                };
                                _dragWindow.AttachChild(group.View, AttachMode.None, 0);
                                _dragWindow.Show();
                            }
                        }
                    }
                }

                    break;
            }
        }

        #endregion
    }
}