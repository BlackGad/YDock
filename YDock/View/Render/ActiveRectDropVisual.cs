using System;
using System.Windows;
using System.Windows.Media;
using YDock.Enum;
using YDock.View.Control;
// ReSharper disable RedundantAssignment

namespace YDock.View.Render
{
    public class ActiveRectDropVisual : BaseDropVisual
    {
        internal Rect Rect;

        #region Constructors

        internal ActiveRectDropVisual(int flag) : base(flag)
        {
        }

        #endregion

        #region Override members

        public override void Update(Size size)
        {
            using (var ctx = RenderOpen())
            {
                if (Flag != DragManager.NONE)
                {
                    ctx.PushOpacity(Constants.DragOpacity);
                    if (Rect.IsEmpty)
                    {
                        if (DropPanel.Target is LayoutDocumentGroupControl)
                        {
                            var innerLeft = DropPanel.InnerRect.Left + DropPanel.OuterRect.Left;
                            var innerTop = DropPanel.InnerRect.Top + DropPanel.OuterRect.Top;
                            if ((Flag & DragManager.LEFT) != 0)
                            {
                                if ((Flag & DragManager.Split) != 0)
                                {
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(innerLeft, innerTop, DropPanel.InnerRect.Size.Width / 2, DropPanel.InnerRect.Size.Height));
                                }
                                else
                                {
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(DropPanel.OuterRect.Left,
                                                               DropPanel.OuterRect.Top,
                                                               Math.Min(DropPanel.OuterRect.Size.Width / 2, DropPanel.Source.Size.Width),
                                                               DropPanel.OuterRect.Size.Height));
                                }
                            }

                            if ((Flag & DragManager.Top) != 0)
                            {
                                if ((Flag & DragManager.Split) != 0)
                                {
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(innerLeft, innerTop, DropPanel.InnerRect.Size.Width, DropPanel.InnerRect.Size.Height / 2));
                                }
                                else
                                {
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(DropPanel.OuterRect.Left,
                                                               DropPanel.OuterRect.Top,
                                                               DropPanel.OuterRect.Size.Width,
                                                               Math.Min(DropPanel.OuterRect.Size.Height / 2, DropPanel.Source.Size.Height)));
                                }
                            }

                            if ((Flag & DragManager.Right) != 0)
                            {
                                if ((Flag & DragManager.Split) != 0)
                                {
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(innerLeft + DropPanel.InnerRect.Size.Width / 2,
                                                               innerTop,
                                                               DropPanel.InnerRect.Size.Width / 2,
                                                               DropPanel.InnerRect.Size.Height));
                                }
                                else
                                {
                                    var length = Math.Min(DropPanel.OuterRect.Size.Width / 2, DropPanel.Source.Size.Width);
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(DropPanel.OuterRect.Left + DropPanel.OuterRect.Size.Width - length,
                                                               DropPanel.OuterRect.Top,
                                                               length,
                                                               DropPanel.OuterRect.Size.Height));
                                }
                            }

