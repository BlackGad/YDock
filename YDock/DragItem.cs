using System;
using System.Windows;
using YDock.Enum;

namespace YDock
{
    public class DragItem : IDisposable
    {
        #region Constructors

        internal DragItem(object relativeObj, DockMode dockMode, DragMode dragMode, Point clickPos, Rect clickRect, Size size)
        {
            RelativeObject = relativeObj;
            DockMode = dockMode;
            DragMode = dragMode;
            ClickPos = clickPos;
            ClickRect = clickRect;
            Size = size;
        }

        #endregion

        #region Properties

        public Point ClickPos { get; }

        public Rect ClickRect { get; }

        /// <summary>
        ///     Mode before dragging
        /// </summary>
        public DockMode DockMode { get; }

        public DragMode DragMode { get; }

        public object RelativeObject { get; private set; }

        public Size Size { get; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            RelativeObject = null;
        }

        #endregion
    }
}