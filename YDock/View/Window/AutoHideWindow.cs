﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using YDock.Enum;
using YDock.Interface;
using YDock.Model;

namespace YDock.View
{
    public class AutoHideWindow : Panel,
                                  ILayout,
                                  IDisposable
    {
        private double _dragBound1;
        private double _dragBound2;

        private Popup _dragPopup;

        private LayoutContentControl _layoutContent;

        private DockElement _model;

        private LayoutDragSplitter _splitter;
        private Point _pToScreen;

        #region Constructors

        public AutoHideWindow()
        {
            _InitChildren();
        }

        #endregion

        #region Properties

        public DockElement Model
        {
            get { return _model; }
            set
            {
                if (_model != value)
                {
                    if (_model != null)
                    {
                        _DestroyContentForModel(_model);
                    }

                    _model = value;
                    if (_model != null)
                    {
                        _CreateContentForModel(_model);
                    }

                    ((FrameworkElement)Parent).InvalidateMeasure();
                    InvalidateMeasure();
                }
            }
        }

        private Panel RootPanel
        {
            get { return ((FrameworkElement)Parent).Parent as Panel; }
        }

        #endregion

        #region Override members

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Model == null) return new Size();
            double wholeLength, finalWidth = 0, finalHeight = 0, avaLength;
            switch (Side)
            {
                case DockSide.Left:
                case DockSide.Right:
                    avaLength = Math.Max(availableSize.Width - Constants.SideLength / 2, 0);
                    wholeLength = Constants.SplitterSpan / 2 + Model.DesiredWidth;
                    if (wholeLength <= avaLength)
                    {
                        _layoutContent.Measure(new Size(Model.DesiredWidth, availableSize.Height));
                        _splitter.Measure(new Size(Constants.SplitterSpan / 2, availableSize.Height));
                        finalWidth = wholeLength;
                        finalHeight = availableSize.Height;
                    }
                    else
                    {
                        _layoutContent.Measure(new Size(Math.Max(0, avaLength - Constants.SplitterSpan / 2), availableSize.Height));
                        _splitter.Measure(new Size(Constants.SplitterSpan / 2, availableSize.Height));
                        finalWidth = avaLength;
                        finalHeight = availableSize.Height;
                    }

                    break;
                case DockSide.Top:
                case DockSide.Bottom:
                    avaLength = Math.Max(availableSize.Height - Constants.SideLength / 2, 0);
                    wholeLength = Constants.SplitterSpan / 2 + Model.DesiredHeight;
                    if (wholeLength <= avaLength)
                    {
                        _layoutContent.Measure(new Size(availableSize.Width, Model.DesiredHeight));
                        _splitter.Measure(new Size(availableSize.Width, Constants.SplitterSpan / 2));
                        finalWidth = availableSize.Width;
                        finalHeight = wholeLength;
                    }
                    else
                    {
                        _layoutContent.Measure(new Size(availableSize.Width, Math.Max(0, avaLength - Constants.SplitterSpan / 2)));
                        _splitter.Measure(new Size(availableSize.Width, Constants.SplitterSpan / 2));
                        finalWidth = availableSize.Width;
                        finalHeight = avaLength;
                    }

                    break;
            }

