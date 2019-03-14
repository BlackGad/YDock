using YDock.Interface;

namespace YDock.View.Render
{
    public class DropPanel : BaseDropPanel
    {
        #region Constructors

        internal DropPanel(IDragTarget target, DragItem source) : base(target, source)
        {
            //Draw the glass appearance of the drop zone
            AddChild(new GlassDropVisual(DragManagerFlags.None));
            //Draw the center's drop zone
            AddChild(new UnitDropVisual(DragManagerFlags.Center));
            //Draw the drag area on the left
            AddChild(new UnitDropVisual(DragManagerFlags.Left));
            //Draw the top drop zone
            AddChild(new UnitDropVisual(DragManagerFlags.Top));
            //Draw the drag area on the right
            AddChild(new UnitDropVisual(DragManagerFlags.Right));
            //Draw the bottom drop zone
            AddChild(new UnitDropVisual(DragManagerFlags.Bottom));
            //Draw a left-handed drop zone
            AddChild(new UnitDropVisual(DragManagerFlags.Left | DragManagerFlags.Split));
            //Draw the drop zone on the split
            AddChild(new UnitDropVisual(DragManagerFlags.Top | DragManagerFlags.Split));
            //Draw a right-handed drop zone
            AddChild(new UnitDropVisual(DragManagerFlags.Right | DragManagerFlags.Split));
            //Draw the drop zone for the next split
            AddChild(new UnitDropVisual(DragManagerFlags.Bottom | DragManagerFlags.Split));
        }

        #endregion
    }
}