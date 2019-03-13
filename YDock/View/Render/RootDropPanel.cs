using YDock.Interface;

namespace YDock.View
{
    public class RootDropPanel : BaseDropPanel
    {
        #region Constructors

        internal RootDropPanel(IDragTarget target, DragItem source) : base(target, source)
        {
            //绘制左边的拖放区域
            AddChild(new UnitDropVisual(DragManager.LEFT));
            //绘制顶部的拖放区域
            AddChild(new UnitDropVisual(DragManager.TOP));
            //绘制右边的拖放区域
            AddChild(new UnitDropVisual(DragManager.RIGHT));
            //绘制底部的拖放区域
            AddChild(new UnitDropVisual(DragManager.BOTTOM));
        }

        #endregion
    }
}