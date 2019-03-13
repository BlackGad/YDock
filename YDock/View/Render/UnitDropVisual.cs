using System.Windows;
using System.Windows.Media;
using YDock.Enum;

namespace YDock.View
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
                double hoffset = DropPanel.InnerRect.Left + DropPanel.OuterRect.Left, voffset = DropPanel.InnerRect.Top + DropPanel.OuterRect.Top;
                if (DropPanel is RootDropPanel)
                {
                    if ((Flag & DragManager.LEFT) != 0)
                    {
                        _DrawLeft(ctx, hoffset + Constants.DropGlassLength, voffset + (size.Height - Constants.DropUnitLength) / 2);
                    }
                    else if ((Flag & DragManager.TOP) != 0)
                    {
                        _DrawTop(ctx, hoffset + (size.Width - Constants.DropUnitLength) / 2, voffset + Constants.DropGlassLength);
                    }
                    else if ((Flag & DragManager.RIGHT) != 0)
                    {
                        _DrawRight(ctx, hoffset + size.Width - Constants.DropGlassLength, voffset + (size.Height - Constants.DropUnitLength) / 2);
                    }
                    else if ((Flag & DragManager.BOTTOM) != 0)
                    {
                        _DrawBottom(ctx, hoffset + (size.Width - Constants.DropUnitLength) / 2, voffset + size.Height - Constants.DropGlassLength);
                    }
                }
                else
                {
                    var flag = false;
                    if ((Flag & DragManager.CENTER) != 0)
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
                                        hoffset + (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2,
                                        voffset + (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2);
                        }
                    }

                    flag = true;
                    LayoutDocumentGroupControl layoutCrtl;
                    LayoutGroupPanel layoutpanel;

                    if ((Flag & DragManager.LEFT) != 0)
                    {
                        if ((Flag & DragManager.SPLIT) != 0)
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutCrtl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutCrtl.DockViewParent != null)
                                {
                                    layoutpanel = layoutCrtl.DockViewParent as LayoutGroupPanel;
                                    flag &= layoutpanel.Direction != Direction.Vertical && layoutCrtl.ChildrenCount > 0;
                                }

                                if (flag)
                                {
                                    hoffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2 - Constants.DropUnitLength;
                                    voffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                                    _DrawCenter(ctx, hoffset, voffset, true, true);
                                }
                            }
                        }
                        else
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutCrtl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutCrtl.DockViewParent != null)
                                {
                                    layoutpanel = layoutCrtl.DockViewParent as LayoutGroupPanel;
                                    if (layoutpanel.Direction == Direction.Horizontal)
                                    {
                                        flag &= layoutCrtl.IndexOf() == 0;
                                    }
                                }

                                hoffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2 - Constants.DropUnitLength * 2;
                                voffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                            }
                            else
                            {
                                hoffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2 - Constants.DropUnitLength;
                                voffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                            }

                            if (DropPanel.Source.DragMode == DragMode.Anchor && flag)
                            {
                                _DrawLeft(ctx, hoffset, voffset, false);
                            }
                        }
                    }

                    if ((Flag & DragManager.RIGHT) != 0)
                    {
                        if ((Flag & DragManager.SPLIT) != 0)
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutCrtl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutCrtl.DockViewParent != null)
                                {
                                    layoutpanel = layoutCrtl.DockViewParent as LayoutGroupPanel;
                                    flag &= layoutpanel.Direction != Direction.Vertical && layoutCrtl.ChildrenCount > 0;
                                }

                                if (flag)
                                {
                                    hoffset += (DropPanel.InnerRect.Size.Width + Constants.DropUnitLength) / 2;
                                    voffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                                    _DrawCenter(ctx, hoffset, voffset, true, true);
                                }
                            }
                        }
                        else
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutCrtl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutCrtl.DockViewParent != null)
                                {
                                    layoutpanel = layoutCrtl.DockViewParent as LayoutGroupPanel;
                                    if (layoutpanel.Direction == Direction.Horizontal)
                                    {
                                        flag &= layoutCrtl.IndexOf() == layoutpanel.Count - 1;
                                    }
                                }

                                hoffset += DropPanel.InnerRect.Size.Width / 2 + Constants.DropUnitLength * 2.5;
                                voffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                            }
                            else
                            {
                                hoffset += DropPanel.InnerRect.Size.Width / 2 + Constants.DropUnitLength * 1.5;
                                voffset += (DropPanel.InnerRect.Size.Height - Constants.DropUnitLength) / 2;
                            }

                            if (DropPanel.Source.DragMode == DragMode.Anchor && flag)
                            {
                                _DrawRight(ctx, hoffset, voffset, false);
                            }
                        }
                    }

                    if ((Flag & DragManager.TOP) != 0)
                    {
                        if ((Flag & DragManager.SPLIT) != 0)
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutCrtl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutCrtl.DockViewParent != null)
                                {
                                    layoutpanel = layoutCrtl.DockViewParent as LayoutGroupPanel;
                                    flag &= layoutpanel.Direction != Direction.Horizontal && layoutCrtl.ChildrenCount > 0;
                                }

                                if (flag)
                                {
                                    hoffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                    voffset += DropPanel.InnerRect.Size.Height / 2 - Constants.DropUnitLength * 1.5;
                                    _DrawCenter(ctx, hoffset, voffset, true);
                                }
                            }
                        }
                        else
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutCrtl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutCrtl.DockViewParent != null)
                                {
                                    layoutpanel = layoutCrtl.DockViewParent as LayoutGroupPanel;
                                    if (layoutpanel.Direction == Direction.Vertical)
                                    {
                                        flag &= layoutCrtl.IndexOf() == 0;
                                    }
                                }

                                hoffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                voffset += DropPanel.InnerRect.Size.Height / 2 - Constants.DropUnitLength * 2.5;
                            }
                            else
                            {
                                hoffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                voffset += DropPanel.InnerRect.Size.Height / 2 - Constants.DropUnitLength * 1.5;
                            }

                            if (DropPanel.Source.DragMode == DragMode.Anchor && flag)
                            {
                                _DrawTop(ctx, hoffset, voffset, false);
                            }
                        }
                    }

                    if ((Flag & DragManager.BOTTOM) != 0)
                    {
                        if ((Flag & DragManager.SPLIT) != 0)
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutCrtl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutCrtl.DockViewParent != null)
                                {
                                    layoutpanel = layoutCrtl.DockViewParent as LayoutGroupPanel;
                                    flag &= layoutpanel.Direction != Direction.Horizontal && layoutCrtl.ChildrenCount > 0;
                                }

                                if (flag)
                                {
                                    hoffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                    voffset += (DropPanel.InnerRect.Size.Height + Constants.DropUnitLength) / 2;
                                    _DrawCenter(ctx, hoffset, voffset, true);
                                }
                            }
                        }
                        else
                        {
                            if (DropPanel.Target.Mode == DragMode.Document)
                            {
                                layoutCrtl = DropPanel.Target as LayoutDocumentGroupControl;
                                if (layoutCrtl.DockViewParent != null)
                                {
                                    layoutpanel = layoutCrtl.DockViewParent as LayoutGroupPanel;
                                    if (layoutpanel.Direction == Direction.Vertical)
                                    {
                                        flag &= layoutCrtl.IndexOf() == layoutpanel.Count - 1;
                                    }
                                }

                                hoffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                voffset += DropPanel.InnerRect.Size.Height / 2 + Constants.DropUnitLength * 2.5;
                            }
                            else
                            {
                                hoffset += (DropPanel.InnerRect.Size.Width - Constants.DropUnitLength) / 2;
                                voffset += DropPanel.InnerRect.Size.Height / 2 + Constants.DropUnitLength * 1.5;
                            }

                            if (DropPanel.Source.DragMode == DragMode.Anchor && flag)
                            {
                                _DrawBottom(ctx, hoffset, voffset, false);
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

            if ((Flag & DragManager.ACTIVE) == 0)
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

            if ((Flag & DragManager.ACTIVE) == 0)
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

            if ((Flag & DragManager.ACTIVE) == 0)
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

            if ((Flag & DragManager.ACTIVE) == 0)
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

            if ((Flag & DragManager.ACTIVE) == 0)
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