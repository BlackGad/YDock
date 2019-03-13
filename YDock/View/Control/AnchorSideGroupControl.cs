using System;
using System.Windows;
using YDock.Enum;
using YDock.Interface;
using YDock.View.Layout;
using YDock.View.Window;

namespace YDock.View.Control
{
    public class AnchorSideGroupControl : BaseGroupControl
    {
        #region Constructors

        static AnchorSideGroupControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnchorSideGroupControl), new FrameworkPropertyMetadata(typeof(AnchorSideGroupControl)));
            FocusableProperty.OverrideMetadata(typeof(AnchorSideGroupControl), new FrameworkPropertyMetadata(false));
        }

        internal AnchorSideGroupControl(ILayoutGroup model,
                                        double desiredWidth = Constants.DockDefaultWidthLength,
                                        double desiredHeight = Constants.DockDefaultHeightLength) : base(model, desiredWidth, desiredHeight)
        {
        }

        #endregion

        #region Properties

        public override DragMode Mode
        {
            get { return DragMode.Anchor; }
        }

        #endregion

        #region Override members

        public override void OnDrop(DragItem source)
        {
            if (DropMode == DropMode.Left
                || DropMode == DropMode.Right
                || DropMode == DropMode.Top
                || DropMode == DropMode.Bottom)
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

                DockManager.ChangeDockMode(child, ((ILayoutGroup)Model).Mode);
                DockManager.ChangeSide(child, Model.Side);

                LayoutGroupPanel panel;
                if (DockViewParent == null)
                {
                    var wnd = (BaseFloatWindow)Parent;
                    wnd.DetachChild(this);
                    panel = new LayoutGroupPanel(Model.Side)
                    {
                        Direction = DropMode == DropMode.Left || DropMode == DropMode.Right ? Direction.Horizontal : Direction.Vertical,
                        DesiredWidth = wnd.ActualWidth,
                        DesiredHeight = wnd.ActualHeight,
                        IsAnchorPanel = true
                    };
                    wnd.DockManager = DockManager;
                    wnd.AttachChild(panel, AttachMode.None, 0);
                    panel._AttachChild(this, 0);
                }
                else
                {
                    panel = DockViewParent as LayoutGroupPanel;
                }

                AttachTo(panel, child, DropMode);
            }
            else
            {
                base.OnDrop(source);
            }

            if (source.RelativeObj is BaseFloatWindow floatWindow)
            {
                floatWindow.Close();
            }
        }

        #endregion

        #region Members

        public void AttachTo(LayoutGroupPanel panel, IDockView source, DropMode mode)
        {
            var index = panel.Children.IndexOf(this);
            switch (mode)
            {
                case DropMode.Left:
                    if (panel.Direction == Direction.Vertical)
                    {
                        var subPanel = new LayoutGroupPanel(Model.Side)
                        {
                            Direction = Direction.Horizontal,
                            DesiredWidth = Math.Max(ActualWidth, Constants.DockDefaultWidthLength),
                            DesiredHeight = Math.Max(ActualHeight, Constants.DockDefaultHeightLength),
                            IsAnchorPanel = true
                        };
                        panel._DetachChild(this);
                        panel._AttachChild(subPanel, Math.Min(index, panel.Count));
                        subPanel._AttachChild(this, 0);
                        subPanel.AttachChild(source, AttachMode.Left, 0);
                    }
                    else
                    {
                        panel._AttachChild(source, index);
                    }

                    break;
                case DropMode.Top:
                    if (panel.Direction == Direction.Horizontal)
                    {
                        var subPanel = new LayoutGroupPanel(Model.Side)
                        {
                            Direction = Direction.Vertical,
                            DesiredWidth = Math.Max(ActualWidth, Constants.DockDefaultWidthLength),
                            DesiredHeight = Math.Max(ActualHeight, Constants.DockDefaultHeightLength),
                            IsAnchorPanel = true
                        };
                        panel._DetachChild(this);
                        panel._AttachChild(subPanel, Math.Min(index, panel.Count));
                        subPanel._AttachChild(this, 0);
                        subPanel.AttachChild(source, AttachMode.Top, 0);
                    }
                    else
                    {
                        panel._AttachChild(source, index);
                    }

                    break;
                case DropMode.Right:
                    if (panel.Direction == Direction.Vertical)
                    {
                        var subPanel = new LayoutGroupPanel(Model.Side)
                        {
                            Direction = Direction.Horizontal,
                            DesiredWidth = Math.Max(ActualWidth, Constants.DockDefaultWidthLength),
                            DesiredHeight = Math.Max(ActualHeight, Constants.DockDefaultHeightLength),
                            IsAnchorPanel = true
                        };
                        panel._DetachChild(this);
                        subPanel._AttachChild(this, 0);
                        subPanel.AttachChild(source, AttachMode.Right, 1);
                        panel._AttachChild(subPanel, Math.Min(index, panel.Count));
                    }
                    else
                    {
                        panel._AttachChild(source, index + 1);
                    }

                    break;
                case DropMode.Bottom:
                    if (panel.Direction == Direction.Horizontal)
                    {
                        var subPanel = new LayoutGroupPanel(Model.Side)
                        {
                            Direction = Direction.Vertical,
                            DesiredWidth = Math.Max(ActualWidth, Constants.DockDefaultWidthLength),
                            DesiredHeight = Math.Max(ActualHeight, Constants.DockDefaultHeightLength),
                            IsAnchorPanel = true
                        };
                        panel._DetachChild(this);
                        subPanel._AttachChild(this, 0);
                        subPanel.AttachChild(source, AttachMode.Bottom, 1);
                        panel._AttachChild(subPanel, Math.Min(index, panel.Count));
                    }
                    else
                    {
                        panel._AttachChild(source, index + 1);
                    }

                    break;
            }
        }

        #endregion
    }
}