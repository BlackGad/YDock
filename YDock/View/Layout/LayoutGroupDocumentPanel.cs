using System;
using System.Collections.Generic;
using System.Linq;
using YDock.Enum;
using YDock.Interface;
using YDock.Model.Layout;
using YDock.View.Control;

namespace YDock.View.Layout
{
    public class LayoutGroupDocumentPanel : LayoutGroupPanel
    {
        #region Constructors

        internal LayoutGroupDocumentPanel()
        {
        }

        #endregion

        #region Override members

        public override void AttachChild(IDockView child, AttachMode mode, int index)
        {
            if (index < 0 || index > Count) throw new ArgumentOutOfRangeException("index out of range!");
            if (!_AssertMode(mode)) throw new ArgumentException("mode is illegal!");

            if (Count == 1 && ActualWidth > 0 && ActualHeight > 0)
            {
                var size = Children[0] as ILayoutSize;
                size.DesiredWidth = ActualWidth;
                size.DesiredHeight = ActualHeight;
            }

            if (child is LayoutDocumentGroupControl
                || child is LayoutGroupDocumentPanel
                || mode == AttachMode.Left_WithSplit
                || mode == AttachMode.Top_WithSplit
                || mode == AttachMode.Right_WithSplit
                || mode == AttachMode.Bottom_WithSplit)
            {
                _AttachWithSplit(child, mode, index);
            }
            else
            {
                if (IsRootPanel)
                {
                    AttachToRootPanel(child, mode);
                }
                else
                {
                    var parent = Parent as LayoutGroupPanel;
                    switch (parent.Direction)
                    {
                        case Direction.Horizontal:
                            if (mode == AttachMode.Left || mode == AttachMode.Right)
                            {
                                if (child is LayoutGroupPanel)
                                {
                                    _AttachSubPanel(child as LayoutGroupPanel, index);
                                }
                                else
                                {
                                    _AttachChild(child, index);
                                }
                            }
                            else
                            {
                                var _index = parent.IndexOf(this);
                                parent._DetachChild(this);
                                var parentParent = new LayoutGroupPanel
                                {
                                    Direction = Direction.Vertical 
                                };
                                parent._AttachChild(parentParent, _index);
                                parentParent._AttachChild(this, 0);
                                parentParent._AttachChild(child, mode == AttachMode.Top ? 0 : 1);
                            }

                            break;
                        case Direction.Vertical:
                            if (mode == AttachMode.Top || mode == AttachMode.Bottom)
                            {
                                if (child is LayoutGroupPanel)
                                {
                                    _AttachSubPanel(child as LayoutGroupPanel, index);
                                }
                                else
                                {
                                    _AttachChild(child, index);
                                }
                            }
                            else
                            {
                                var _index = parent.IndexOf(this);
                                parent._DetachChild(this);
                                var pparent = new LayoutGroupPanel
                                    { Direction = Direction.Horizontal };
                                parent._AttachChild(pparent, _index);
                                pparent._AttachChild(this, 0);
                                pparent._AttachChild(child, mode == AttachMode.Left ? 0 : 1);
                            }

                            break;
                    }
                }
            }
        }

        public override void DetachChild(IDockView child, bool force = true)
        {
            if (!(child is LayoutDocumentGroupControl))
            {
                throw new InvalidOperationException("not support Operation!");
            }

            _DetachChild(child);
            if (DockViewParent != null)
            {
                DockManager.Root.DocumentModels.Remove(child.Model as BaseLayoutGroup);
            }

            if (Count < 2)
            {
                Direction = Direction.None;
            }

            if (Count == 1)
            {
                if (DockViewParent == null)
                {
                    var _child = Children[0];
                    var wnd = Parent as ILayoutViewParent;
                    wnd.DetachChild(this, false);
                    _Dispose();
                    wnd.AttachChild(_child as IDockView, AttachMode.None, 0);
                }
            }
        }

        internal override bool _AssertMode(AttachMode mode)
        {
            return mode == AttachMode.Left
                   || mode == AttachMode.Top
                   || mode == AttachMode.Right
                   || mode == AttachMode.Bottom
                   || mode == AttachMode.Left_WithSplit
                   || mode == AttachMode.Top_WithSplit
                   || mode == AttachMode.Right_WithSplit
                   || mode == AttachMode.Bottom_WithSplit;
        }

        #endregion

        #region Members

        private void _AttachWithSplit(IDockView child, AttachMode mode, int index)
        {
            if (child is LayoutDocumentGroupControl)
            {
                if (Direction == Direction.None)
                {
                    Direction = mode == AttachMode.Left_WithSplit || mode == AttachMode.Right_WithSplit ? Direction.Horizontal : Direction.Vertical;
                }

                _AttachChild(child, index);

                if (DockViewParent != null)
                {
                    DockManager.Root.DocumentModels.Add(child.Model as BaseLayoutGroup);
                }
            }

            if (child is AnchorSideGroupControl)
            {
                var model = (child as AnchorSideGroupControl).Model as LayoutGroup;
                var _children = new List<IDockElement>(model.Children);
                model.Dispose();
                var group = new LayoutDocumentGroup(DockViewParent == null ? DockMode.Float : DockMode.Normal, DockManager);
                foreach (var _child in _children)
                {
                    group.Attach(_child);
                }

                var ctrl = new LayoutDocumentGroupControl(group);
                _AttachChild(ctrl, index);
                child.Dispose();
            }

            if (child is LayoutGroupDocumentPanel
                || child is LayoutGroupPanel)
            {
                var _children = new List<IDockView>((child as LayoutGroupPanel).Children.OfType<IDockView>());
                _children.Reverse();
                (child as LayoutGroupPanel).Children.Clear();
                foreach (var _child in _children)
                {
                    _AttachWithSplit(_child, mode, index);
                }

                child.Dispose();
            }

            //if (child is IDisposable)
            //    (child as IDisposable).Dispose();
        }

        #endregion
    }
}