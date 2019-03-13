using System.Windows;
using YDock.Enum;

namespace YDock.Interface
{
    public interface IDragTarget : IAttach
    {
        #region Properties

        DockManager DockManager { get; }
        DropMode DropMode { get; set; }
        DragMode Mode { get; }

        #endregion

        #region Members

        void CloseDropWindow();
        void HideDropWindow();
        void OnDrop(DragItem source);
        void ShowDropWindow();
        void Update(Point mouseP);

        #endregion
    }
}