            return new Size(finalWidth, finalHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Model == null)
            {
                _layoutContent.Arrange(new Rect());
                _splitter.Arrange(new Rect());
            }
            else
            {
                double wholeLength;
                switch (Side)
                {
                    case DockSide.Left:
                    case DockSide.Right:
                        wholeLength = Constants.SplitterSpan / 2 + Model.DesiredWidth;
                        if (wholeLength <= finalSize.Width)
                        {
                            if (Side == DockSide.Left)
                            {
                                _layoutContent.Arrange(new Rect(new Point(0, 0), new Size(Model.DesiredWidth, finalSize.Height)));
                                _splitter.Arrange(new Rect(new Point(Model.DesiredWidth, 0), new Size(Constants.SplitterSpan / 2, finalSize.Height)));
                            }
                            else
                            {
                                _splitter.Arrange(new Rect(new Point(0, 0), new Size(Constants.SplitterSpan / 2, finalSize.Height)));
                                _layoutContent.Arrange(new Rect(new Point(Constants.SplitterSpan / 2, 0), new Size(Model.DesiredWidth, finalSize.Height)));
                            }
                        }
                        else
                        {
                            var deceed = wholeLength - finalSize.Width;
                            if (Side == DockSide.Left)
                            {
                                if (Model.DesiredWidth > deceed)
                                {
                                    _layoutContent.Arrange(new Rect(new Point(0, 0), new Size(Model.DesiredWidth - deceed, finalSize.Height)));
                                    _splitter.Arrange(new Rect(new Point(Model.DesiredWidth - deceed, 0), new Size(Constants.SplitterSpan / 2, finalSize.Height)));
                                }
                                else
                                {
                                    _layoutContent.Arrange(new Rect(new Point(0, 0), new Size(0, finalSize.Height)));
                                    _splitter.Arrange(new Rect(new Point(0, 0), new Size(Math.Max(0, Constants.SplitterSpan / 2 - deceed), finalSize.Height)));
                                }
                            }
                            else
                            {
                                var useLength = finalSize.Width;
                                if (useLength >= Constants.SplitterSpan / 2)
                                {
                                    useLength -= Constants.SplitterSpan / 2;
                                    _splitter.Arrange(new Rect(new Point(0, 0), new Size(Constants.SplitterSpan / 2, finalSize.Height)));
                                    _layoutContent.Arrange(new Rect(new Point(Constants.SplitterSpan / 2, 0), new Size(useLength, finalSize.Height)));
                                }
                                else
                                {
                                    _splitter.Arrange(new Rect(new Point(0, 0), new Size(useLength, finalSize.Height)));
                                    _layoutContent.Arrange(new Rect());
                                }
                            }
                        }

                        break;
                    case DockSide.Top:
                    case DockSide.Bottom:
                        wholeLength = Constants.SplitterSpan / 2 + Model.DesiredHeight;
                        if (wholeLength <= finalSize.Height)
                        {
                            if (Side == DockSide.Top)
                            {
                                _layoutContent.Arrange(new Rect(new Point(0, 0), new Size(finalSize.Width, Model.DesiredHeight)));
                                _splitter.Arrange(new Rect(new Point(0, Model.DesiredHeight), new Size(finalSize.Width, Constants.SplitterSpan / 2)));
                            }
                            else
                            {
                                _splitter.Arrange(new Rect(new Point(0, 0), new Size(finalSize.Width, Constants.SplitterSpan / 2)));
                                _layoutContent.Arrange(new Rect(new Point(0, Constants.SplitterSpan / 2), new Size(finalSize.Width, Model.DesiredHeight)));
                            }
                        }
                        else
                        {
                            var deceed = wholeLength - finalSize.Height;
                            if (Side == DockSide.Top)
                            {
                                if (Model.DesiredHeight > deceed)
                                {
                                    _layoutContent.Arrange(new Rect(new Point(0, 0), new Size(finalSize.Width, Model.DesiredHeight - deceed)));
                                    _splitter.Arrange(new Rect(new Point(0, Model.DesiredHeight - deceed), new Size(finalSize.Width, Constants.SplitterSpan / 2)));
                                }
                                else
                                {
                                    _layoutContent.Arrange(new Rect(new Point(0, 0), new Size(finalSize.Width, 0)));
                                    _splitter.Arrange(new Rect(new Point(0, 0), new Size(finalSize.Width, Math.Max(0, Constants.SplitterSpan / 2 - deceed))));
                                }
                            }
                            else
                            {
                                var useLength = finalSize.Height;
                                if (useLength >= Constants.SplitterSpan / 2)
                                {
                                    useLength -= Constants.SplitterSpan / 2;
                                    _splitter.Arrange(new Rect(new Point(0, 0), new Size(finalSize.Width, Constants.SplitterSpan / 2)));
                                    _layoutContent.Arrange(new Rect(new Point(0, Constants.SplitterSpan / 2), new Size(finalSize.Width, useLength)));
                                }
                                else
                                {
                                    _splitter.Arrange(new Rect(new Point(0, 0), new Size(finalSize.Width, useLength)));
                                    _layoutContent.Arrange(new Rect());
                                }
                            }
                        }

                        break;
                }
            }

