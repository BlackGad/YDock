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

        internal int Index { get; }

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
            var anchorSideGroupControl = _relativeObj.View as AnchorSideGroupControl;
            if (anchorSideGroupControl?.TryDetachFromParent(false) != true) return false;

            if (Parent is BaseGroupControl control)
            {
                if (_mode == AttachMode.None)
                {
                    var group = (ILayoutGroup)control.Model;

                    _relativeObj.Dispose();

                    foreach (var child in _relativeObj.Children.Reverse())
                    {
                        group.Attach(child, Math.Min(Index, group.Children.Count - 1));
                    }

                    return true;
                }

                var targetControl = (AnchorSideGroupControl)control;
                if (targetControl.DockViewParent != null)
                {
                    if (_relativeObj.View == null)
                    {
                        _relativeObj.View = new AnchorSideGroupControl(_relativeObj);
                    }

                    if (targetControl.DockViewParent == null) return false;
                    switch (_mode)
                    {
                        case AttachMode.Left:
                            targetControl.AttachTo(targetControl.DockViewParent as LayoutGroupPanel, anchorSideGroupControl, DropMode.Left);
                            break;
                        case AttachMode.Top:
                            targetControl.AttachTo(targetControl.DockViewParent as LayoutGroupPanel, anchorSideGroupControl, DropMode.Top);
                            break;
                        case AttachMode.Right:
                            targetControl.AttachTo(targetControl.DockViewParent as LayoutGroupPanel, anchorSideGroupControl, DropMode.Right);
                            break;
                        case AttachMode.Bottom:
                            targetControl.AttachTo(targetControl.DockViewParent as LayoutGroupPanel, anchorSideGroupControl, DropMode.Bottom);
                            break;
                    }
                }
                else
                {
                    return false;
                }

                Dispose();
            }

            if (Parent is LayoutGroupPanel parentPanel)
            {
                if (_relativeObj.View == null)
                {
                    _relativeObj.View = new AnchorSideGroupControl(_relativeObj);
                }

                if (_mode == AttachMode.None)
                {
                    _relativeObj.Mode = DockMode.Normal;
                    parentPanel.AttachChild(_relativeObj.View, _mode, Math.Min(Index, parentPanel.Children.Count - 1));
                }
                else
                {
                    if (parentPanel.DockViewParent is LayoutGroupPanel panel)
                    {
                        _relativeObj.Mode = DockMode.Normal;
                        panel.AttachChild(_relativeObj.View, _mode, Math.Min(Index, panel.Children.Count - 1));
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