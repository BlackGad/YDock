using YDock.Interface;

namespace YDock.View.Render
{
    public class DropPanel : BaseDropPanel
    {
        #region Constructors

        internal DropPanel(IDragTarget target, DragItem source) : base(target, source)
        {
            //绘制拖放区域的玻璃外观
            AddChild(new GlassDropVisual(DragManager.NONE));
            //绘制中心的拖放区域
            AddChild(new UnitDropVisual(DragManager.Center));
            //绘制左边的拖放区域
            AddChild(new UnitDropVisual(DragManager.LEFT));
            //绘制顶部的拖放区域
            AddChild(new UnitDropVisual(DragManager.Top));
            //绘制右边的拖放区域
            AddChild(new UnitDropVisual(DragManager.Right));
            //绘制底部的拖放区域
            AddChild(new UnitDropVisual(DragManager.Bottom));
            //绘制左分割的拖放区域
            AddChild(new UnitDropVisual(DragManager.LEFT | DragManager.Split));
            //绘制上分割的拖放区域
            AddChild(new UnitDropVisual(DragManager.Top | DragManager.Split));
            //绘制右分割的拖放区域
            AddChild(new UnitDropVisual(DragManager.Right | DragManager.Split));
            //绘制下分割的拖放区域
            AddChild(new UnitDropVisual(DragManager.Bottom | DragManager.Split));
        }

        #endregion
    }
}