                            if ((Flag & DragManager.Bottom) != 0)
                            {
                                if ((Flag & DragManager.Split) != 0)
                                {
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(innerLeft,
                                                               innerTop + DropPanel.InnerRect.Size.Height / 2,
                                                               DropPanel.InnerRect.Size.Width,
                                                               DropPanel.InnerRect.Size.Height / 2));
                                }
                                else
                                {
                                    var length = Math.Min(DropPanel.OuterRect.Size.Height / 2, DropPanel.Source.Size.Height);
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(DropPanel.OuterRect.Left,
                                                               DropPanel.OuterRect.Top + DropPanel.OuterRect.Size.Height - length,
                                                               DropPanel.OuterRect.Size.Width,
                                                               length));
                                }
                            }
                        }
                        else
                        {
                            if ((Flag & DragManager.LEFT) != 0)
                            {
                                if ((Flag & DragManager.Split) != 0)
                                {
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(DropPanel.InnerRect.Left,
                                                               DropPanel.InnerRect.Top,
                                                               DropPanel.InnerRect.Size.Width / 2,
                                                               DropPanel.InnerRect.Size.Height));
                                }
                                else
                                {
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(DropPanel.InnerRect.Left,
                                                               DropPanel.InnerRect.Top,
                                                               Math.Min(DropPanel.InnerRect.Size.Width / 2, DropPanel.Source.Size.Width),
                                                               DropPanel.InnerRect.Size.Height));
                                }
                            }

                            if ((Flag & DragManager.Top) != 0)
                            {
                                if ((Flag & DragManager.Split) != 0)
                                {
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(DropPanel.InnerRect.Left,
                                                               DropPanel.InnerRect.Top,
                                                               DropPanel.InnerRect.Size.Width,
                                                               DropPanel.InnerRect.Size.Height / 2));
                                }
                                else
                                {
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(DropPanel.InnerRect.Left,
                                                               DropPanel.InnerRect.Top,
                                                               DropPanel.InnerRect.Size.Width,
                                                               Math.Min(DropPanel.InnerRect.Size.Height / 2, DropPanel.Source.Size.Height)));
                                }
                            }

                            if ((Flag & DragManager.Right) != 0)
                            {
                                if ((Flag & DragManager.Split) != 0)
                                {
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(DropPanel.InnerRect.Left + DropPanel.InnerRect.Size.Width / 2,
                                                               DropPanel.InnerRect.Top,
                                                               DropPanel.InnerRect.Size.Width / 2,
                                                               DropPanel.InnerRect.Size.Height));
                                }
                                else
                                {
                                    var length = Math.Min(DropPanel.InnerRect.Size.Width / 2, DropPanel.Source.Size.Width);
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(DropPanel.InnerRect.Left + DropPanel.InnerRect.Size.Width - length,
                                                               DropPanel.InnerRect.Top,
                                                               length,
                                                               DropPanel.InnerRect.Size.Height));
                                }
                            }

                            if ((Flag & DragManager.Bottom) != 0)
                            {
                                if ((Flag & DragManager.Split) != 0)
                                {
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(DropPanel.InnerRect.Left,
                                                               DropPanel.InnerRect.Top + DropPanel.InnerRect.Size.Height / 2,
                                                               DropPanel.InnerRect.Size.Width,
                                                               DropPanel.InnerRect.Size.Height / 2));
                                }
                                else
                                {
                                    var length = Math.Min(DropPanel.InnerRect.Size.Height / 2, DropPanel.Source.Size.Height);
                                    ctx.DrawRectangle(ResourceManager.RectBrush,
                                                      ResourceManager.RectBorderPen,
                                                      new Rect(DropPanel.InnerRect.Left,
                                                               DropPanel.InnerRect.Top + DropPanel.InnerRect.Size.Height - length,
                                                               DropPanel.InnerRect.Size.Width,
                                                               length));
                                }
                            }
                        }

                        if ((Flag & DragManager.Center) != 0)
                        {
                            var stream = new StreamGeometry();
                            using (var context = stream.Open())
                            {
                                double currentX = DropPanel.InnerRect.Left + DropPanel.OuterRect.Left, currentY = DropPanel.InnerRect.Top + DropPanel.OuterRect.Top;
                                context.BeginFigure(new Point(currentX, currentY), true, false);
                                if (DropPanel.Target.Mode == DragMode.Anchor)
                                {
                                    context.LineTo(new Point(currentX += DropPanel.InnerRect.Size.Width, currentY), true, false);
                                    if (DropPanel.InnerRect.Size.Width < 60)
                                    {
                                        context.LineTo(new Point(currentX, currentY += DropPanel.InnerRect.Size.Height), true, false);
                                        context.LineTo(new Point(currentX -= DropPanel.InnerRect.Size.Width, currentY), true, false);
                                    }
                                    else
                                    {
                                        context.LineTo(new Point(currentX, currentY += DropPanel.InnerRect.Size.Height - 20), true, false);
                                        context.LineTo(new Point(currentX -= DropPanel.InnerRect.Size.Width - 60, currentY), true, false);
                                        context.LineTo(new Point(currentX, currentY += 20), true, false);
                                        context.LineTo(new Point(currentX -= 60, currentY), true, false);
                                    }

                                    context.LineTo(new Point(currentX, currentY -= DropPanel.InnerRect.Size.Height), true, false);
                                }
                                else
                                {
                                    if (DropPanel.InnerRect.Size.Width < 120)
                                    {
                                        context.LineTo(new Point(currentX += DropPanel.InnerRect.Size.Width, currentY), true, false);
                                        context.LineTo(new Point(currentX, currentY += DropPanel.InnerRect.Size.Height), true, false);
                                    }
                                    else
                                    {
                                        context.LineTo(new Point(currentX += 120, currentY), true, false);
                                        context.LineTo(new Point(currentX, currentY += 22), true, false);
                                        context.LineTo(new Point(currentX += DropPanel.InnerRect.Size.Width - 120, currentY), true, false);
                                        context.LineTo(new Point(currentX, currentY += DropPanel.InnerRect.Size.Height - 22), true, false);
                                    }

                                    context.LineTo(new Point(currentX -= DropPanel.InnerRect.Size.Width, currentY), true, false);
                                    context.LineTo(new Point(currentX, currentY -= DropPanel.InnerRect.Size.Height), true, false);
                                }

                                context.Close();
                            }

                            ctx.DrawGeometry(ResourceManager.RectBrush, ResourceManager.RectBorderPen, stream);
                        }
                    }
                    else
                    {
                        var stream = new StreamGeometry();
                        using (var context = stream.Open())
                        {
                            double currentX = DropPanel.InnerRect.Left + DropPanel.OuterRect.Left, currentY = DropPanel.InnerRect.Top + DropPanel.OuterRect.Top;
                            if (DropPanel.Target.Mode == DragMode.Anchor)
                            {
                                context.BeginFigure(new Point(currentX, currentY), true, false);
                                if (DropPanel.InnerRect.Size.Width < Rect.X + Rect.Width)
                                {
                                    Rect.Width = DropPanel.InnerRect.Size.Width - Rect.X;
                                }

                                context.LineTo(new Point(currentX, currentY += DropPanel.InnerRect.Size.Height - 20), true, false);
                                context.LineTo(new Point(currentX += Rect.X, currentY), true, false);
                                context.LineTo(new Point(currentX, currentY += 20), true, false);
                                context.LineTo(new Point(currentX += Rect.Width, currentY), true, false);
                                context.LineTo(new Point(currentX, currentY -= 20), true, false);
                                context.LineTo(new Point(currentX += DropPanel.InnerRect.Size.Width - Rect.Width - Rect.X, currentY), true, false);
                                context.LineTo(new Point(currentX, currentY -= DropPanel.InnerRect.Size.Height - 20), true, false);
                                context.LineTo(new Point(currentX -= DropPanel.InnerRect.Size.Width, currentY), true, false);
                            }
                            else
                            {
                                currentY += DropPanel.InnerRect.Size.Height;
                                context.BeginFigure(new Point(currentX, currentY), true, false);
                                if (DropPanel.InnerRect.Size.Width < Rect.X + Rect.Width)
                                {
                                    Rect.Width = DropPanel.InnerRect.Size.Width - Rect.X;
                                }

                                context.LineTo(new Point(currentX, currentY -= DropPanel.InnerRect.Size.Height - 22), true, false);
                                context.LineTo(new Point(currentX += Rect.X, currentY), true, false);
                                context.LineTo(new Point(currentX, currentY -= 22), true, false);
                                context.LineTo(new Point(currentX += Rect.Width, currentY), true, false);
                                context.LineTo(new Point(currentX, currentY += 22), true, false);
                                context.LineTo(new Point(currentX += DropPanel.InnerRect.Size.Width - Rect.Width - Rect.X, currentY), true, false);
                                context.LineTo(new Point(currentX, currentY += DropPanel.InnerRect.Size.Height - 22), true, false);
                                context.LineTo(new Point(currentX -= DropPanel.InnerRect.Size.Width, currentY), true, false);
                            }

                            context.Close();
                        }

                        ctx.DrawGeometry(ResourceManager.RectBrush, ResourceManager.RectBorderPen, stream);
                    }
                }
            }
        }

        #endregion
    }
}