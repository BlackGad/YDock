using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using YDock.Enum;
using YDock.Interface;
using YDock.Model.Element;

namespace YDock.View.Window
{
    public class _AutoHideWindow : HwndHost,
                                   ILayout
    {
        private bool _contentRendered;
        private AutoHideWindow _innerContent;

        private HwndSource _innerSource;
        private IntPtr _parentWindowHandle;

        #region Constructors

        public _AutoHideWindow()
        {
            _innerContent = new AutoHideWindow();
        }

        #endregion

        #region Properties

        public DockElement Model
        {
            get { return _innerContent.Model; }
            set { _innerContent.Model = value; }
        }

        #endregion

        #region Override members

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            _parentWindowHandle = hwndParent.Handle;
            _innerSource = new HwndSource(new HwndSourceParameters("static")
            {
                ParentWindow = hwndParent.Handle,
                WindowStyle = Win32Helper.WS_CHILD | Win32Helper.WS_VISIBLE | Win32Helper.WS_CLIPSIBLINGS | Win32Helper.WS_CLIPCHILDREN,
                Width = 0,
                Height = 0
            });

            _contentRendered = false;
            _innerSource.ContentRendered += _OnContentRendered;
            _innerSource.RootVisual = _innerContent;
            AddLogicalChild(_innerContent);
            Win32Helper.BringWindowToTop(_innerSource.Handle);
            return new HandleRef(this, _innerSource.Handle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            Win32Helper.DestroyWindow(_innerSource.Handle);
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32Helper.WM_WINDOWPOSCHANGING && _contentRendered)
            {
                Win32Helper.BringWindowToTop(_innerSource.Handle);
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _innerContent.Measure(constraint);
            return _innerContent.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _innerContent.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _innerSource.ContentRendered -= _OnContentRendered;
                _innerContent.Dispose();
                _innerContent = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region ILayout Members

        public DockManager DockManager
        {
            get { return _innerContent.DockManager; }
        }

        public DockSide Side
        {
            get { return _innerContent.Side; }
        }

        #endregion

        #region Event handlers

        private void _OnContentRendered(object sender, EventArgs e)
        {
            _contentRendered = true;
        }

        #endregion
    }
}