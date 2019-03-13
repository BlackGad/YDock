using YDock.Interface;

namespace YDock.View
{
    public class DropPanel : BaseDropPanel
    {
        #region Constructors

        internal DropPanel(IDragTarget target, DragItem source) : base(target, source)
        {
            //绘制拖放区域的玻璃外观
            AddChild(new GlassDropVisual(DragManager.NONE));
            //绘制中心的拖放区域
            AddChild(new UnitDropVisual(DragManager.CENTER));
            //绘制左边的拖放区域
            AddChild(new UnitDropVisual(DragManager.LEFT));
            //绘制顶部的拖放区域
            AddChild(new UnitDropVisual(DragManager.TOP));
            //绘制右边的拖放区域
            AddChild(new UnitDropVisual(DragManager.RIGHT));
            //绘制底部的拖放区域
            AddChild(new UnitDropVisual(DragManager.BOTTOM));
            //绘制左分割的拖放区域
            AddChild(new UnitDropVisual(DragManager.LEFT | DragManager.SPLIT));
            //绘制上分割的拖放区域
            AddChild(new UnitDropVisual(DragManager.TOP | DragManager.SPLIT));
            //绘制右分割的拖放区域
            AddChild(new UnitDropVisual(DragManager.RIGHT | DragManager.SPLIT));
            //绘制下分割的拖放区域
            AddChild(new UnitDropVisual(DragManager.BOTTOM | DragManager.SPLIT));
        }

        #endregion
    }
}