            return finalSize;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Model = null;
            _splitter.DragStarted -= OnDragStarted;
            _splitter.DragDelta -= OnDragDelta;
            _splitter.DragCompleted -= OnDragCompleted;
            _splitter = null;
            _layoutContent.Dispose();
        }

        #endregion

        #region ILayout Members

        public DockSide Side
        {
            get { return _model == null ? DockSide.None : _model.Side; }
        }

        public DockManager DockManager
        {
            get { return _model == null ? null : _model.DockManager; }
        }

        #endregion

        #region Event handlers

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (Side == DockSide.Left)
            {
                Model.DesiredWidth += _dragPopup.HorizontalOffset - _pToScreen.X;
            }

            if (Side == DockSide.Right)
            {
                Model.DesiredWidth -= _dragPopup.HorizontalOffset - _pToScreen.X;
            }

            if (Side == DockSide.Top)
            {
                Model.DesiredHeight += _dragPopup.VerticalOffset - _pToScreen.Y;
            }

            if (Side == DockSide.Bottom)
            {
                Model.DesiredHeight -= _dragPopup.VerticalOffset - _pToScreen.Y;
            }

            (Parent as FrameworkElement).InvalidateMeasure();
            InvalidateMeasure();
            _DisposeDragPopup();
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Side == DockSide.Left || Side == DockSide.Right)
            {
                if (e.HorizontalChange != 0)
                {
                    var newPos = _pToScreen.X + e.HorizontalChange;
                    if (_dragBound1 + Constants.SideLength >= _dragBound2 - Constants.SideLength) return;
                    if (newPos >= _dragBound1 + Constants.SideLength && newPos <= _dragBound2 - Constants.SideLength)
                    {
                        _dragPopup.HorizontalOffset = newPos;
                    }
                    else
                    {
                        if (e.HorizontalChange > 0)
                        {
                            _dragPopup.HorizontalOffset = _dragBound2 - Constants.SideLength;
                        }
                        else
                        {
                            _dragPopup.HorizontalOffset = _dragBound1 + Constants.SideLength;
                        }
                    }
                }
                else
                {
                    _dragPopup.HorizontalOffset = _pToScreen.X;
                }
            }
            else
            {
                if (e.VerticalChange != 0)
                {
                    var newPos = _pToScreen.Y + e.VerticalChange;
                    if (_dragBound1 + Constants.SideLength >= _dragBound2 - Constants.SideLength) return;
                    if (newPos >= _dragBound1 + Constants.SideLength && newPos <= _dragBound2 - Constants.SideLength)
                    {
                        _dragPopup.VerticalOffset = newPos;
                    }
                    else
                    {
                        if (e.VerticalChange > 0)
                        {
                            _dragPopup.VerticalOffset = _dragBound2 - Constants.SideLength;
                        }
                        else
                        {
                            _dragPopup.VerticalOffset = _dragBound1 + Constants.SideLength;
                        }
                    }
                }
                else
                {
                    _dragPopup.VerticalOffset = _pToScreen.Y;
                }
            }
        }

        private void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            _ComputeDragBounds(sender as LayoutDragSplitter, ref _dragBound1, ref _dragBound2);
            _CreateDragPopup(sender as LayoutDragSplitter);
        }

        #endregion

        #region Members

        /// <summary>
        ///     计算拖动时的上下边界值
        /// </summary>
        /// <param name="splitter">拖动的对象</param>
        /// <param name="x1">下界</param>
        /// <param name="x2">上界</param>
        private void _ComputeDragBounds(LayoutDragSplitter splitter, ref double x1, ref double x2)
        {
            var pToScreen = RootPanel.PointToScreenDPIWithoutFlowDirection(new Point());
            switch (Side)
            {
                case DockSide.Left:
                case DockSide.Right:
                    _dragBound1 = pToScreen.X;
                    _dragBound2 = pToScreen.X + RootPanel.ActualWidth - Constants.SplitterSpan / 2;
                    break;
                case DockSide.Top:
                case DockSide.Bottom:
                    _dragBound1 = pToScreen.Y;
                    _dragBound2 = pToScreen.Y + RootPanel.ActualHeight - Constants.SplitterSpan / 2;
                    break;
            }
        }

        private void _CreateContentForModel(DockElement model)
        {
            _layoutContent.Model = model;
            switch (model.Side)
            {
                case DockSide.Left:
                case DockSide.Right:
                    _splitter.Cursor = Cursors.SizeWE;
                    break;
                case DockSide.Top:
                case DockSide.Bottom:
                    _splitter.Cursor = Cursors.SizeNS;
                    break;
            }

            Children.Add(_layoutContent);
            Children.Add(_splitter);
        }

        private void _CreateDragPopup(LayoutDragSplitter splitter)
        {
            _pToScreen = this.PointToScreenDPIWithoutFlowDirection(new Point());
            var transfrom = splitter.TransformToAncestor(this);
            var _pToInterPanel = transfrom.Transform(new Point(0, 0));
            _pToScreen.X += _pToInterPanel.X;
            _pToScreen.Y += _pToInterPanel.Y;

            //switch (Side)
            //{
            //    case DockSide.Left:
            //    case DockSide.Right:
            //        Model.DesiredWidth = ActualWidth - Constants.SplitterSpan / 2;
            //        break;
            //    case DockSide.Top:
            //    case DockSide.Bottom:
            //        Model.DesiredHeight = ActualHeight - Constants.SplitterSpan / 2;
            //        break;
            //}

            _dragPopup = new Popup
            {
                Child = new Rectangle
                {
                    Height = splitter.ActualHeight,
                    Width = splitter.ActualWidth,
                    Fill = Brushes.Black,
                    Opacity = Constants.DragOpacity,
                    IsHitTestVisible = false
                },
                Placement = PlacementMode.Absolute,
                HorizontalOffset = _pToScreen.X,
                VerticalOffset = _pToScreen.Y,
                AllowsTransparency = true
            };

            DockHelper.ComputeSpliterLocation(_dragPopup, _pToScreen, new Size(splitter.ActualWidth, splitter.ActualHeight));
            _dragPopup.IsOpen = true;
        }

        private void _DestroyContentForModel(DockElement Model)
        {
            if (ActualWidth > 0)
            {
                Model.DesiredWidth = ActualWidth - Constants.SplitterSpan / 2;
                Model.DesiredHeight = ActualHeight - Constants.SplitterSpan / 2;
            }

            _layoutContent.Model = null;
            Children.Clear();
        }

        private void _DisposeDragPopup()
        {
            _dragPopup.IsOpen = false;
            _dragPopup = null;
        }

        private void _InitChildren()
        {
            _layoutContent = new LayoutContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, BorderThickness = new Thickness(1), BorderBrush =
                    new SolidColorBrush(new Color
                                            { A = 0xFF, R = 0xCC, G = 0xCE, B = 0xDB })
            };
            _splitter = new LayoutDragSplitter();
            _splitter.DragStarted += OnDragStarted;
            _splitter.DragDelta += OnDragDelta;
            _splitter.DragCompleted += OnDragCompleted;
            _splitter.Background = ResourceManager.SplitterBrushVertical;
        }

        #endregion
    }
}