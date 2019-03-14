using System.Windows;

namespace YDock.View.Render
{
    public abstract class BaseDropVisual : BaseVisual
    {
        #region Constructors

        internal BaseDropVisual(DragManagerFlags flag)
        {
            Flag = flag;
        }

        #endregion

        #region Properties

        public BaseDropPanel DropPanel
        {
            get { return VisualParent as BaseDropPanel; }
        }

        internal DragManagerFlags Flag { get; set; }

        #endregion

        #region Members

        public void Update()
        {
            Update(new Size(DropPanel.ActualWidth, DropPanel.ActualHeight));
        }

        #endregion
    }
}