using System;
using System.Linq;
using YDock.Enum;
using YDock.Interface;
using YDock.View;

namespace YDock.Model
{
    internal class AttachObject : IDisposable
    {
        private readonly AttachMode _mode;

        private LayoutGroup _relativeObj;

        #region Constructors

        internal AttachObject(LayoutGroup relativeObj, INotifyDisposable parent, int index, AttachMode mode = AttachMode.None)
        {
            _relativeObj = relativeObj;
            Parent = parent;
            Index = index;
            _mode = mode;
            Parent.Disposed += OnDisposed;
        }

        #endregion

        #region Properties

        internal int Index { get; } = -1;

        internal INotifyDisposable Parent { get; private set; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (Parent != null)
            {
                Parent.Disposed -= OnDisposed;
            }

            if (_relativeObj != null)
            {
                _relativeObj.AttachObj = null;
            }

            _relativeObj = null;
            Parent = null;
        }

        #endregion

        #region Event handlers

        private void OnDisposed(object sender, EventArgs e)
        {
            Dispose();
        }

        #endregion

        #region Members

        internal bool AttachTo()
        {
            if (Parent is BaseGroupControl)
            {
                if (_mode == AttachMode.None)
                {
                    var _group = (Parent as BaseGroupControl).Model as ILayoutGroup;
                    var ctrl = _relativeObj.View as AnchorSideGroupControl;
                    if (ctrl == null) return false;
                    if (ctrl.TryDeatchFromParent(false))
                    {
                        var _children = _relativeObj.Children.ToList();
                        _children.Reverse();
                        _relativeObj.Dispose();
                        foreach (var child in _children)
                        {
                            _group.Attach(child, Math.Min(Index, _group.Children.Count() - 1));
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var targetctrl = Parent as AnchorSideGroupControl;
                    if (targetctrl.DockViewParent != null)
                    {
                        if (_relativeObj.View == null)
                        {
                            _relativeObj.View = new AnchorSideGroupControl(_relativeObj);
                        }

                        var ctrl = _relativeObj.View as AnchorSideGroupControl;
                        if (ctrl == null) return false;
                        if (ctrl.TryDeatchFromParent(false))
                        {
                            if (targetctrl.DockViewParent == null) return false;
                            switch (_mode)
                            {
                                case AttachMode.Left:
                                    targetctrl.AttachTo(targetctrl.DockViewParent as LayoutGroupPanel, ctrl, DropMode.Left);
                                    break;
                                case AttachMode.Top:
                                    targetctrl.AttachTo(targetctrl.DockViewParent as LayoutGroupPanel, ctrl, DropMode.Top);
                                    break;
                                case AttachMode.Right:
                                    targetctrl.AttachTo(targetctrl.DockViewParent as LayoutGroupPanel, ctrl, DropMode.Right);
                                    break;
                                case AttachMode.Bottom:
                                    targetctrl.AttachTo(targetctrl.DockViewParent as LayoutGroupPanel, ctrl, DropMode.Bottom);
                                    break;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    Dispose();
                }
            }

            if (Parent is LayoutGroupPanel)
            {
                if (_relativeObj.View == null)
                {
                    _relativeObj.View = new AnchorSideGroupControl(_relativeObj);
                }

                var ctrl = _relativeObj.View as AnchorSideGroupControl;
                if (ctrl == null) return false;
                if (_mode == AttachMode.None)
                {
                    var panel = Parent as LayoutGroupPanel;
                    if (ctrl.TryDeatchFromParent(false))
                    {
                        _relativeObj.Mode = DockMode.Normal;
                        panel.AttachChild(_relativeObj.View, _mode, Math.Min(Index, panel.Children.Count - 1));
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var panel = (Parent as LayoutGroupPanel).DockViewParent as LayoutGroupPanel;
                    if (panel != null)
                    {
                        if (ctrl.TryDeatchFromParent(false))
                        {
                            _relativeObj.Mode = DockMode.Normal;
                            panel.AttachChild(_relativeObj.View, _mode, Math.Min(Index, panel.Children.Count - 1));
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                Dispose();
            }

            return true;
        }

        #endregion
    }
}