using System;

namespace YDock.Interface
{
    public interface INotifyDisposable
    {
        #region Events

        event EventHandler Disposed;

        #endregion
    }
}