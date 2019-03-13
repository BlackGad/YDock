using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using YDock.Enum;
using YDock.Interface;
using YDock.Model.Element;
using YDock.View.Control;
using YDock.View.Window;

namespace YDock.Model.Layout
{
    public class LayoutGroup : BaseLayoutGroup
    {
        private DockManager _dockManager;

        #region Constructors

        public LayoutGroup(DockSide side, DockMode mode, DockManager dockManager)
        {
            _side = side;
            _mode = mode;
            _dockManager = dockManager;
        }

        #endregion

        #region Properties

        public override DockManager DockManager
        {
            get { return _dockManager; }
        }

        internal AttachObject AttachObj { get; set; }

        #endregion

        #region Override members

        protected override void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.OnChildrenCollectionChanged(sender, e);
            if (_view == null) return;
            var tab = _view as TabControl;
            if (e.NewItems?.Count > 0 && (e.NewItems[e.NewItems.Count - 1] as IDockElement).CanSelect)
            {
                tab.SelectedIndex = IndexOf(e.NewItems[e.NewItems.Count - 1] as IDockElement);
            }
            else
            {
                tab.SelectedIndex = Math.Max(0, tab.SelectedIndex);
            }
        }

        protected override void OnChildrenPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnChildrenPropertyChanged(sender, e);
            if (e.PropertyName == "CanSelect")
            {
                if (_view == null) return;
                if ((sender as DockElement).CanSelect)
                {
                    (_view as TabControl).SelectedIndex = Children_CanSelect.Count() - 1;
                }
                else
                {
                    (_view as TabControl).SelectedIndex = Children_CanSelect.Any() ? 0 : -1;
                }

                if (!Children_CanSelect.Any())
                {
                    _DetachFromParent();
                }
            }
        }

        public override void ShowWithActive(IDockElement element, bool activate = true)
        {
            base.ShowWithActive(element, activate);
            if (_view != null)
            {
                (_view as TabControl).SelectedIndex = IndexOf(element);
            }
            else //_view不存在则要创建新的_view
            {
                if (AttachObj == null || !AttachObj.AttachTo())
                {
                    if (this is LayoutDocumentGroup)
                    {
                        var _children = Children.ToList();
                        _children.Reverse();
                        var dockManager = _dockManager;
                        Dispose();
                        foreach (var child in _children)
                        {
                            dockManager.Root.DocumentModels[0].Attach(child);
                        }
                    }
                    else
                    {
                        var ctrl = new AnchorSideGroupControl(this);
                        switch (Side)
                        {
                            case DockSide.Left:
                                _dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(ctrl, AttachMode.Left, 0);
                                break;
                            case DockSide.Right:
                                _dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(ctrl, AttachMode.Right, _dockManager.LayoutRootPanel.RootGroupPanel.Count);
                                break;
                            case DockSide.Top:
                                _dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(ctrl, AttachMode.Top, 0);
                                break;
                            case DockSide.Bottom:
                                _dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(ctrl, AttachMode.Bottom, _dockManager.LayoutRootPanel.RootGroupPanel.Count);
                                break;
                        }
                    }
                }
            }
        }

        public override void Detach(IDockElement element)
        {
            base.Detach(element);
            //保存Size信息
            if (_view != null)
            {
                var bgc = _view as BaseGroupControl;
                if (bgc.IsInitialized && bgc.ActualHeight >= Constants.SideLength && bgc.ActualWidth >= Constants.SideLength)
                {
                    (element as DockElement).DesiredHeight = bgc.ActualHeight;
                    (element as DockElement).DesiredWidth = bgc.ActualWidth;
                }

                //如果Children_CanSelect数量为0，且Container不是LayoutDocumentGroup，则尝试将view从界面移除
                if (!Children_CanSelect.Any()) //如果Children_CanSelect数量为0
                {
                    _DetachFromParent();
                }
            }
        }

        public override void Attach(IDockElement element, int index = -1)
        {
            if (!element.Side.Assert())
            {
                throw new ArgumentException("Side is illegal!");
            }

            base.Attach(element, index);
        }

        public override void ToFloat()
        {
            BaseFloatWindow wnd;
            BaseGroupControl ctrl;
            BaseLayoutGroup group;
            var dockManager = _dockManager;
            var children = _children.ToList();
            var element = children.First();
            //hide all first
            foreach (var child in children)
            {
                Detach(child);
            }

            if (this is LayoutDocumentGroup)
            {
                group = new LayoutDocumentGroup(DockMode.Float, dockManager);
                foreach (var child in children)
                {
                    group.Attach(child);
                }

                ctrl = new LayoutDocumentGroupControl(group) { DesiredHeight = element.DesiredHeight, DesiredWidth = element.DesiredWidth };
                wnd = new DocumentGroupWindow(dockManager)
                {
                    Height = element.DesiredHeight,
                    Width = element.DesiredWidth,
                    Left = element.FloatLeft,
                    Top = element.FloatTop
                };
            }
            else
            {
                group = new LayoutGroup(_side, DockMode.Float, dockManager);
                foreach (var child in children)
                {
                    group.Attach(child);
                }

                ctrl = new AnchorSideGroupControl(group) { DesiredHeight = element.DesiredHeight, DesiredWidth = element.DesiredWidth };
                wnd = new AnchorGroupWindow(dockManager)
                {
                    Height = element.DesiredHeight,
                    Width = element.DesiredWidth,
                    Left = element.FloatLeft,
                    Top = element.FloatTop
                };
            }

            wnd.AttachChild(ctrl, AttachMode.None, 0);
            wnd.Show();

            dockManager.ActiveControl.SetActive();
        }

        public override void Dispose()
        {
            AttachObj?.Dispose();
            AttachObj = null;
            if (_view != null)
            {
                _dockManager.DragManager.OnDragStatusChanged -= (_view as BaseGroupControl).OnDragStatusChanged;
            }

            base.Dispose();
            _dockManager = null;
        }

        #endregion

        #region Members

        private void _DetachFromParent()
        {
            if ((_view as ILayoutGroupControl).TryDetachFromParent())
            {
                _view = null;
                if (_children.Count == 0)
                {
                    Dispose();
                }
            }
        }

        #endregion
    }
}