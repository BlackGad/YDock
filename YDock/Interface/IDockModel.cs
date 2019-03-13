using System;

namespace YDock.Interface
{
    public interface IDockModel : ILayout,
                                  IDisposable
    {
        #region Properties

        IDockView View { get; }

        #endregion
    }
}