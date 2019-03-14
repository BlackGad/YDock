using YDock.Interface;

namespace YDock.View.Render
{
    public class RootDropPanel : BaseDropPanel
    {
        #region Constructors

        internal RootDropPanel(IDragTarget target, DragItem source) : base(target, source)
        {
            //Draw the drag area on the left
            AddChild(new UnitDropVisual(DragManagerFlags.Left));
            //Draw the top drop zone
            AddChild(new UnitDropVisual(DragManagerFlags.Top));
            //Draw the drag area on the right
            AddChild(new UnitDropVisual(DragManagerFlags.Right));
            //Draw the bottom drop zone
            AddChild(new UnitDropVisual(DragManagerFlags.Bottom));
        }

        #endregion
    }
}