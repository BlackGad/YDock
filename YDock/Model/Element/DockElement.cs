using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using YDock.Enum;
using YDock.Interface;
using YDock.Model.Layout;
using YDock.View.Control;
using YDock.View.Window;

namespace YDock.Model.Element
{
    public class DockElement : DependencyObject,
                               IDockElement,
                               IComparable<DockElement>
    {
        #region Property definitions

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource",
                                        typeof(ImageSource),
                                        typeof(DockElement));

        public static readonly DependencyProperty SideProperty =
            DependencyProperty.Register("Side",
                                        typeof(DockSide),
                                        typeof(DockElement));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title",
                                        typeof(string),
                                        typeof(DockElement),
                                        new FrameworkPropertyMetadata(string.Empty));

        #endregion

        private bool _canSelect;

        private object _content;

        private DockControl _dockControl;

        private bool _isActive;
        private bool _isVisible;
        private DockMode _mode;

        #region Constructors

        internal DockElement(bool isDocument = false)
        {
            IsDocument = isDocument;
        }

        #endregion

        #region Properties

        public DockControl DockControl
        {
            get { return _dockControl; }
            internal set
            {
                if (_dockControl != null && _dockControl != value)
                {
                    _dockControl = value;
                }
            }
        }

        public string ToolTip
        {
            get
            {
                if (Content is IDockDocSource dockDocSource) return dockDocSource.FullFileName;
                return Title;
            }
        }

        #endregion

        #region IComparable<DockElement> Members

        public int CompareTo(DockElement other)
        {
            return string.Compare(Title, other.Title, StringComparison.InvariantCulture);
        }

        #endregion

        #region IDockElement Members

        public object Content
        {
            get { return _content; }
            internal set
            {
                if (_content != value)
                {
                    _content = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Content"));
                }
            }
        }

        public string Title
        {
            set { SetValue(TitleProperty, value); }
            get { return (string)GetValue(TitleProperty); }
        }

        public ImageSource ImageSource
        {
            set { SetValue(ImageSourceProperty, value); }
            get { return (ImageSource)GetValue(ImageSourceProperty); }
        }

        public DockSide Side
        {
            internal set { SetValue(SideProperty, value); }
            get { return (DockSide)GetValue(SideProperty); }
        }

        public int ID { get; internal set; }

        public DockMode Mode
        {
            get { return _mode; }
            internal set
            {
                if (_mode != value)
                {
                    _mode = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Mode"));
                }
            }
        }

        /// <summary>
        ///     Whether Content is visible
        /// </summary>
        public bool IsVisible
        {
            internal set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("IsVisible"));
                }
            }
            get { return _isVisible; }
        }

        /// <summary>
        ///     Whether to register in Document mode, this property will affect the mode of Dock's floating window
        /// </summary>
        public bool IsDocument { get; }

        /// <summary>
        ///     Is current window active
        /// </summary>
        public bool IsActive
        {
            internal set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("IsActive"));
                }
            }
            get { return _isActive; }
        }

        /// <summary>
        ///     Whether it is displayed in the user interface for the user to click to display, the default is false
        /// </summary>
        public bool CanSelect
        {
            internal set
            {
                if (_canSelect != value)
                {
                    _canSelect = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("CanSelect"));
                    if (_canSelect && IsDocument)
                    {
                        DockManager.PushBackwards(ID);
                    }
                }
            }
            get { return _canSelect; }
        }

        public bool IsDocked
        {
            get { return CanSelect && Container is LayoutGroup && Mode == DockMode.Normal; }
        }

        public bool IsFloat
        {
            get { return CanSelect && Container is LayoutGroup && Mode == DockMode.Float; }
        }

        public bool IsAutoHide
        {
            get { return Container != null && this == DockManager.AutoHideElement; }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public ILayoutGroup Container { get; internal set; }

        public DockManager DockManager
        {
            get { return Container?.DockManager; }
        }

        public double DesiredWidth { get; set; }

        public double DesiredHeight { get; set; }

        public double FloatLeft { get; set; }

        public double FloatTop { get; set; }

        public bool CanFloat
        {
            get
            {
                if (Container == null) return false;
                if (Mode != DockMode.Float) return true;
                if (Container.View == null) return true;
                if (Container.Children.Count > 1) return true;
                if (Container.View.DockViewParent != null) return true;
                return !_canSelect;
            }
        }

        public bool CanDock
        {
            get { return Container != null && Mode != DockMode.Normal; }
        }

        public bool CanDockAsDocument
        {
            get { return true; }
        }

        public bool CanSwitchAutoHideStatus
        {
            get { return Container != null && Mode != DockMode.Float; }
        }

        public bool CanHide
        {
            get { return true; }
        }

        public void ToFloat(bool isActive = true)
        {
            if (!CanFloat && isActive)
            {
                _dockControl.SetActive();
                return;
            }

            if (Container != null)
            {
                CanSelect = true;
                //Note switching mode
                Mode = DockMode.Float;
                var dockManager = DockManager;
                Container.Detach(this);
                Container = null;
                BaseFloatWindow wnd;
                BaseGroupControl groupControl;
                if (!IsDocument)
                {
                    var group = new LayoutGroup(Side, Mode, dockManager);
                    group.Attach(this);
                    groupControl = new AnchorSideGroupControl(group) { DesiredHeight = DesiredHeight, DesiredWidth = DesiredWidth };
                    wnd = new AnchorGroupWindow(dockManager)
                    {
                        Height = DesiredHeight,
                        Width = DesiredWidth,
                        Left = FloatLeft,
                        Top = FloatTop
                    };
                }
                else
                {
                    var group = new LayoutDocumentGroup(Mode, dockManager);
                    group.Attach(this);
                    groupControl = new LayoutDocumentGroupControl(group) { DesiredHeight = DesiredHeight, DesiredWidth = DesiredWidth };
                    wnd = new DocumentGroupWindow(dockManager)
                    {
                        Height = DesiredHeight,
                        Width = DesiredWidth,
                        Left = FloatLeft,
                        Top = FloatTop
                    };
                }

                wnd.AttachChild(groupControl, AttachMode.None, 0);
                wnd.Show();

                if (isActive)
                {
                    _dockControl.SetActive();
                }
            }
        }

        public void ToDock(bool isActive = true)
        {
            if (!CanDock && isActive)
            {
                _dockControl.SetActive();
                return;
            }

            if (Container != null)
            {
                CanSelect = true;
                Mode = DockMode.Normal;
                var dockManager = DockManager;
                var group = Container as LayoutGroup;
                if (group?.AttachObj == null || !group.AttachObj.AttachTo())
                {
                    //默认向下停靠
                    if (Side == DockSide.None)
                    {
                        Side = DockSide.Bottom;
                    }

                    Container?.Detach(this);
                    Container = null;
                    _ToRoot(dockManager);
                }

                if (isActive)
                {
                    _dockControl.SetActive();
                }
            }
        }

        public void ToDockAsDocument(bool isActive = true)
        {
            ToDockAsDocument(0, isActive);
        }

        public void ToDockAsDocument(int index, bool isActive = true)
        {
            if (!CanDockAsDocument && isActive)
            {
                _dockControl.SetActive();
                return;
            }

            if (index < 0 || index >= DockManager.DocumentTabCount)
            {
                return;
            }

            if (Container != null)
            {
                CanSelect = true;
                var dockManager = DockManager;
                Container.Detach(this);
                Container = null;
                Side = DockSide.None;
                Mode = DockMode.Normal;

                dockManager.Root.DocumentModels[index].Attach(this, 0);

                if (isActive)
                {
                    _dockControl.SetActive();
                }
            }
        }

        public void SwitchAutoHideStatus()
        {
            if (!CanSwitchAutoHideStatus) return;
            if (Container != null)
            {
                var dockManager = DockManager;
                Container.Detach(this);
                Container = null;

                if (Mode == DockMode.Normal)
                {
                    Mode = DockMode.DockBar;
                    dockManager.Root.AddSideChild(this, Side);
                }
                else if (Mode == DockMode.DockBar)
                {
                    _ToRoot(dockManager);
                }
            }
        }

        public void ToDockSide(DockSide side, bool isActive = false)
        {
            if (side != DockSide.Left && side != DockSide.Top && side != DockSide.Right && side != DockSide.Bottom) return;
            if (Container != null)
            {
                if (side != Side || Mode != DockMode.DockBar)
                {
                    var dockManager = DockManager;
                    Container.Detach(this);
                    Container = null;

                    Mode = DockMode.DockBar;
                    Side = side;
                    dockManager.Root.AddSideChild(this, Side);
                }

                if (isActive)
                {
                    _dockControl.SetActive();
                }
            }
        }

        public void Hide()
        {
            if (!CanHide) return;
            if (_isVisible && IsDocument)
            {
                IsVisible = false;
                var id = DockManager.FindVisibleControl();
                DockManager.PushBackwards(id);
                DockManager.ShowByID(id);
            }

            if (Equals(DockManager.AutoHideElement, this)) DockManager.AutoHideElement = null;

            _dockControl.SetActive(false);
            CanSelect = false;
        }

        public XElement Save()
        {
            var element = new XElement("Item");
            element.SetAttributeValue("ID", ID);
            element.SetAttributeValue("DesiredWidth", DesiredWidth);
            element.SetAttributeValue("DesiredHeight", DesiredHeight);
            element.SetAttributeValue("FloatLeft", FloatLeft);
            element.SetAttributeValue("FloatTop", FloatTop);
            element.SetAttributeValue("CanSelect", _canSelect);
            return element;
        }

        public void Load(XElement element)
        {
            double.TryParse(element.Attribute("DesiredWidth")?.Value, out var desiredWidth);
            DesiredWidth = desiredWidth;

            double.TryParse(element.Attribute("DesiredHeight")?.Value, out var desiredHeight);
            DesiredHeight = desiredHeight;

            double.TryParse(element.Attribute("FloatLeft")?.Value, out var floatLeft);
            FloatLeft = floatLeft;

            double.TryParse(element.Attribute("FloatTop")?.Value, out var floatTop);
            FloatTop = floatTop;

            bool.TryParse(element.Attribute("CanSelect")?.Value, out var canSelect);
            CanSelect = canSelect;
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            Container?.Detach(this);
            _dockControl = null;

            if (_content is IDockSource source) source.DockControl = null;

            _content = null;
            Container = null;
        }

        #endregion

        #region Members

        private void _ToRoot(DockManager dockManager)
        {
            Mode = DockMode.Normal;
            var group = new LayoutGroup(Side, Mode, dockManager);
            group.Attach(this);
            var groupControl = new AnchorSideGroupControl(group) { DesiredHeight = DesiredHeight, DesiredWidth = DesiredWidth };
            switch (Side)
            {
                case DockSide.Left:
                    dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(groupControl, AttachMode.Left, 0);
                    break;
                case DockSide.Right:
                    dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(groupControl, AttachMode.Right, dockManager.LayoutRootPanel.RootGroupPanel.Count);
                    break;
                case DockSide.Top:
                    dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(groupControl, AttachMode.Top, 0);
                    break;
                case DockSide.Bottom:
                    dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(groupControl, AttachMode.Bottom, dockManager.LayoutRootPanel.RootGroupPanel.Count);
                    break;
            }
        }

        #endregion
    }
}