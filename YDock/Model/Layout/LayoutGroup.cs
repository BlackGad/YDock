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
            if (View is TabControl tab)
            {
                if (e.NewItems?.Count > 0 && ((IDockElement)e.NewItems[e.NewItems.Count - 1]).CanSelect)
                {
                    tab.SelectedIndex = IndexOf(e.NewItems[e.NewItems.Count - 1] as IDockElement);
                }
                else
                {
                    tab.SelectedIndex = Math.Max(0, tab.SelectedIndex);
                }
            }
        }

        protected override void OnChildrenPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnChildrenPropertyChanged(sender, e);

            if (e.PropertyName == "CanSelect" && sender is DockElement dockElement)
            {
                if (View == null) return;
                var tabControl = (TabControl)View;
                if (dockElement.CanSelect)
                {
                    tabControl.SelectedIndex = SelectableChildren.Count - 1;
                }
                else
                {
                    tabControl.SelectedIndex = SelectableChildren.Any() ? 0 : -1;
                }

                if (!SelectableChildren.Any())
                {
                    DetachFromParent();
                }
            }
        }

        public override void ShowWithActive(IDockElement element, bool activate = true)
        {
            base.ShowWithActive(element, activate);
            if (View is TabControl tabControl)
            {
                tabControl.SelectedIndex = IndexOf(element);
                return;
            }

            if (AttachObj != null && AttachObj.AttachTo()) return;

            //If view does not exist, create a new view
            if (this is LayoutDocumentGroup)
            {
                var dockManager = _dockManager;
                Dispose();

                foreach (var child in Children.Reverse())
                {
                    dockManager.Root.DocumentModels[0].Attach(child);
                }
            }
            else
            {
                var control = new AnchorSideGroupControl(this);
                switch (Side)
                {
                    case DockSide.Left:
                        _dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(control, AttachMode.Left, 0);
                        break;
                    case DockSide.Right:
                        _dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(control, AttachMode.Right, _dockManager.LayoutRootPanel.RootGroupPanel.Count);
                        break;
                    case DockSide.Top:
                        _dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(control, AttachMode.Top, 0);
                        break;
                    case DockSide.Bottom:
                        _dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(control, AttachMode.Bottom, _dockManager.LayoutRootPanel.RootGroupPanel.Count);
                        break;
                }
            }
        }

        public override void Detach(IDockElement element)
        {
            base.Detach(element);

            if (View == null) return;

            //Save Size information
            if (View is BaseGroupControl groupControl &&
                groupControl.IsInitialized &&
                groupControl.ActualHeight >= Constants.SideLength &&
                groupControl.ActualWidth >= Constants.SideLength)
            {
                element.DesiredHeight = groupControl.ActualHeight;
                element.DesiredWidth = groupControl.ActualWidth;
            }

            //If the Number of Children Can Select is 0
            //and the Container is not a LayoutDocument Group,
            //try removing the view from the interface.
            if (!SelectableChildren.Any()) //If the number of Children Can Select is 0
            {
                DetachFromParent();
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
            BaseGroupControl control;
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

                control = new LayoutDocumentGroupControl(group) { DesiredHeight = element.DesiredHeight, DesiredWidth = element.DesiredWidth };
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

                control = new AnchorSideGroupControl(group) { DesiredHeight = element.DesiredHeight, DesiredWidth = element.DesiredWidth };
                wnd = new AnchorGroupWindow(dockManager)
                {
                    Height = element.DesiredHeight,
                    Width = element.DesiredWidth,
                    Left = element.FloatLeft,
                    Top = element.FloatTop
                };
            }

            wnd.AttachChild(control, AttachMode.None, 0);
            wnd.Show();

            dockManager.ActiveControl.SetActive();
        }

        public override void Dispose()
        {
            AttachObj?.Dispose();
            AttachObj = null;
            if (View is BaseGroupControl groupControl)
            {
                _dockManager.DragManager.OnDragStatusChanged -= groupControl.OnDragStatusChanged;
            }

            base.Dispose();
            _dockManager = null;
        }

        #endregion

        #region Members

        private void DetachFromParent()
        {
            if (View is ILayoutGroupControl layoutGroupControl && layoutGroupControl.TryDetachFromParent())
            {
                View = null;
                if (!_children.Any()) Dispose();
            }
        }

        #endregion
    }
}