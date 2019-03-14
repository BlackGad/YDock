using YDock.Interface;

namespace YDock.View.Render
{
    public class RootDropPanel : BaseDropPanel
    {
        #region Constructors

        internal RootDropPanel(IDragTarget target, DragItem source) : base(target, source)
        {
            //Draw the drag area on the left
            AddChild(new UnitDropVisual(DragManager.LEFT));
            //Draw the top drop zone
            AddChild(new UnitDropVisual(DragManager.Top));
            //Draw the drag area on the right
            AddChild(new UnitDropVisual(DragManager.Right));
            //Draw the bottom drop zone
            AddChild(new UnitDropVisual(DragManager.Bottom));
        }

        #endregion
    }
}