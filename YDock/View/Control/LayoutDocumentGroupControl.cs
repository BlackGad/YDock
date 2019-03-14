using System;
using System.Windows;
using YDock.Enum;
using YDock.Interface;
using YDock.View.Layout;
using YDock.View.Window;

namespace YDock.View.Control
{
    public class LayoutDocumentGroupControl : BaseGroupControl
    {
        #region Constructors

        static LayoutDocumentGroupControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutDocumentGroupControl), new FrameworkPropertyMetadata(typeof(LayoutDocumentGroupControl)));
            FocusableProperty.OverrideMetadata(typeof(LayoutDocumentGroupControl), new FrameworkPropertyMetadata(false));
        }

        internal LayoutDocumentGroupControl(ILayoutGroup model,
                                            double desiredWidth = Constants.DockDefaultWidthLength,
                                            double desiredHeight = Constants.DockDefaultHeightLength) : base(model, desiredWidth, desiredHeight)
        {
        }

        #endregion

        #region Properties

        public override DragMode Mode
        {
            get { return DragMode.Document; }
        }

        #endregion

        #region Override members

        public override void OnDrop(DragItem source)
        {
            if (DropMode == DropMode.Header
                || DropMode == DropMode.Center)
            {
                base.OnDrop(source);
            }
            else
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

                if (AssertSplitMode(DropMode))
                {
                    //must to change side
                    DockManager.ChangeSide(child, Model.Side);

                    if (DockViewParent == null)
                    {
                        var parent = (BaseFloatWindow)Parent;
                        parent.DetachChild(this, false);
                        var panel = new LayoutGroupDocumentPanel
                        {
                            DesiredWidth = Math.Max(parent.ActualWidth, Constants.DockDefaultWidthLength),
                            DesiredHeight = Math.Max(parent.ActualHeight, Constants.DockDefaultHeightLength),
                            Direction = DropMode == DropMode.Left_WithSplit || DropMode == DropMode.Right_WithSplit ? Direction.Horizontal : Direction.Vertical
                        };
                        panel.AttachChild(this, 0);
                        if (DropMode == DropMode.Left_WithSplit || DropMode == DropMode.Top_WithSplit)
                        {
                            panel.AttachChild(child, DropMode == DropMode.Left_WithSplit ? AttachMode.Left_WithSplit : AttachMode.Top_WithSplit, 0);
                        }
                        else
                        {
                            panel.AttachChild(child, DropMode == DropMode.Right_WithSplit ? AttachMode.Right_WithSplit : AttachMode.Bottom_WithSplit, 1);
                        }

                        parent.AttachChild(panel, AttachMode.None, 0);
                    }
                    else
                    {
                        var parent = (LayoutGroupDocumentPanel)Parent;
                        parent.Direction = DropMode == DropMode.Left_WithSplit || DropMode == DropMode.Right_WithSplit ? Direction.Horizontal : Direction.Vertical;
                        var index = parent.IndexOf(this);
                        switch (DropMode)
                        {
                            case DropMode.Left_WithSplit:
                                parent.AttachChild(child, AttachMode.Left_WithSplit, index);
                                break;
                            case DropMode.Top_WithSplit:
                                parent.AttachChild(child, AttachMode.Top_WithSplit, index);
                                break;
                            case DropMode.Right_WithSplit:
                                parent.AttachChild(child, AttachMode.Right_WithSplit, index + 1);
                                break;
                            case DropMode.Bottom_WithSplit:
                                parent.AttachChild(child, AttachMode.Bottom_WithSplit, index + 1);
                                break;
                        }
                    }
                }
                else
                {
                    DockManager.FormatChildSize(child as ILayoutSize, new Size(ActualWidth, ActualHeight));

                    var parent = (LayoutGroupDocumentPanel)Parent;
                    if (parent.DockViewParent is LayoutRootPanel rootPanel)
                    {
                        rootPanel.DetachChild(parent, false);
                        var parentPanel = new LayoutGroupPanel
                        {
                            Direction = DropMode == DropMode.Left || DropMode == DropMode.Right ? Direction.Horizontal : Direction.Vertical
                        };
                        parentPanel.AttachChild(parent, 0);
                        switch (DropMode)
                        {
                            case DropMode.Left:
                                DockManager.ChangeSide(child, DockSide.Left);
                                parentPanel.AttachChild(child, AttachMode.Left, 0);
                                break;
                            case DropMode.Top:
                                DockManager.ChangeSide(child, DockSide.Top);
                                parentPanel.AttachChild(child, AttachMode.Top, 0);
                                break;
                            case DropMode.Right:
                                DockManager.ChangeSide(child, DockSide.Right);
                                parentPanel.AttachChild(child, AttachMode.Right, 1);
                                break;
                            case DropMode.Bottom:
                                DockManager.ChangeSide(child, DockSide.Bottom);
                                parentPanel.AttachChild(child, AttachMode.Bottom, 1);
                                break;
                        }

                        rootPanel.AttachChild(parentPanel, AttachMode.None, 0);
                    }
                    else
                    {
                        var panel = (LayoutGroupPanel)parent.DockViewParent;
                        var index = panel.IndexOf(parent);
                        switch (DropMode)
                        {
                            case DropMode.Left:
                                DockManager.ChangeSide(child, DockSide.Left);
                                if (panel.Direction == Direction.Horizontal)
                                {
                                    panel.AttachChild(child, AttachMode.Left, index);
                                }
                                else
                                {
                                    panel.InternalDetachChild(parent);
                                    var parentPanel = new LayoutGroupPanel
                                    {
                                        Direction = Direction.Horizontal
                                    };
                                    parentPanel.AttachChild(parent, 0);
                                    parentPanel.AttachChild(child, 0);
                                    panel.AttachChild(parentPanel, Math.Min(index, panel.Count));
                                }

                                break;
                            case DropMode.Top:
                                DockManager.ChangeSide(child, DockSide.Top);
                                if (panel.Direction == Direction.Vertical)
                                {
                                    panel.AttachChild(child, AttachMode.Top, index);
                                }
                                else
                                {
                                    panel.InternalDetachChild(parent);
                                    var parentPanel = new LayoutGroupPanel
                                    {
                                        Direction = Direction.Vertical
                                    };
                                    parentPanel.AttachChild(parent, 0);
                                    parentPanel.AttachChild(child, 0);
                                    panel.AttachChild(parentPanel, Math.Min(index, panel.Count));
                                }

                                break;
                            case DropMode.Right:
                                DockManager.ChangeSide(child, DockSide.Right);
                                if (panel.Direction == Direction.Horizontal)
                                {
                                    panel.AttachChild(child, AttachMode.Right, index + 1);
                                }
                                else
                                {
                                    panel.InternalDetachChild(parent);
                                    var parentPanel = new LayoutGroupPanel
                                    {
                                        Direction = Direction.Horizontal
                                    };
                                    parentPanel.AttachChild(parent, 0);
                                    parentPanel.AttachChild(child, 1);
                                    panel.AttachChild(parentPanel, Math.Min(index, panel.Count));
                                }

                                break;
                            case DropMode.Bottom:
                                DockManager.ChangeSide(child, DockSide.Bottom);
                                if (panel.Direction == Direction.Vertical)
                                {
                                    panel.AttachChild(child, AttachMode.Bottom, index + 1);
                                }
                                else
                                {
                                    panel.InternalDetachChild(parent);
                                    var parentPanel = new LayoutGroupPanel
                                    {
                                        Direction = Direction.Vertical
                                    };
                                    parentPanel.AttachChild(parent, 0);
                                    parentPanel.AttachChild(child, 1);
                                    panel.AttachChild(parentPanel, Math.Min(index, panel.Count));
                                }

                                break;
                        }
                    }
                }
            }

            if (source.RelativeObj is BaseFloatWindow floatWindow)
            {
                floatWindow.Close();
            }
        }

        #endregion

        #region Members

        private bool AssertSplitMode(DropMode mode)
        {
            return mode == DropMode.Left_WithSplit
                   || mode == DropMode.Right_WithSplit
                   || mode == DropMode.Top_WithSplit
                   || mode == DropMode.Bottom_WithSplit;
        }

        #endregion
    }
}