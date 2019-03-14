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
                double hOffset, vOffset, sideLength;
                if (DropPanel.Target.Mode == DragMode.Document
                    && DropPanel.Source.DragMode == DragMode.Anchor)
                {
                    hOffset = DropPanel.InnerRect.Size.Width / 2 - Constants.DropUnitLength * 5 / 2 + DropPanel.InnerRect.Left + DropPanel.OuterRect.Left;
                    vOffset = DropPanel.InnerRect.Size.Height / 2 - Constants.DropUnitLength / 2 + DropPanel.InnerRect.Top + DropPanel.OuterRect.Top;
                    sideLength = Constants.DropUnitLength * 2 - Constants.DropCornerLength;
                }
                else if (DropPanel.Target.Mode == DragMode.None || DropPanel.Source.DragMode == DragMode.None || DropPanel.Target.Mode == DragMode.Anchor
                         && DropPanel.Source.DragMode == DragMode.Document)
                {
                    return;
                }
                else
                {
                    hOffset = DropPanel.InnerRect.Size.Width / 2 - Constants.DropUnitLength * 3 / 2 + DropPanel.InnerRect.Left + DropPanel.OuterRect.Left;
                    vOffset = DropPanel.InnerRect.Size.Height / 2 - Constants.DropUnitLength / 2 + DropPanel.InnerRect.Top + DropPanel.OuterRect.Top;
                    sideLength = Constants.DropUnitLength - Constants.DropCornerLength;
                }

                ctx.PushOpacity(Constants.DragOpacity);
                var points = new List<Point>();
                double currentX = hOffset, currentY = vOffset;

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
                using (var context = stream.Open())
                {
                    context.BeginFigure(new Point(currentX, currentY), true, true);
                    context.PolyLineTo(points, true, true);
                    context.Close();
                }

                ctx.DrawGeometry(Brushes.White, ResourceManager.BorderPen, stream);
            }
        }

        #endregion
    }
}