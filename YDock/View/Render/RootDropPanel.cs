using YDock.Interface;

namespace YDock.View.Render
{
    public class RootDropPanel : BaseDropPanel
    {
        #region Constructors

        internal RootDropPanel(IDragTarget target, DragItem source) : base(target, source)
        {
            //绘制左边的拖放区域
            AddChild(new UnitDropVisual(DragManager.LEFT));
            //绘制顶部的拖放区域
            AddChild(new UnitDropVisual(DragManager.Top));
            //绘制右边的拖放区域
            AddChild(new UnitDropVisual(DragManager.Right));
            //绘制底部的拖放区域
            AddChild(new UnitDropVisual(DragManager.Bottom));
        }

        #endregion
    }
}