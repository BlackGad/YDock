using YDock.Interface;

namespace YDock.View.Render
{
    public class DropPanel : BaseDropPanel
    {
        #region Constructors

        internal DropPanel(IDragTarget target, DragItem source) : base(target, source)
        {
            //Draw the glass appearance of the drop zone
            AddChild(new GlassDropVisual(DragManager.NONE));
            //Draw the center's drop zone
            AddChild(new UnitDropVisual(DragManager.Center));
            //Draw the drag area on the left
            AddChild(new UnitDropVisual(DragManager.LEFT));
            //Draw the top drop zone
            AddChild(new UnitDropVisual(DragManager.Top));
            //Draw the drag area on the right
            AddChild(new UnitDropVisual(DragManager.Right));
            //Draw the bottom drop zone
            AddChild(new UnitDropVisual(DragManager.Bottom));
            //Draw a left-handed drop zone
            AddChild(new UnitDropVisual(DragManager.LEFT | DragManager.Split));
            //Draw the drop zone on the split
            AddChild(new UnitDropVisual(DragManager.Top | DragManager.Split));
            //Draw a right-handed drop zone
            AddChild(new UnitDropVisual(DragManager.Right | DragManager.Split));
            //Draw the drop zone for the next split
            AddChild(new UnitDropVisual(DragManager.Bottom | DragManager.Split));
        }

        #endregion
    }
}