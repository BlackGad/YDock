using System.Windows;
using YDock.Model.Element;

namespace YDock.View.Render
{
    public class TextureHeaderVisual : BaseVisual
    {
        #region Override members

        public override void Update(Size size)
        {
            var vOffset = (size.Height - 4) / 2;
            using (var ctx = RenderOpen())
            {
                var model = ((FrameworkElement)VisualParent).DataContext as DockElement;
                if (model == null) return;
                if (model.IsActive)
                {
                    ctx.DrawLine(ResourceManager.ActiveDashPen, new Point(0, vOffset), new Point(size.Width, vOffset));
                    ctx.DrawLine(ResourceManager.ActiveDashPen, new Point(2, vOffset + 2), new Point(size.Width, vOffset + 2));
                    ctx.DrawLine(ResourceManager.ActiveDashPen, new Point(0, vOffset + 4), new Point(size.Width, vOffset + 4));
                }
                else
                {
                    ctx.DrawLine(ResourceManager.DisActiveDashPen, new Point(0, vOffset), new Point(size.Width, vOffset));
                    ctx.DrawLine(ResourceManager.DisActiveDashPen, new Point(2, vOffset + 2), new Point(size.Width, vOffset + 2));
                    ctx.DrawLine(ResourceManager.DisActiveDashPen, new Point(0, vOffset + 4), new Point(size.Width, vOffset + 4));
                }
            }
        }

        #endregion
    }
}