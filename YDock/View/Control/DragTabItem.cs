using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YDock.Enum;
using YDock.Global.Commands;
using YDock.Interface;
using YDock.Model.Element;
using YDock.Model.Layout;
using YDock.View.Menu;

namespace YDock.View.Control
{
    public class DragTabItem : TabItem,
                               IDockView
    {
        private BaseGroupControl _dockViewParent;

        #region Constructors

        static DragTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragTabItem), new FrameworkPropertyMetadata(typeof(DragTabItem)));
        }

        internal DragTabItem(BaseGroupControl dockViewParent)
        {
            AllowDrop = true;
            _dockViewParent = dockViewParent;
        }

        #endregion

        #region Properties

        public ILayoutGroup Container
        {
            get { return _dockViewParent.Model as ILayoutGroup; }
        }

        #endregion

        #region Override members

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.MiddleButton == MouseButtonState.Pressed && Content is DockElement dockElement)
            {
                if (dockElement.IsActive)
                {
                    Container.ShowWithActive(null);
                }

                dockElement.CanSelect = false;
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            if (oldContent != null)
            {
                ContextMenu = null;
            }

            if (newContent is IDockItem dockItem)
            {
                if (_dockViewParent is AnchorSideGroupControl)
                {
                    ContextMenu = new DockMenu(dockItem);
                }
                else
                {
                    ContextMenu = new DocumentMenu(dockItem as IDockElement);
                }
            }

            ToolTip = string.Empty;
        }

        protected override void OnToolTipOpening(ToolTipEventArgs e)
        {
            if (Content is DockElement element)
            {
                ToolTip = element.ToolTip;
            }

            base.OnToolTipOpening(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }

            _dockViewParent._mouseInside = false;
            _dockViewParent._dragItem = null;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            //Set after the base class event is processed
            base.OnMouseLeftButtonDown(e);

            _dockViewParent._mouseInside = true;
            _dockViewParent._mouseDown = e.GetPosition(this);
            _dockViewParent._dragItem = Content as IDockElement;

            if (_dockViewParent._dragItem?.Container is LayoutDocumentGroup)
            {
                _dockViewParent._rect = new Rect();
            }
            else
            {
                _dockViewParent._rect = DockHelper.CreateChildRectFromParent(VisualParent as Panel, this);
            }

            _dockViewParent.UpdateChildrenBounds(VisualParent as Panel);

            Container.ShowWithActive(_dockViewParent._dragItem);

            if (!IsMouseCaptured)
            {
                CaptureMouse();
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            Container.ShowWithActive(Content as IDockElement);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {
                if (_dockViewParent._dragItem != null)
                {
                    var parent = VisualParent as Panel;
                    var p = e.GetPosition(parent);
                    var src = Container.IndexOf(_dockViewParent._dragItem);
                    var des = _dockViewParent._childrenBounds.FindIndex(p);
                    if (des < 0)
                    {
                        if (IsMouseCaptured)
                        {
                            ReleaseMouseCapture();
                        }

                        //TODO Drag
                        var item = _dockViewParent._dragItem;
                        _dockViewParent._dragItem = null;
                        item.DesiredWidth = _dockViewParent.ActualWidth;
                        item.DesiredHeight = _dockViewParent.ActualHeight;
                        if (_dockViewParent is AnchorSideGroupControl)
                        {
                            _dockViewParent.Model.DockManager.DragManager.IntoDragAction(new DragItem(item,
                                                                                                      item.Mode,
                                                                                                      DragMode.Anchor,
                                                                                                      _dockViewParent._mouseDown,
                                                                                                      _dockViewParent._rect,
                                                                                                      new Size(item.DesiredWidth, item.DesiredHeight)));
                        }
                        else
                        {
                            _dockViewParent.Model.DockManager.DragManager.IntoDragAction(new DragItem(item,
                                                                                                      item.Mode,
                                                                                                      DragMode.Document,
                                                                                                      _dockViewParent._mouseDown,
                                                                                                      _dockViewParent._rect,
                                                                                                      new Size(item.DesiredWidth, item.DesiredHeight)));
                        }
                    }
                    else
                    {
                        if (_dockViewParent._mouseInside)
                        {
                            if (src != des)
                            {
                                MoveTo(src, des, parent);
                                _dockViewParent._mouseInside = false;
                            }
                            else if (!_dockViewParent._mouseInside)
                            {
                                _dockViewParent._mouseInside = true;
                            }
                        }
                        else
                        {
                            if (src == des)
                            {
                                _dockViewParent._mouseInside = true;
                            }
                            else
                            {
                                if (des < src)
                                {
                                    double len = 0;
                                    for (var i = 0; i < des; i++)
                                    {
                                        len += _dockViewParent._childrenBounds[i].Size.Width;
                                    }

                                    len += _dockViewParent._childrenBounds[src].Size.Width;
                                    if (len > p.X)
                                    {
                                        MoveTo(src, des, parent);
                                    }
                                }
                                else
                                {
                                    double len = 0;
                                    for (var i = 0; i < src; i++)
                                    {
                                        len += _dockViewParent._childrenBounds[i].Size.Width;
                                    }

                                    len += _dockViewParent._childrenBounds[des].Size.Width;
                                    if (len < p.X)
                                    {
                                        MoveTo(src, des, parent);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            var control = _dockViewParent;
            if (control != null)
            {
                control.SelectedItem = Content;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            CommandBindings.Add(new CommandBinding(GlobalCommands.HideStatusCommand, OnCommandExecute, OnCommandCanExecute));
            base.OnInitialized(e);
        }

        #endregion

        #region IDockView Members

        public IDockModel Model
        {
            get { return null; }
        }

        public IDockView DockViewParent
        {
            get { return _dockViewParent; }
        }

        public void Dispose()
        {
            _dockViewParent = null;
            if (ContextMenu is IDisposable disposable)
            {
                disposable.Dispose();
            }

            ContextMenu = null;
            Content = null;
        }

        #endregion

        #region Event handlers

        private void OnCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OnCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var element = (DockElement)Content;
            element.DockControl?.Hide();
        }

        #endregion

        #region Members

        private void MoveTo(int src, int des, Panel parent)
        {
            Container.MoveTo(src, des);
            parent.UpdateLayout();
            _dockViewParent.SelectedIndex = des;
            parent.Children[des].CaptureMouse();
            _dockViewParent.UpdateChildrenBounds(parent);
        }

        #endregion
    }
}