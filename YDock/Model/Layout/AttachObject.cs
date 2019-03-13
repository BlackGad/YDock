using System;
using System.Linq;
using YDock.Enum;
using YDock.Interface;
using YDock.View.Control;
using YDock.View.Layout;

namespace YDock.Model.Layout
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
            if (Parent is BaseGroupControl control)
            {
                if (_mode == AttachMode.None)
                {
                    var group = control.Model as ILayoutGroup;
                    var ctrl = _relativeObj.View as AnchorSideGroupControl;
                    if (ctrl == null) return false;
                    if (ctrl.TryDeatchFromParent(false))
                    {
                        var children = _relativeObj.Children.ToList();
                        children.Reverse();
                        _relativeObj.Dispose();
                        foreach (var child in children)
                        {
                            group.Attach(child, Math.Min(Index, group.Children.Count() - 1));
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var targetControl = (AnchorSideGroupControl)control;
                    if (targetControl.DockViewParent != null)
                    {
                        if (_relativeObj.View == null)
                        {
                            _relativeObj.View = new AnchorSideGroupControl(_relativeObj);
                        }

                        var ctrl = _relativeObj.View as AnchorSideGroupControl;
                        if (ctrl == null) return false;
                        if (ctrl.TryDeatchFromParent(false))
                        {
                            if (targetControl.DockViewParent == null) return false;
                            switch (_mode)
                            {
                                case AttachMode.Left:
                                    targetControl.AttachTo(targetControl.DockViewParent as LayoutGroupPanel, ctrl, DropMode.Left);
                                    break;
                                case AttachMode.Top:
                                    targetControl.AttachTo(targetControl.DockViewParent as LayoutGroupPanel, ctrl, DropMode.Top);
                                    break;
                                case AttachMode.Right:
                                    targetControl.AttachTo(targetControl.DockViewParent as LayoutGroupPanel, ctrl, DropMode.Right);
                                    break;
                                case AttachMode.Bottom:
                                    targetControl.AttachTo(targetControl.DockViewParent as LayoutGroupPanel, ctrl, DropMode.Bottom);
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
                    var panel = (LayoutGroupPanel)Parent;
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
                    if (((LayoutGroupPanel)Parent).DockViewParent is LayoutGroupPanel panel)
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