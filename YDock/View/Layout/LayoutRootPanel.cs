using System.Windows;
using System.Windows.Controls;
using YDock.Enum;
using YDock.Interface;
using YDock.Model;
using YDock.View.Control;
using YDock.View.Window;

namespace YDock.View.Layout
{
    /// <summary>
    ///     Used to hold <see cref="LayoutGroupPanel" />, as well as AutoHideWindow
    /// </summary>
    public class LayoutRootPanel : Panel,
                                   IDockView,
                                   ILayoutViewParent
    {
        private Win32Window _autoHideWindow;
        private IDockModel _model;
        private LayoutGroupPanel _rootGroupPanel;

        #region Constructors

        static LayoutRootPanel()
        {
            FocusableProperty.OverrideMetadata(typeof(LayoutRootPanel), new FrameworkPropertyMetadata(false));
        }

        internal LayoutRootPanel(IDockModel model)
        {
            Model = model;
            _InitContent();
        }

        #endregion

        #region Properties

        public Win32Window AutoHideWindow
        {
            get { return _autoHideWindow; }
            set
            {
                if (_autoHideWindow != value)
                {
                    if (_autoHideWindow != null)
                    {
                        Children.Remove(_autoHideWindow);
                        _autoHideWindow.Dispose();
                    }

                    _autoHideWindow = value;
                    if (_autoHideWindow != null)
                    {
                        Children.Add(_autoHideWindow);
                        SetZIndex(_autoHideWindow, 2);
                    }
                }
            }
        }

        public LayoutGroupPanel RootGroupPanel
        {
            get { return _rootGroupPanel; }
            internal set
            {
                if (_rootGroupPanel != value)
                {
                    if (_rootGroupPanel != null)
                    {
                        Children.Remove(_rootGroupPanel);
                        _rootGroupPanel.CloseDropWindow();
                    }

                    _rootGroupPanel = value;
                    if (_rootGroupPanel != null)
                    {
                        Children.Add(_rootGroupPanel);
                    }
                }
            }
        }

        #endregion

        #region Override members

        protected override Size MeasureOverride(Size availableSize)
        {
            _rootGroupPanel.Measure(new Size(availableSize.Width - 10, availableSize.Height - 10));
            switch (_autoHideWindow.Side)
            {
                case DockSide.Right:
                case DockSide.Left:
                    _autoHideWindow.Measure(new Size(availableSize.Width, availableSize.Height - 10));
                    break;
                case DockSide.Top:
                case DockSide.Bottom:
                    _autoHideWindow.Measure(new Size(availableSize.Width - 10, availableSize.Height));
                    break;
                default:
                    _autoHideWindow.Measure(availableSize);
                    break;
            }

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _rootGroupPanel.Arrange(new Rect(new Point(5, 5), new Size(finalSize.Width - 10, finalSize.Height - 10)));
            switch (_autoHideWindow.Side)
            {
                case DockSide.Left:
                    _autoHideWindow.Arrange(new Rect(new Point(2, 5), _autoHideWindow.DesiredSize));
                    break;
                case DockSide.Right:
                    _autoHideWindow.Arrange(new Rect(new Point(finalSize.Width - _autoHideWindow.DesiredSize.Width - 2, 5), _autoHideWindow.DesiredSize));
                    break;
                case DockSide.Top:
                    _autoHideWindow.Arrange(new Rect(new Point(5, 2), _autoHideWindow.DesiredSize));
                    break;
                case DockSide.Bottom:
                    _autoHideWindow.Arrange(new Rect(new Point(5, finalSize.Height - _autoHideWindow.DesiredSize.Height - 2), _autoHideWindow.DesiredSize));
                    break;
                case DockSide.None:
                    _autoHideWindow.Arrange(new Rect());
                    break;
            }

            return finalSize;
        }

        #endregion

        #region IDockView Members

        public IDockModel Model
        {
            get { return _model; }
            internal set
            {
                if (_model == value) return;
                if (_model != null)
                {
                    ((DockRoot)_model).View = null;
                }

                _model = value;
                if (_model != null)
                {
                    ((DockRoot)_model).View = this;
                }
            }
        }

        public IDockView DockViewParent
        {
            get { return _model.DockManager; }
        }

        public void Dispose()
        {
            Model = null;
            RootGroupPanel.Dispose();
            RootGroupPanel = null;
            AutoHideWindow = null;
            Children.Clear();
        }

        #endregion

        #region ILayoutViewParent Members

        public void AttachChild(IDockView child, AttachMode mode, int index)
        {
            if (child is LayoutGroupPanel panel)
            {
                RootGroupPanel = panel;
            }
        }

        public void DetachChild(IDockView child, bool force = true)
        {
            if (Equals(child, RootGroupPanel))
            {
                RootGroupPanel = null;
            }
        }

        public int IndexOf(IDockView child)
        {
            return Equals(child, RootGroupPanel) ? 0 : -1;
        }

        #endregion

        #region Members

        internal IDockView FindChildByLevel(int level, DockSide side)
        {
            IDockView view = _rootGroupPanel;
            while (level > 0)
            {
                level--;
                if (view is BaseGroupControl)
                {
                    break;
                }

                if (view is LayoutGroupPanel panel)
                {
                    if (panel.Direction == Direction.None) break;

                    if (panel.Direction == Direction.Horizontal)
                    {
                        if (side == DockSide.Top || side == DockSide.Bottom)
                        {
                            break;
                        }

                        if (side == DockSide.Left)
                        {
                            var child = panel.Children[0];
                            if (child is LayoutDocumentGroupControl)
                            {
                                break;
                            }

                            view = child as IDockView;
                        }

                        if (side == DockSide.Right)
                        {
                            var child = panel.Children[panel.Count - 1];
                            if (child is LayoutDocumentGroupControl)
                            {
                                break;
                            }

                            view = child as IDockView;
                        }
                    }
                    else
                    {
                        if (side == DockSide.Left || side == DockSide.Right)
                        {
                            break;
                        }

                        if (side == DockSide.Top)
                        {
                            var child = panel.Children[0];
                            if (child is LayoutDocumentGroupControl)
                            {
                                break;
                            }

                            view = child as IDockView;
                        }

                        if (side == DockSide.Bottom)
                        {
                            var child = panel.Children[panel.Count - 1];
                            if (child is LayoutDocumentGroupControl)
                            {
                                break;
                            }

                            view = child as IDockView;
                        }
                    }
                }
            }

            return view;
        }

        private void _InitContent()
        {
            AutoHideWindow = new Win32Window();
            //Initialize the Document area first
            RootGroupPanel = new LayoutGroupDocumentPanel();
            var documentControl = new LayoutDocumentGroupControl(((DockRoot)_model).DocumentModels[0]);
            RootGroupPanel.AttachChild(documentControl, 0);
        }

        #endregion
    }
}