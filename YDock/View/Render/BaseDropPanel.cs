using System.Windows;
using System.Windows.Media;
using YDock.Enum;
using YDock.Interface;
using YDock.View.Control;

namespace YDock.View.Render
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
                        _activeVisual.Flag &= ~DragManager.Active;
                        _activeVisual.Update();
                    }

                    _activeVisual = value;
                    if (_activeVisual != null)
                    {
                        _activeVisual.Flag |= DragManager.Active;
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
            //Draw the docked area
            ActiveRect = new ActiveRectDropVisual(DragManager.NONE);
            AddChild(ActiveRect);
        }

        #endregion

        #region Properties

        public ActiveRectDropVisual ActiveRect { get; private set; }

        public Rect InnerRect { get; set; }

        public Rect OuterRect { get; set; }

        /// <summary>
        ///     Drag and drop source
        /// </summary>
        internal DragItem Source
        {
            get { return _source; }
        }

        /// <summary>
        ///     Drag and drop target
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
            if (result?.VisualHit is UnitDropVisual visual)
            {
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
                if (_target is BaseGroupControl control)
                {
                    control.HitTest(mouseP, ActiveRect);
                    if (!control.canUpdate) return;

                    control.DropMode = _GetMode(ActiveRect.Flag);
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
            if ((flag & DragManager.Head) != 0)
            {
                return DropMode.Header;
            }

            if ((flag & DragManager.LEFT) != 0)
            {
                if ((flag & DragManager.Split) != 0)
                {
                    return DropMode.Left_WithSplit;
                }

                return DropMode.Left;
            }

            if ((flag & DragManager.Right) != 0)
            {
                if ((flag & DragManager.Split) != 0)
                {
                    return DropMode.Right_WithSplit;
                }

                return DropMode.Right;
            }

            if ((flag & DragManager.Top) != 0)
            {
                if ((flag & DragManager.Split) != 0)
                {
                    return DropMode.Top_WithSplit;
                }

                return DropMode.Top;
            }

            if ((flag & DragManager.Bottom) != 0)
            {
                if ((flag & DragManager.Split) != 0)
                {
                    return DropMode.Bottom_WithSplit;
                }

                return DropMode.Bottom;
            }

            if ((flag & DragManager.Center) != 0)
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