using System.Windows;
using System.Windows.Media;
using YDock.Enum;
using YDock.Interface;

namespace YDock.View
{
    public class BaseDropPanel : BaseRenderPanel
    {
        #region Static members

        internal static BaseDropVisual ActiveVisual
        {
            get { return _activeVisual; }
            set
            {
                if (_activeVisual != value)
                {
                    if (_activeVisual != null)
                    {
                        _activeVisual.Flag &= ~DragManager.ACTIVE;
                        _activeVisual.Update();
                    }

                    _activeVisual = value;
                    if (_activeVisual != null)
                    {
                        _activeVisual.Flag |= DragManager.ACTIVE;
                        _activeVisual.Update();
                    }
                }
            }
        }

        internal static ActiveRectDropVisual CurrentRect
        {
            get { return _currentRect; }
            set
            {
                if (_currentRect != value)
                {
                    if (_currentRect != null)
                    {
                        _currentRect.DropPanel._target.DropMode = DropMode.None;
                        _currentRect.Flag = DragManager.NONE;
                        _currentRect.Update();
                    }

                    _currentRect = value;
                    if (_currentRect != null)
                    {
                        _currentRect.Update();
                    }
                }
                else
                {
                    _currentRect?.Update();
                }
            }
        }

        #endregion

        protected DragItem _source;

        protected IDragTarget _target;

        #region Constructors

        internal BaseDropPanel(IDragTarget target, DragItem source)
        {
            _target = target;
            _source = source;
            //绘制停靠的区域
            ActiveRect = new ActiveRectDropVisual(DragManager.NONE);
            AddChild(ActiveRect);
        }

        #endregion

        #region Properties

        public ActiveRectDropVisual ActiveRect { get; private set; }

        public Rect InnerRect { get; set; }

        public Rect OuterRect { get; set; }

        /// <summary>
        ///     拖放源
        /// </summary>
        internal DragItem Source
        {
            get { return _source; }
        }

        /// <summary>
        ///     拖放目标
        /// </summary>
        internal IDragTarget Target
        {
            get { return _target; }
        }

        #endregion

        #region Override members

        public override void Dispose()
        {
            _target = null;
            _source = null;
            ActiveRect = null;
            base.Dispose();
        }

        #endregion

        #region Members

        public void Update(Point mouseP)
        {
            var p = this.PointToScreenDPIWithoutFlowDirection(new Point());
            var result = VisualTreeHelper.HitTest(this, new Point(mouseP.X - p.X, mouseP.Y - p.Y));
            if (result?.VisualHit != null && result?.VisualHit is UnitDropVisual)
            {
                var visual = result?.VisualHit as UnitDropVisual;
                ActiveVisual = visual;
                var mode = _GetMode(visual.Flag);
                if (mode == _target.DropMode)
                {
                    return;
                }

                _target.DropMode = mode;

                ActiveRect.Flag = visual.Flag;
                ActiveRect.Rect = Rect.Empty;
            }
            else
            {
                ActiveVisual = null;
                if (_target is BaseGroupControl)
                {
                    (_target as BaseGroupControl).HitTest(mouseP, ActiveRect);
                    if (!(_target as BaseGroupControl).canUpdate)
                    {
                        return;
                    }

                    _target.DropMode = _GetMode(ActiveRect.Flag);
                }
                else
                {
                    _target.DropMode = DropMode.None;
                    ActiveRect.Flag = DragManager.NONE;
                }
            }

            if (this is RootDropPanel)
            {
                _target.DockManager.DragManager.IsDragOverRoot = ActiveVisual != null;
                if (_target.DropMode != DropMode.None && _currentRect != null)
                {
                    _currentRect.Flag = DragManager.NONE;
                    _currentRect.DropPanel._target.DropMode = DropMode.None;
                    _currentRect.Update();
                }

                ActiveRect.Update();
            }

            if (!(this is RootDropPanel))
            {
                CurrentRect = ActiveRect;
            }
        }

        private DropMode _GetMode(int flag)
        {
            if ((flag & DragManager.HEAD) != 0)
            {
                return DropMode.Header;
            }

            if ((flag & DragManager.LEFT) != 0)
            {
                if ((flag & DragManager.SPLIT) != 0)
                {
                    return DropMode.Left_WithSplit;
                }

                return DropMode.Left;
            }

            if ((flag & DragManager.RIGHT) != 0)
            {
                if ((flag & DragManager.SPLIT) != 0)
                {
                    return DropMode.Right_WithSplit;
                }

                return DropMode.Right;
            }

            if ((flag & DragManager.TOP) != 0)
            {
                if ((flag & DragManager.SPLIT) != 0)
                {
                    return DropMode.Top_WithSplit;
                }

                return DropMode.Top;
            }

            if ((flag & DragManager.BOTTOM) != 0)
            {
                if ((flag & DragManager.SPLIT) != 0)
                {
                    return DropMode.Bottom_WithSplit;
                }

                return DropMode.Bottom;
            }

            if ((flag & DragManager.CENTER) != 0)
            {
                return DropMode.Center;
            }

            return DropMode.None;
        }

        #endregion

        private static BaseDropVisual _activeVisual;

        private static ActiveRectDropVisual _currentRect;
    }
}