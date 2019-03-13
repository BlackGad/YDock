using System;

namespace YDock
{
    public class DragStatusChangedEventArgs : EventArgs
    {
        #region Constructors

        internal DragStatusChangedEventArgs(bool status)
        {
            IsDragging = status;
        }

        #endregion

        #region Properties

        public bool IsDragging { get; }

        #endregion
    }
}