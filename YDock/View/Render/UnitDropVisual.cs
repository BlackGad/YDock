using System.Windows;
using System.Windows.Media;
using YDock.Enum;
using YDock.View.Control;
using YDock.View.Layout;

// ReSharper disable RedundantAssignment

namespace YDock.View.Render
{
    public class UnitDropVisual : BaseDropVisual
    {
        #region Constructors

        internal UnitDropVisual(int flag) : base(flag)
        {
        }

        #endregion

        #region Override members

        public override void Update(Size size)
        {
            if (DropPanel.Target == null) return;
            using (var ctx = RenderOpen())
            {
                double hOffset = DropPanel.InnerRect.Left + DropPanel.OuterRect.Left, vOffset = DropPanel.InnerRect.Top + DropPanel.OuterRect.Top;
                if (DropPanel is RootDropPanel)
                {
                    if ((Flag & DragManager.LEFT) != 0)
                    {
                        DrawLeft(ctx, hOffset + Constants.DropGlassLength, vOffset + (size.Height - Constants.DropUnitLength) / 2);
                    }
                    else if ((Flag & DragManager.Top) != 0)
                    {
                        DrawTop(ctx, hOffset + (size.Width - Constants.DropUnitLength) / 2, vOffset + Constants.DropGlassLength);
                    }
                    else if ((Flag & DragManager.Right) != 0)
                    {
                        DrawRight(ctx, hOffset + size.Width - Constants.DropGlassLength, vOffset + (size.Height - Constants.DropUnitLength) / 2);
                    }
                    else if ((Flag & DragManager.Bottom) != 0)
                    {
                        DrawBottom(ctx, hOffset + (size.Width - Constants.DropUnitLength) / 2, vOffset + size.Height - Constants.DropGlassLength);
                    }
                }
                else
                {
                    var flag = false;
                    if ((Flag & DragManager.Center) != 0)
                    {
                        if (DropPanel.Target.Mode == DragMode.Document)
                        {
                            flag = true;
                        }

                        if (DropPanel.Source.DragMode != DragMode.Document
                            && DropPanel.Target.Mode == DragMode.Anchor)
                        {
                            flag = true;
                        }

                        if (flag)
                        {
                            DrawCenter(ctx,
                                       hOffset + (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2,
                                       vOffset + (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2);
                        }
                    }

                    flag = true;
                    LayoutDocumentGroupControl layoutControl;
                    LayoutGroupPanel layoutPanel;

                    if ((Flag & DragManager.LEFT) != 0)
                    {
                        if ((Flag & DragManager.Split) != 0)
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = (LayoutDocumentGroupControl)DropPanel.Target;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = (LayoutGroupPanel)layoutControl.DockViewParent;
                                    flag &= layoutPanel.Direction != Direction.Vertical && layoutControl.ChildrenCount > 0;
                                }

                                if (flag)
                                {
                                    hOffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2 - Constants.DropUnitLength;
                                    vOffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                                    DrawCenter(ctx, hOffset, vOffset, true, true);
                                }
                            }
                        }
                        else
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = (LayoutDocumentGroupControl)DropPanel.Target;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = (LayoutGroupPanel)layoutControl.DockViewParent;
                                    if (layoutPanel.Direction == Direction.Horizontal)
                                    {
                                        flag &= layoutControl.IndexOf() == 0;
                                    }
                                }

                                hOffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2 - Constants.DropUnitLength * 2;
                                vOffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                            }
                            else
                            {
                                hOffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2 - Constants.DropUnitLength;
                                vOffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                            }

                            if (DropPanel.Source.DragMode == DragMode.Anchor && flag)
                            {
                                DrawLeft(ctx, hOffset, vOffset, false);
                            }
                        }
                    }

                    if ((Flag & DragManager.Right) != 0)
                    {
                        if ((Flag & DragManager.Split) != 0)
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = (LayoutDocumentGroupControl)DropPanel.Target;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = (LayoutGroupPanel)layoutControl.DockViewParent;
                                    flag &= layoutPanel.Direction != Direction.Vertical && layoutControl.ChildrenCount > 0;
                                }

