using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace YDock.View.Control
{
    public class PopupNotTop : Popup
    {
        #region Static members

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        private static void OnTopmostChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as PopupNotTop).UpdateWindow();
        }

        [DllImport("user32", EntryPoint = "SetWindowPos")]
        private static extern int SetWindowPos(IntPtr hWnd, int hwndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        #endregion

        #region Properties

        public bool Topmost
        {
            get { return (bool)GetValue(TopmostProperty); }
            set { SetValue(TopmostProperty, value); }
        }

        #endregion

        #region Override members

        protected override void OnOpened(EventArgs e)
        {
            UpdateWindow();
        }

        #endregion

        #region Members

        private void UpdateWindow()
        {
            var hwnd = ((HwndSource)PresentationSource.FromDependencyObject(this)).Handle;
            RECT rect;
            if (GetWindowRect(hwnd, out rect))
            {
                SetWindowPos(hwnd, Topmost ? -1 : -2, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, 0);
            }
        }

        #endregion

        public static DependencyProperty TopmostProperty = System.Windows.Window.TopmostProperty.AddOwner(typeof(PopupNotTop), new FrameworkPropertyMetadata(false, OnTopmostChanged));

        #region Nested type: RECT

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion
    }
}