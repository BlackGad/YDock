using System;

namespace YDock.Interface
{
    public interface IDockView : IDisposable
    {
        #region Properties

        IDockView DockViewParent { get; }
        IDockModel Model { get; }

        #endregion
    }
}