using YDock.Enum;

namespace YDock.Interface
{
    public interface ILayout
    {
        #region Properties

        DockManager DockManager { get; }
        DockSide Side { get; }

        #endregion
    }
}