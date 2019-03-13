using System;
using System.ComponentModel;
using System.Linq;
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
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(DockElement));

        public static readonly DependencyProperty SideProperty =
            DependencyProperty.Register("Side", typeof(DockSide), typeof(DockElement));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title",
                                        typeof(string),
                                        typeof(DockElement),
                                        new FrameworkPropertyMetadata(string.Empty));

        #endregion

        private bool _canSelect;

        private ILayoutGroup _container;
        private object _content;

        private DockControl _dockControl;

        private double _floatLeft;

        private double _floatTop;
        private int _id;
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
                if (_dockControl != value)
                {
                    _dockControl = value;
                }
            }
        }

        public string ToolTip
        {
            get
            {
                if (Content is IDockDocSource)
                {
                    return (Content as IDockDocSource).FullFileName;
                }

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

        public int ID
        {
            get { return _id; }
            internal set
            {
                if (_id != value)
                {
                    _id = value;
                }
            }
        }

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
        ///     Content是否可见
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
        ///     是否以Document模式注册，该属性将影响Dock的浮动窗口的模式
        /// </summary>
        public bool IsDocument { get; }

        /// <summary>
        ///     是否为当前的活动窗口
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
        ///     是否显示在用户界面供用户点击显示，默认为false
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
                        DockManager.PushBackwards(_id);
                    }
                }
            }
            get { return _canSelect; }
        }

        public bool IsDocked
        {
            get { return CanSelect && _container is LayoutGroup && Mode == DockMode.Normal; }
        }

        public bool IsFloat
        {
            get { return CanSelect && _container is LayoutGroup && Mode == DockMode.Float; }
        }

        public bool IsAutoHide
        {
            get { return _container != null && this == DockManager.AutoHideElement; }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public ILayoutGroup Container
        {
            get { return _container; }
            internal set
            {
                if (_container != value)
                {
                    _container = value;
                }
            }
        }

        public DockManager DockManager
        {
            get { return _container?.DockManager; }
        }

        public double DesiredWidth { get; set; }

        public double DesiredHeight { get; set; }

        public double FloatLeft
        {
            get { return _floatLeft; }
            set
            {
                if (_floatLeft != value)
                {
                    _floatLeft = value;
                }
            }
        }

        public double FloatTop
        {
            get { return _floatTop; }
            set
            {
                if (_floatTop != value)
                {
                    _floatTop = value;
                }
            }
        }

        public bool CanFloat
        {
            get
            {
                if (_container == null) return false;
                return Mode != DockMode.Float ||
                       _container?.View == null ||
                       _container.Children.Count() > 1 ||
                       _container.View.DockViewParent != null ||
                       !_canSelect;
            }
        }

        public bool CanDock
        {
            get { return _container != null && Mode != DockMode.Normal; }
        }

        public bool CanDockAsDocument
        {
            get { return true; }
        }

        public bool CanSwitchAutoHideStatus
        {
            get { return _container != null && Mode != DockMode.Float; }
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

            if (_container != null)
            {
                CanSelect = true;
                //注意切换模式
                Mode = DockMode.Float;
                var dockManager = DockManager;
                _container.Detach(this);
                _container = null;
                BaseFloatWindow wnd;
                BaseGroupControl groupctrl;
                if (!IsDocument)
                {
                    var group = new LayoutGroup(Side, Mode, dockManager);
                    group.Attach(this);
                    groupctrl = new AnchorSideGroupControl(group) { DesiredHeight = DesiredHeight, DesiredWidth = DesiredWidth };
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
                    groupctrl = new LayoutDocumentGroupControl(group) { DesiredHeight = DesiredHeight, DesiredWidth = DesiredWidth };
                    wnd = new DocumentGroupWindow(dockManager)
                    {
                        Height = DesiredHeight,
                        Width = DesiredWidth,
                        Left = FloatLeft,
                        Top = FloatTop
                    };
                }

                wnd.AttachChild(groupctrl, AttachMode.None, 0);
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

            if (_container != null)
            {
                CanSelect = true;
                Mode = DockMode.Normal;
                var dockManager = DockManager;
                var group = _container as LayoutGroup;
                if (group == null || group.AttachObj == null || !group.AttachObj.AttachTo())
                {
                    //默认向下停靠
                    if (Side == DockSide.None)
                    {
                        Side = DockSide.Bottom;
                    }

                    _container?.Detach(this);
                    _container = null;
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

            if (_container != null)
            {
                CanSelect = true;
                var dockManager = DockManager;
                _container.Detach(this);
                _container = null;
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
            if (_container != null)
            {
                var dockManager = DockManager;
                _container.Detach(this);
                _container = null;

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
            if (_container != null)
            {
                if (side != Side || Mode != DockMode.DockBar)
                {
                    var dockManager = DockManager;
                    _container.Detach(this);
                    _container = null;

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
                var id = DockManager.FindVisibleCtrl();
                DockManager.PushBackwards(id);
                DockManager.ShowByID(id);
            }

            if (DockManager.AutoHideElement == this)
            {
                DockManager.AutoHideElement = null;
            }

            _dockControl.SetActive(false);
            CanSelect = false;
        }

        public XElement Save()
        {
            var element = new XElement("Item");
            element.SetAttributeValue("ID", _id);
            element.SetAttributeValue("DesiredWidth", DesiredWidth);
            element.SetAttributeValue("DesiredHeight", DesiredHeight);
            element.SetAttributeValue("FloatLeft", _floatLeft);
            element.SetAttributeValue("FloatTop", _floatTop);
            element.SetAttributeValue("CanSelect", _canSelect);
            return element;
        }

        public void Load(XElement element)
        {
            DesiredWidth = double.Parse(element.Attribute("DesiredWidth").Value);
            DesiredHeight = double.Parse(element.Attribute("DesiredHeight").Value);
            _floatLeft = double.Parse(element.Attribute("FloatLeft").Value);
            _floatTop = double.Parse(element.Attribute("FloatTop").Value);
            CanSelect = bool.Parse(element.Attribute("CanSelect").Value);
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            _container?.Detach(this);
            _dockControl = null;
            if (_content is IDockSource)
            {
                (_content as IDockSource).DockControl = null;
            }

            _content = null;
            _container = null;
        }

        #endregion

        #region Members

        private void _ToRoot(DockManager dockManager)
        {
            Mode = DockMode.Normal;
            var group = new LayoutGroup(Side, Mode, dockManager);
            group.Attach(this);
            var groupctrl = new AnchorSideGroupControl(group) { DesiredHeight = DesiredHeight, DesiredWidth = DesiredWidth };
            switch (Side)
            {
                case DockSide.Left:
                    dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(groupctrl, AttachMode.Left, 0);
                    break;
                case DockSide.Right:
                    dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(groupctrl, AttachMode.Right, dockManager.LayoutRootPanel.RootGroupPanel.Count);
                    break;
                case DockSide.Top:
                    dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(groupctrl, AttachMode.Top, 0);
                    break;
                case DockSide.Bottom:
                    dockManager.LayoutRootPanel.RootGroupPanel.AttachChild(groupctrl, AttachMode.Bottom, dockManager.LayoutRootPanel.RootGroupPanel.Count);
                    break;
            }
        }

        #endregion
    }
}