                                if (flag)
                                {
                                    hOffset += (DropPanel.InnerRect.Size.Width + Constants.DropUnitLength) / 2;
                                    vOffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                                    DrawCenter(ctx, hOffset, vOffset, true, true);
                                }
                            }
                        }
                        else
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = (LayoutDocumentGroupControl)DropPanel.Target;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = (LayoutGroupPanel)layoutControl.DockViewParent;
                                    if (layoutPanel.Direction == Direction.Horizontal)
                                    {
                                        flag &= layoutControl.IndexOf() == layoutPanel.Count - 1;
                                    }
                                }

                                hOffset += DropPanel.InnerRect.Size.Width / 2 + Constants.DropUnitLength * 2.5;
                                vOffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                            }
                            else
                            {
                                hOffset += DropPanel.InnerRect.Size.Width / 2 + Constants.DropUnitLength * 1.5;
                                vOffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                            }

                            if (DropPanel.Source.DragMode == DragMode.Anchor && flag)
                            {
                                DrawRight(ctx, hOffset, vOffset, false);
                            }
                        }
                    }

                    if ((Flag & DragManager.Top) != 0)
                    {
                        if ((Flag & DragManager.Split) != 0)
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = (LayoutDocumentGroupControl)DropPanel.Target;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = (LayoutGroupPanel)layoutControl.DockViewParent;
                                    flag &= layoutPanel.Direction != Direction.Horizontal && layoutControl.ChildrenCount > 0;
                                }

                                if (flag)
                                {
                                    hOffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                    vOffset += DropPanel.InnerRect.Size.Height / 2 - Constants.DropUnitLength * 1.5;
                                    DrawCenter(ctx, hOffset, vOffset, true);
                                }
                            }
                        }
                        else
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = (LayoutDocumentGroupControl)DropPanel.Target;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = (LayoutGroupPanel)layoutControl.DockViewParent;
                                    if (layoutPanel.Direction == Direction.Vertical)
                                    {
                                        flag &= layoutControl.IndexOf() == 0;
                                    }
                                }

                                hOffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                vOffset += DropPanel.InnerRect.Size.Height / 2 - Constants.DropUnitLength * 2.5;
                            }
                            else
                            {
                                hOffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                vOffset += DropPanel.InnerRect.Size.Height / 2 - Constants.DropUnitLength * 1.5;
                            }

                            if (DropPanel.Source.DragMode == DragMode.Anchor && flag)
                            {
                                DrawTop(ctx, hOffset, vOffset, false);
                            }
                        }
                    }

                    if ((Flag & DragManager.Bottom) != 0)
                    {
                        if ((Flag & DragManager.Split) != 0)
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = (LayoutDocumentGroupControl)DropPanel.Target;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = (LayoutGroupPanel)layoutControl.DockViewParent;
                                    flag &= layoutPanel.Direction != Direction.Horizontal && layoutControl.ChildrenCount > 0;
                                }

                                if (flag)
                                {
                                    hOffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                    vOffset += (DropPanel.InnerRect.Size.Height + Constants.DropUnitLength) / 2;
                                    DrawCenter(ctx, hOffset, vOffset, true);
                                }
                            }
                        }
                        else
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = (LayoutDocumentGroupControl)DropPanel.Target;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = (LayoutGroupPanel)layoutControl.DockViewParent;
                                    if (layoutPanel.Direction == Direction.Vertical)
                                    {
                                        flag &= layoutControl.IndexOf() == layoutPanel.Count - 1;
                                    }
                                }

                                hOffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                vOffset += DropPanel.InnerRect.Size.Height / 2 + Constants.DropUnitLength * 2.5;
                            }
                            else
                            {
                                hOffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                vOffset += DropPanel.InnerRect.Size.Height / 2 + Constants.DropUnitLength * 1.5;
                            }

                            if (DropPanel.Source.DragMode == DragMode.Anchor && flag)
                            {
                                DrawBottom(ctx, hOffset, vOffset, false);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Members

        private void DrawBottom(DrawingContext ctx, double hOffset, double vOffset, bool hasGlassBorder = true)
        {
            if (hasGlassBorder)
            {
                //Drawing the appearance of the glass
                ctx.PushOpacity(Constants.DragOpacity);
                ctx.DrawRectangle(Brushes.White,
                                  ResourceManager.BorderPen,
                                  new Rect(hOffset, vOffset - Constants.DropUnitLength, Constants.DropUnitLength, Constants.DropUnitLength));
                ctx.Pop();
            }

            if ((Flag & DragManager.Active) == 0)
            {
                ctx.PushOpacity(Constants.DragOpacity * 1.8);
            }

            ctx.DrawRoundedRectangle(Brushes.White,
                                     null,
                                     new Rect(hOffset += Constants.DropGlassLength,
                                              (vOffset -= Constants.DropGlassLength) - (Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                     3,
                                     3);
            hOffset += Constants.DropGlassLength;
            vOffset -= Constants.DropGlassLength;

            ctx.DrawLine(ResourceManager.DropRectPenHeavy,
                         new Point(hOffset - 0.5, vOffset - 12),
                         new Point(hOffset + Constants.DropUnitLength - Constants.DropGlassLength * 4 + 0.5, vOffset - 12));

            //Draw a small window
            var stream = new StreamGeometry();
            using (var context = stream.Open())
            {
                double currentX = hOffset, currentY = vOffset - 12;
                context.BeginFigure(new Point(currentX, currentY), true, false);
                context.LineTo(new Point(currentX, currentY += 12), true, true);
                context.LineTo(new Point(currentX += Constants.DropUnitLength - Constants.DropGlassLength * 4, currentY), true, true);
                context.LineTo(new Point(currentX, currentY -= 12), true, true);
                context.Close();
            }

            ctx.DrawGeometry(null, ResourceManager.DropRectPen, stream);

            //Drawing direction arrows
            stream = new StreamGeometry();
            using (var context = stream.Open())
            {
                double currentX = hOffset + (Constants.DropUnitLength - Constants.DropGlassLength * 4) / 2, currentY = vOffset - 20;
                context.BeginFigure(new Point(currentX, currentY), true, true);
                context.LineTo(new Point(currentX += 5, currentY -= 5), true, true);
                context.LineTo(new Point(currentX -= 10, currentY), true, true);
                context.Close();
            }

            ctx.DrawGeometry(Brushes.Black, null, stream);
        }

        private void DrawCenter(DrawingContext ctx, double hOffset, double vOffset, bool withSplitterLine = false, bool isVertical = false)
        {
            double currentX = hOffset, currentY = vOffset;

            if ((Flag & DragManager.Active) == 0)
            {
                ctx.PushOpacity(Constants.DragOpacity * 1.8);
            }

            ctx.DrawRoundedRectangle(Brushes.White,
                                     null,
                                     new Rect(currentX += Constants.DropGlassLength,
                                              currentY += Constants.DropGlassLength,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                     3,
                                     3);
            currentX += Constants.DropGlassLength;
            currentY += Constants.DropGlassLength + 2;

            ctx.DrawLine(ResourceManager.DropRectPenHeavy,
                         new Point(currentX - 0.5, currentY),
                         new Point(currentX + Constants.DropUnitLength - Constants.DropGlassLength * 4 + 0.5, currentY));

            //Draw a small window
            var stream = new StreamGeometry();
            using (var context = stream.Open())
            {
                context.BeginFigure(new Point(currentX, currentY), true, false);
                context.LineTo(new Point(currentX, currentY += 22), true, true);
                context.LineTo(new Point(currentX += Constants.DropUnitLength - Constants.DropGlassLength * 4, currentY), true, true);
                context.LineTo(new Point(currentX, currentY -= 22), true, true);
                context.Close();
            }

            ctx.DrawGeometry(null, ResourceManager.DropRectPen, stream);

            if (withSplitterLine)
            {
                if (isVertical)
                {
                    ctx.DrawLine(ResourceManager.BlueDashPen,
                                 new Point(hOffset + Constants.DropUnitLength / 2, vOffset + Constants.DropGlassLength * 2 + 3),
                                 new Point(hOffset + Constants.DropUnitLength / 2, vOffset + Constants.DropUnitLength - 2 * Constants.DropGlassLength));
                }
                else
                {
                    ctx.DrawLine(ResourceManager.BlueDashPen,
                                 new Point(hOffset + Constants.DropGlassLength * 2, vOffset + Constants.DropUnitLength / 2),
                                 new Point(hOffset + Constants.DropUnitLength - 2 * Constants.DropGlassLength, vOffset + Constants.DropUnitLength / 2));
                }
            }
        }

        private void DrawLeft(DrawingContext ctx, double hOffset, double vOffset, bool hasGlassBorder = true)
        {
            if (hasGlassBorder)
            {
                //Drawing the appearance of the glass
                ctx.PushOpacity(Constants.DragOpacity);
                ctx.DrawRectangle(Brushes.White, ResourceManager.BorderPen, new Rect(hOffset, vOffset, Constants.DropUnitLength, Constants.DropUnitLength));
                ctx.Pop();
            }

            if ((Flag & DragManager.Active) == 0)
            {
                ctx.PushOpacity(Constants.DragOpacity * 1.8);
            }

            ctx.DrawRoundedRectangle(Brushes.White,
                                     null,
                                     new Rect(hOffset += Constants.DropGlassLength,
                                              vOffset += Constants.DropGlassLength,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                     3,
                                     3);
            hOffset += Constants.DropGlassLength;
            vOffset += Constants.DropGlassLength;
            ctx.DrawLine(ResourceManager.DropRectPenHeavy, new Point(hOffset - 0.5, vOffset), new Point(hOffset + 12.5, vOffset));

            //Draw a small window
            var stream = new StreamGeometry();
            using (var context = stream.Open())
            {
                double currentX = hOffset, currentY = vOffset;
                context.BeginFigure(new Point(currentX, currentY), true, false);
                context.LineTo(new Point(currentX, currentY += Constants.DropUnitLength - Constants.DropGlassLength * 4), true, true);
                context.LineTo(new Point(currentX += 12, currentY), true, true);
                context.LineTo(new Point(currentX, currentY -= Constants.DropUnitLength - Constants.DropGlassLength * 4), true, true);
                context.Close();
            }

            ctx.DrawGeometry(null, ResourceManager.DropRectPen, stream);

            //Drawing direction arrows
            stream = new StreamGeometry();
            using (var context = stream.Open())
            {
                double currentX = hOffset + 20, currentY = vOffset + (Constants.DropUnitLength - Constants.DropGlassLength * 4) / 2;
                context.BeginFigure(new Point(currentX, currentY), true, true);
                context.LineTo(new Point(currentX += 5, currentY -= 5), true, true);
                context.LineTo(new Point(currentX, currentY += 10), true, true);
                context.Close();
            }

            ctx.DrawGeometry(Brushes.Black, null, stream);
        }

        private void DrawRight(DrawingContext ctx, double hOffset, double vOffset, bool hasGlassBorder = true)
        {
            if (hasGlassBorder)
            {
                //Drawing the appearance of the glass
                ctx.PushOpacity(Constants.DragOpacity);
                ctx.DrawRectangle(Brushes.White,
                                  ResourceManager.BorderPen,
                                  new Rect(hOffset - Constants.DropUnitLength, vOffset, Constants.DropUnitLength, Constants.DropUnitLength));
                ctx.Pop();
            }

            if ((Flag & DragManager.Active) == 0)
            {
                ctx.PushOpacity(Constants.DragOpacity * 1.8);
            }

            ctx.DrawRoundedRectangle(Brushes.White,
                                     null,
                                     new Rect((hOffset -= Constants.DropGlassLength) - (Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                              vOffset += Constants.DropGlassLength,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                     3,
                                     3);
            hOffset -= Constants.DropGlassLength;
            vOffset += Constants.DropGlassLength;
            ctx.DrawLine(ResourceManager.DropRectPenHeavy, new Point(hOffset + 0.5, vOffset), new Point(hOffset - 12.5, vOffset));

            //Draw a small window
            var stream = new StreamGeometry();
            using (var context = stream.Open())
            {
                double currentX = hOffset, currentY = vOffset;
                context.BeginFigure(new Point(currentX, currentY), true, false);
                context.LineTo(new Point(currentX, currentY += Constants.DropUnitLength - Constants.DropGlassLength * 4), true, true);
                context.LineTo(new Point(currentX -= 12, currentY), true, true);
                context.LineTo(new Point(currentX, currentY -= Constants.DropUnitLength - Constants.DropGlassLength * 4), true, true);
                context.Close();
            }

            ctx.DrawGeometry(null, ResourceManager.DropRectPen, stream);

            //Drawing direction arrows
            stream = new StreamGeometry();
            using (var context = stream.Open())
            {
                double currentX = hOffset - 20, currentY = vOffset + (Constants.DropUnitLength - Constants.DropGlassLength * 4) / 2;
                context.BeginFigure(new Point(currentX, currentY), true, true);
                context.LineTo(new Point(currentX -= 5, currentY -= 5), true, true);
                context.LineTo(new Point(currentX, currentY += 10), true, true);
                context.Close();
            }

            ctx.DrawGeometry(Brushes.Black, null, stream);
        }

        private void DrawTop(DrawingContext ctx, double hOffset, double vOffset, bool hasGlassBorder = true)
        {
            if (hasGlassBorder)
            {
                //Drawing the appearance of the glass
                ctx.PushOpacity(Constants.DragOpacity);
                ctx.DrawRectangle(Brushes.White, ResourceManager.BorderPen, new Rect(hOffset, vOffset, Constants.DropUnitLength, Constants.DropUnitLength));
                ctx.Pop();
            }

            if ((Flag & DragManager.Active) == 0)
            {
                ctx.PushOpacity(Constants.DragOpacity * 1.8);
            }

            ctx.DrawRoundedRectangle(Brushes.White,
                                     null,
                                     new Rect(hOffset += Constants.DropGlassLength,
                                              vOffset += Constants.DropGlassLength,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                     3,
                                     3);
            hOffset += Constants.DropGlassLength;
            vOffset += Constants.DropGlassLength;

            ctx.DrawLine(ResourceManager.DropRectPenHeavy,
                         new Point(hOffset - 0.5, vOffset),
                         new Point(hOffset + Constants.DropUnitLength - Constants.DropGlassLength * 4 + 0.5, vOffset));

            //Draw a small window
            var stream = new StreamGeometry();
            using (var context = stream.Open())
            {
                double currentX = hOffset, currentY = vOffset;
                context.BeginFigure(new Point(currentX, currentY), true, false);
                context.LineTo(new Point(currentX, currentY += 12), true, true);
                context.LineTo(new Point(currentX += Constants.DropUnitLength - Constants.DropGlassLength * 4, currentY), true, true);
                context.LineTo(new Point(currentX, currentY -= 12), true, true);
                context.Close();
            }

            ctx.DrawGeometry(null, ResourceManager.DropRectPen, stream);

            //Drawing direction arrows
            stream = new StreamGeometry();
            using (var context = stream.Open())
            {
                double currentX = hOffset + (Constants.DropUnitLength - Constants.DropGlassLength * 4) / 2, currentY = vOffset + 20;
                context.BeginFigure(new Point(currentX, currentY), true, true);
                context.LineTo(new Point(currentX += 5, currentY += 5), true, true);
                context.LineTo(new Point(currentX -= 10, currentY), true, true);
                context.Close();
            }

            ctx.DrawGeometry(Brushes.Black, null, stream);
        }

        #endregion
    }
}