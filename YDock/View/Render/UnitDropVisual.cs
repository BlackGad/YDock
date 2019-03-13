using System.Windows;
using System.Windows.Media;
using YDock.Enum;
using YDock.View.Control;
using YDock.View.Layout;

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
                        _DrawLeft(ctx, hOffset + Constants.DropGlassLength, vOffset + (size.Height - Constants.DropUnitLength) / 2);
                    }
                    else if ((Flag & DragManager.Top) != 0)
                    {
                        _DrawTop(ctx, hOffset + (size.Width - Constants.DropUnitLength) / 2, vOffset + Constants.DropGlassLength);
                    }
                    else if ((Flag & DragManager.Right) != 0)
                    {
                        _DrawRight(ctx, hOffset + size.Width - Constants.DropGlassLength, vOffset + (size.Height - Constants.DropUnitLength) / 2);
                    }
                    else if ((Flag & DragManager.Bottom) != 0)
                    {
                        _DrawBottom(ctx, hOffset + (size.Width - Constants.DropUnitLength) / 2, vOffset + size.Height - Constants.DropGlassLength);
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
                            _DrawCenter(ctx,
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
                                layoutControl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = layoutControl.DockViewParent as LayoutGroupPanel;
                                    flag &= layoutPanel.Direction != Direction.Vertical && layoutControl.ChildrenCount > 0;
                                }

                                if (flag)
                                {
                                    hOffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2 - Constants.DropUnitLength;
                                    vOffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                                    _DrawCenter(ctx, hOffset, vOffset, true, true);
                                }
                            }
                        }
                        else
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = layoutControl.DockViewParent as LayoutGroupPanel;
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
                                _DrawLeft(ctx, hOffset, vOffset, false);
                            }
                        }
                    }

                    if ((Flag & DragManager.Right) != 0)
                    {
                        if ((Flag & DragManager.Split) != 0)
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = layoutControl.DockViewParent as LayoutGroupPanel;
                                    flag &= layoutPanel.Direction != Direction.Vertical && layoutControl.ChildrenCount > 0;
                                }

                                if (flag)
                                {
                                    hOffset += (DropPanel.InnerRect.Size.Width + Constants.DropUnitLength) / 2;
                                    vOffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                                    _DrawCenter(ctx, hOffset, vOffset, true, true);
                                }
                            }
                        }
                        else
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = layoutControl.DockViewParent as LayoutGroupPanel;
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
                                _DrawRight(ctx, hOffset, vOffset, false);
                            }
                        }
                    }

                    if ((Flag & DragManager.Top) != 0)
                    {
                        if ((Flag & DragManager.Split) != 0)
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = layoutControl.DockViewParent as LayoutGroupPanel;
                                    flag &= layoutPanel.Direction != Direction.Horizontal && layoutControl.ChildrenCount > 0;
                                }

                                if (flag)
                                {
                                    hOffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                    vOffset += DropPanel.InnerRect.Size.Height / 2 - Constants.DropUnitLength * 1.5;
                                    _DrawCenter(ctx, hOffset, vOffset, true);
                                }
                            }
                        }
                        else
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = layoutControl.DockViewParent as LayoutGroupPanel;
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
                                _DrawTop(ctx, hOffset, vOffset, false);
                            }
                        }
                    }

                    if ((Flag & DragManager.Bottom) != 0)
                    {
                        if ((Flag & DragManager.Split) != 0)
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = layoutControl.DockViewParent as LayoutGroupPanel;
                                    flag &= layoutPanel.Direction != Direction.Horizontal && layoutControl.ChildrenCount > 0;
                                }

                                if (flag)
                                {
                                    hOffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                    vOffset += (DropPanel.InnerRect.Size.Height + Constants.DropUnitLength) / 2;
                                    _DrawCenter(ctx, hOffset, vOffset, true);
                                }
                            }
                        }
                        else
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutControl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutControl.DockViewParent != null)
                                {
                                    layoutPanel = layoutControl.DockViewParent as LayoutGroupPanel;
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
                                _DrawBottom(ctx, hOffset, vOffset, false);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Members

        private void _DrawBottom(DrawingContext ctx, double hoffset, double voffset, bool hasGlassBorder = true)
        {
            if (hasGlassBorder)
            {
                //绘制玻璃外观
                ctx.PushOpacity(Constants.DragOpacity);
                ctx.DrawRectangle(Brushes.White,
                                  ResourceManager.BorderPen,
                                  new Rect(hoffset, voffset - Constants.DropUnitLength, Constants.DropUnitLength, Constants.DropUnitLength));
                ctx.Pop();
            }

            if ((Flag & DragManager.Active) == 0)
            {
                ctx.PushOpacity(Constants.DragOpacity * 1.8);
            }

            ctx.DrawRoundedRectangle(Brushes.White,
                                     null,
                                     new Rect(hoffset += Constants.DropGlassLength,
                                              (voffset -= Constants.DropGlassLength) - (Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                     3,
                                     3);
            hoffset += Constants.DropGlassLength;
            voffset -= Constants.DropGlassLength;

            ctx.DrawLine(ResourceManager.DropRectPen_Heavy,
                         new Point(hoffset - 0.5, voffset - 12),
                         new Point(hoffset + Constants.DropUnitLength - Constants.DropGlassLength * 4 + 0.5, voffset - 12));

            //绘制小窗口
            var stream = new StreamGeometry();
            using (var sctx = stream.Open())
            {
                double currentX = hoffset, currentY = voffset - 12;
                sctx.BeginFigure(new Point(currentX, currentY), true, false);
                sctx.LineTo(new Point(currentX, currentY += 12), true, true);
                sctx.LineTo(new Point(currentX += Constants.DropUnitLength - Constants.DropGlassLength * 4, currentY), true, true);
                sctx.LineTo(new Point(currentX, currentY -= 12), true, true);
                sctx.Close();
            }

            ctx.DrawGeometry(null, ResourceManager.DropRectPen, stream);

            //绘制方向箭头
            stream = new StreamGeometry();
            using (var sctx = stream.Open())
            {
                double currentX = hoffset + (Constants.DropUnitLength - Constants.DropGlassLength * 4) / 2, currentY = voffset - 20;
                sctx.BeginFigure(new Point(currentX, currentY), true, true);
                sctx.LineTo(new Point(currentX += 5, currentY -= 5), true, true);
                sctx.LineTo(new Point(currentX -= 10, currentY), true, true);
                sctx.Close();
            }

            ctx.DrawGeometry(Brushes.Black, null, stream);
        }

        private void _DrawCenter(DrawingContext ctx, double hoffset, double voffset, bool withSpliterLine = false, bool isVertical = false)
        {
            double currentX = hoffset, currentY = voffset;

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

            ctx.DrawLine(ResourceManager.DropRectPen_Heavy,
                         new Point(currentX - 0.5, currentY),
                         new Point(currentX + Constants.DropUnitLength - Constants.DropGlassLength * 4 + 0.5, currentY));

            //绘制小窗口
            var stream = new StreamGeometry();
            using (var sctx = stream.Open())
            {
                sctx.BeginFigure(new Point(currentX, currentY), true, false);
                sctx.LineTo(new Point(currentX, currentY += 22), true, true);
                sctx.LineTo(new Point(currentX += Constants.DropUnitLength - Constants.DropGlassLength * 4, currentY), true, true);
                sctx.LineTo(new Point(currentX, currentY -= 22), true, true);
                sctx.Close();
            }

            ctx.DrawGeometry(null, ResourceManager.DropRectPen, stream);

            if (withSpliterLine)
            {
                if (isVertical)
                {
                    ctx.DrawLine(ResourceManager.BlueDashPen,
                                 new Point(hoffset + Constants.DropUnitLength / 2, voffset + Constants.DropGlassLength * 2 + 3),
                                 new Point(hoffset + Constants.DropUnitLength / 2, voffset + Constants.DropUnitLength - 2 * Constants.DropGlassLength));
                }
                else
                {
                    ctx.DrawLine(ResourceManager.BlueDashPen,
                                 new Point(hoffset + Constants.DropGlassLength * 2, voffset + Constants.DropUnitLength / 2),
                                 new Point(hoffset + Constants.DropUnitLength - 2 * Constants.DropGlassLength, voffset + Constants.DropUnitLength / 2));
                }
            }
        }

        private void _DrawLeft(DrawingContext ctx, double hoffset, double voffset, bool hasGlassBorder = true)
        {
            if (hasGlassBorder)
            {
                //绘制玻璃外观
                ctx.PushOpacity(Constants.DragOpacity);
                ctx.DrawRectangle(Brushes.White, ResourceManager.BorderPen, new Rect(hoffset, voffset, Constants.DropUnitLength, Constants.DropUnitLength));
                ctx.Pop();
            }

            if ((Flag & DragManager.Active) == 0)
            {
                ctx.PushOpacity(Constants.DragOpacity * 1.8);
            }

            ctx.DrawRoundedRectangle(Brushes.White,
                                     null,
                                     new Rect(hoffset += Constants.DropGlassLength,
                                              voffset += Constants.DropGlassLength,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                     3,
                                     3);
            hoffset += Constants.DropGlassLength;
            voffset += Constants.DropGlassLength;
            ctx.DrawLine(ResourceManager.DropRectPen_Heavy, new Point(hoffset - 0.5, voffset), new Point(hoffset + 12.5, voffset));

            //绘制小窗口
            var stream = new StreamGeometry();
            using (var sctx = stream.Open())
            {
                double currentX = hoffset, currentY = voffset;
                sctx.BeginFigure(new Point(currentX, currentY), true, false);
                sctx.LineTo(new Point(currentX, currentY += Constants.DropUnitLength - Constants.DropGlassLength * 4), true, true);
                sctx.LineTo(new Point(currentX += 12, currentY), true, true);
                sctx.LineTo(new Point(currentX, currentY -= Constants.DropUnitLength - Constants.DropGlassLength * 4), true, true);
                sctx.Close();
            }

            ctx.DrawGeometry(null, ResourceManager.DropRectPen, stream);

            //绘制方向箭头
            stream = new StreamGeometry();
            using (var sctx = stream.Open())
            {
                double currentX = hoffset + 20, currentY = voffset + (Constants.DropUnitLength - Constants.DropGlassLength * 4) / 2;
                sctx.BeginFigure(new Point(currentX, currentY), true, true);
                sctx.LineTo(new Point(currentX += 5, currentY -= 5), true, true);
                sctx.LineTo(new Point(currentX, currentY += 10), true, true);
                sctx.Close();
            }

            ctx.DrawGeometry(Brushes.Black, null, stream);
        }

        private void _DrawRight(DrawingContext ctx, double hoffset, double voffset, bool hasGlassBorder = true)
        {
            if (hasGlassBorder)
            {
                //绘制玻璃外观
                ctx.PushOpacity(Constants.DragOpacity);
                ctx.DrawRectangle(Brushes.White,
                                  ResourceManager.BorderPen,
                                  new Rect(hoffset - Constants.DropUnitLength, voffset, Constants.DropUnitLength, Constants.DropUnitLength));
                ctx.Pop();
            }

            if ((Flag & DragManager.Active) == 0)
            {
                ctx.PushOpacity(Constants.DragOpacity * 1.8);
            }

            ctx.DrawRoundedRectangle(Brushes.White,
                                     null,
                                     new Rect((hoffset -= Constants.DropGlassLength) - (Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                              voffset += Constants.DropGlassLength,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                     3,
                                     3);
            hoffset -= Constants.DropGlassLength;
            voffset += Constants.DropGlassLength;
            ctx.DrawLine(ResourceManager.DropRectPen_Heavy, new Point(hoffset + 0.5, voffset), new Point(hoffset - 12.5, voffset));

            //绘制小窗口
            var stream = new StreamGeometry();
            using (var sctx = stream.Open())
            {
                double currentX = hoffset, currentY = voffset;
                sctx.BeginFigure(new Point(currentX, currentY), true, false);
                sctx.LineTo(new Point(currentX, currentY += Constants.DropUnitLength - Constants.DropGlassLength * 4), true, true);
                sctx.LineTo(new Point(currentX -= 12, currentY), true, true);
                sctx.LineTo(new Point(currentX, currentY -= Constants.DropUnitLength - Constants.DropGlassLength * 4), true, true);
                sctx.Close();
            }

            ctx.DrawGeometry(null, ResourceManager.DropRectPen, stream);

            //绘制方向箭头
            stream = new StreamGeometry();
            using (var sctx = stream.Open())
            {
                double currentX = hoffset - 20, currentY = voffset + (Constants.DropUnitLength - Constants.DropGlassLength * 4) / 2;
                sctx.BeginFigure(new Point(currentX, currentY), true, true);
                sctx.LineTo(new Point(currentX -= 5, currentY -= 5), true, true);
                sctx.LineTo(new Point(currentX, currentY += 10), true, true);
                sctx.Close();
            }

            ctx.DrawGeometry(Brushes.Black, null, stream);
        }

        private void _DrawTop(DrawingContext ctx, double hoffset, double voffset, bool hasGlassBorder = true)
        {
            if (hasGlassBorder)
            {
                //绘制玻璃外观
                ctx.PushOpacity(Constants.DragOpacity);
                ctx.DrawRectangle(Brushes.White, ResourceManager.BorderPen, new Rect(hoffset, voffset, Constants.DropUnitLength, Constants.DropUnitLength));
                ctx.Pop();
            }

            if ((Flag & DragManager.Active) == 0)
            {
                ctx.PushOpacity(Constants.DragOpacity * 1.8);
            }

            ctx.DrawRoundedRectangle(Brushes.White,
                                     null,
                                     new Rect(hoffset += Constants.DropGlassLength,
                                              voffset += Constants.DropGlassLength,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2,
                                              Constants.DropUnitLength - Constants.DropGlassLength * 2),
                                     3,
                                     3);
            hoffset += Constants.DropGlassLength;
            voffset += Constants.DropGlassLength;

            ctx.DrawLine(ResourceManager.DropRectPen_Heavy,
                         new Point(hoffset - 0.5, voffset),
                         new Point(hoffset + Constants.DropUnitLength - Constants.DropGlassLength * 4 + 0.5, voffset));

            //绘制小窗口
            var stream = new StreamGeometry();
            using (var sctx = stream.Open())
            {
                double currentX = hoffset, currentY = voffset;
                sctx.BeginFigure(new Point(currentX, currentY), true, false);
                sctx.LineTo(new Point(currentX, currentY += 12), true, true);
                sctx.LineTo(new Point(currentX += Constants.DropUnitLength - Constants.DropGlassLength * 4, currentY), true, true);
                sctx.LineTo(new Point(currentX, currentY -= 12), true, true);
                sctx.Close();
            }

            ctx.DrawGeometry(null, ResourceManager.DropRectPen, stream);

            //绘制方向箭头
            stream = new StreamGeometry();
            using (var sctx = stream.Open())
            {
                double currentX = hoffset + (Constants.DropUnitLength - Constants.DropGlassLength * 4) / 2, currentY = voffset + 20;
                sctx.BeginFigure(new Point(currentX, currentY), true, true);
                sctx.LineTo(new Point(currentX += 5, currentY += 5), true, true);
                sctx.LineTo(new Point(currentX -= 10, currentY), true, true);
                sctx.Close();
            }

            ctx.DrawGeometry(Brushes.Black, null, stream);
        }

        #endregion
    }
}