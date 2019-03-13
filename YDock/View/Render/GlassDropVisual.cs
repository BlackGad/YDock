using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using YDock.Enum;

namespace YDock.View.Render
{
    public class GlassDropVisual : BaseDropVisual
    {
        #region Constructors

        internal GlassDropVisual(int flag) : base(flag)
        {
        }

        #endregion

        #region Override members

        public override void Update(Size size)
        {
            using (var ctx = RenderOpen())
            {
                double hoffset = 0, voffset = 0, sideLength = 0;
                if (DropPanel.Target.Mode == DragMode.Document
                    && DropPanel.Source.DragMode == DragMode.Anchor)
                {
                    hoffset = DropPanel.InnerRect.Size.Width / 2 - Constants.DropUnitLength * 5 / 2 + DropPanel.InnerRect.Left + DropPanel.OuterRect.Left;
                    voffset = DropPanel.InnerRect.Size.Height / 2 - Constants.DropUnitLength / 2 + DropPanel.InnerRect.Top + DropPanel.OuterRect.Top;
                    sideLength = Constants.DropUnitLength * 2 - Constants.DropCornerLength;
                }
                else if (DropPanel.Target.Mode == DragMode.None || DropPanel.Source.DragMode == DragMode.None || DropPanel.Target.Mode == DragMode.Anchor
                         && DropPanel.Source.DragMode == DragMode.Document)
                {
                    return;
                }
                else
                {
                    hoffset = DropPanel.InnerRect.Size.Width / 2 - Constants.DropUnitLength * 3 / 2 + DropPanel.InnerRect.Left + DropPanel.OuterRect.Left;
                    voffset = DropPanel.InnerRect.Size.Height / 2 - Constants.DropUnitLength / 2 + DropPanel.InnerRect.Top + DropPanel.OuterRect.Top;
                    sideLength = Constants.DropUnitLength - Constants.DropCornerLength;
                }

                ctx.PushOpacity(Constants.DragOpacity);
                var points = new List<Point>();
                double currentX = hoffset, currentY = voffset;

                points.Add(new Point(currentX += sideLength, currentY));
                points.Add(new Point(currentX += Constants.DropCornerLength, currentY -= Constants.DropCornerLength));
                points.Add(new Point(currentX, currentY -= sideLength));
                points.Add(new Point(currentX += Constants.DropUnitLength, currentY));
                points.Add(new Point(currentX, currentY += sideLength));
                points.Add(new Point(currentX += Constants.DropCornerLength, currentY += Constants.DropCornerLength));
                points.Add(new Point(currentX += sideLength, currentY));
                points.Add(new Point(currentX, currentY += Constants.DropUnitLength));
                points.Add(new Point(currentX -= sideLength, currentY));
                points.Add(new Point(currentX -= Constants.DropCornerLength, currentY += Constants.DropCornerLength));
                points.Add(new Point(currentX, currentY += sideLength));
                points.Add(new Point(currentX -= Constants.DropUnitLength, currentY));
                points.Add(new Point(currentX, currentY -= sideLength));
                points.Add(new Point(currentX -= Constants.DropCornerLength, currentY -= Constants.DropCornerLength));
                points.Add(new Point(currentX -= sideLength, currentY));
                points.Add(new Point(currentX, currentY -= Constants.DropUnitLength));

                var stream = new StreamGeometry();
                using (var sctx = stream.Open())
                {
                    sctx.BeginFigure(new Point(currentX, currentY), true, true);
                    sctx.PolyLineTo(points, true, true);
                    sctx.Close();
                }

                ctx.DrawGeometry(Brushes.White, ResourceManager.BorderPen, stream);
            }
        }

        #endregion
    }
}