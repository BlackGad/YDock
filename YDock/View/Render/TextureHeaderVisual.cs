using System.Windows;
using YDock.Model.Element;

namespace YDock.View.Render
{
    public class TextureHeaderVisual : BaseVisual
    {
        #region Override members

        public override void Update(Size size)
        {
            var voffset = (size.Height - 4) / 2;
            using (var ctx = RenderOpen())
            {
                var model = (VisualParent as FrameworkElement).DataContext as DockElement;
                if (model == null) return;
                if (model.IsActive)
                {
                    ctx.DrawLine(ResourceManager.ActiveDashPen, new Point(0, voffset), new Point(size.Width, voffset));
                    ctx.DrawLine(ResourceManager.ActiveDashPen, new Point(2, voffset + 2), new Point(size.Width, voffset + 2));
                    ctx.DrawLine(ResourceManager.ActiveDashPen, new Point(0, voffset + 4), new Point(size.Width, voffset + 4));
                }
                else
                {
                    ctx.DrawLine(ResourceManager.DisActiveDashPen, new Point(0, voffset), new Point(size.Width, voffset));
                    ctx.DrawLine(ResourceManager.DisActiveDashPen, new Point(2, voffset + 2), new Point(size.Width, voffset + 2));
                    ctx.DrawLine(ResourceManager.DisActiveDashPen, new Point(0, voffset + 4), new Point(size.Width, voffset + 4));
                }
            }
        }

        #endregion
    }
}