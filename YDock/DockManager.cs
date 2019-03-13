using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using YDock.Enum;
using YDock.Interface;
using YDock.LayoutSetting;
using YDock.Model;
using YDock.View;

namespace YDock
{
    [ContentProperty("Root")]
    public class DockManager : Control,
                               IDockManager
    {
        #region Property definitions

        public static readonly DependencyProperty DockImageSourceProperty =
            DependencyProperty.Register("DockImageSource", typeof(ImageSource), typeof(DockManager));

        public static readonly DependencyProperty DockTitleProperty =
            DependencyProperty.Register("DockTitle",
                                        typeof(string),
                                        typeof(DockManager),
                                        new FrameworkPropertyMetadata("YDock"));

        public static readonly DependencyProperty DocumentHeaderTemplateProperty =
            DependencyProperty.Register("DocumentHeaderTemplate", typeof(ControlTemplate), typeof(DockManager));

        public static readonly DependencyProperty LeftSideProperty =
            DependencyProperty.Register("LeftSide",
                                        typeof(DockBarGroupControl),
                                        typeof(DockManager),
                                        new FrameworkPropertyMetadata(null,
                                                                      OnLeftSideChanged));

        public static readonly DependencyProperty RightSideProperty =
            DependencyProperty.Register("RightSide",
                                        typeof(DockBarGroupControl),
                                        typeof(DockManager),
                                        new FrameworkPropertyMetadata(null,
                                                                      OnRightSideChanged));

        public static readonly DependencyProperty TopSideProperty =
            DependencyProperty.Register("TopSide",
                                        typeof(DockBarGroupControl),
                                        typeof(DockManager),
                                        new FrameworkPropertyMetadata(null,
                                                                      OnTopSideChanged));

        public static readonly DependencyProperty BottomSideProperty =
            DependencyProperty.Register("BottomSide",
                                        typeof(DockBarGroupControl),
                                        typeof(DockManager),
                                        new FrameworkPropertyMetadata(null,
                                                                      OnBottomSideChanged));

        public static readonly DependencyProperty LayoutRootPanelProperty =
            DependencyProperty.Register("LayoutRootPanel",
                                        typeof(LayoutRootPanel),
                                        typeof(DockManager),
                                        new FrameworkPropertyMetadata(null,
                                                                      OnLayoutRootPanelChanged));

        #endregion

        #region Static members

        internal static void ChangeDockMode(IDockView view, DockMode mode)
        {
            if (view is BaseGroupControl)
            {
                (view.Model as BaseLayoutGroup).Mode = mode;
            }

            if (view is LayoutGroupPanel)
            {
                foreach (var _view in (view as LayoutGroupPanel).Children.OfType<IDockView>())
                {
                    ChangeDockMode(_view, mode);
                }
            }
        }

        internal static void ChangeSide(IDockView view, DockSide side)
        {
            if (view.Model != null && view.Model.Side == side) return;
            if (view is BaseGroupControl)
            {
                (view.Model as BaseLayoutGroup).Side = side;
            }

            if (view is LayoutGroupPanel)
            {
                (view as LayoutGroupPanel).Side = side;
            }
        }

        internal static void ClearAttachObj(IDockView view)
        {
            if (view is AnchorSideGroupControl)
            {
                (view.Model as LayoutGroup).AttachObj?.Dispose();
            }

            if (view is LayoutGroupPanel)
            {
                foreach (var _view in (view as LayoutGroupPanel).Children.OfType<IDockView>())
                {
                    ClearAttachObj(_view);
                }
            }
        }

        internal static void FormatChildSize(ILayoutSize child, Size size)
        {
            if (child == null) return;
            child.DesiredWidth = Math.Min(child.DesiredWidth, size.Width / 2);
            child.DesiredHeight = Math.Min(child.DesiredHeight, size.Height / 2);
        }

        private static void OnBottomSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockManager)d).OnBottomSideChanged(e);
        }

        private static void OnLayoutRootPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockManager)d).OnLayoutRootPanelChanged(e);
        }

        private static void OnLeftSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockManager)d).OnLeftSideChanged(e);
        }

        private static void OnRightSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockManager)d).OnRightSideChanged(e);
        }

        private static void OnTopSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockManager)d).OnTopSideChanged(e);
        }

        #endregion

        internal List<Window> _windows = new List<Window>();

        internal Stack<int> backwards;
        internal Stack<int> forwards;
        internal int id;
        private DockElement _activeElement;
        private SortedDictionary<int, IDockControl> _dockControls;

        private List<BaseFloatWindow> _floatWindows;
        private readonly SortedDictionary<string, LayoutSetting.LayoutSetting> _layouts;
        private Window _mainWindow;
        private DockRoot _root;
        private IDockControl _selectedDocument;

        #region Constructors

        static DockManager()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockManager), new FrameworkPropertyMetadata(typeof(DockManager)));
            FocusableProperty.OverrideMetadata(typeof(DockManager), new FrameworkPropertyMetadata(false));
        }

        public DockManager()
        {
            Root = new DockRoot();
            DragManager = new DragManager(this);
            _dockControls = new SortedDictionary<int, IDockControl>();
            _floatWindows = new List<BaseFloatWindow>();
            backwards = new Stack<int>();
            forwards = new Stack<int>();

            _layouts = new SortedDictionary<string, LayoutSetting.LayoutSetting>();
        }

        #endregion

        #region Properties

        public DockBarGroupControl BottomSide
        {
            get { return (DockBarGroupControl)GetValue(BottomSideProperty); }
            set { SetValue(BottomSideProperty, value); }
        }

        public bool CanNavigateBackward
        {
            get { return backwards.Count > 1; }
        }

        public bool CanNavigateForward
        {
            get { return forwards.Count > 0; }
        }

        public ControlTemplate DocumentHeaderTemplate
        {
            internal set { SetValue(DocumentHeaderTemplateProperty, value); }
            get { return (ControlTemplate)GetValue(DocumentHeaderTemplateProperty); }
        }

        public IEnumerable<BaseFloatWindow> FloatWindows
        {
            get { return _floatWindows; }
        }

        public bool IsDragging
        {
            get { return DragManager.IsDragging; }
        }

        public LayoutRootPanel LayoutRootPanel
        {
            get { return (LayoutRootPanel)GetValue(LayoutRootPanelProperty); }
            set { SetValue(LayoutRootPanelProperty, value); }
        }

        public IDictionary<string, LayoutSetting.LayoutSetting> Layouts
        {
            get { return _layouts; }
        }

        public DockBarGroupControl LeftSide
        {
            get { return (DockBarGroupControl)GetValue(LeftSideProperty); }
            set { SetValue(LeftSideProperty, value); }
        }

        public Window MainWindow
        {
            get
            {
                if (_mainWindow == null)
                {
                    _mainWindow = Window.GetWindow(this);
                }

                return _mainWindow;
            }
        }

        public DockBarGroupControl RightSide
        {
            get { return (DockBarGroupControl)GetValue(RightSideProperty); }
            set { SetValue(RightSideProperty, value); }
        }

        public DockBarGroupControl TopSide
        {
            get { return (DockBarGroupControl)GetValue(TopSideProperty); }
            set { SetValue(TopSideProperty, value); }
        }

        /// <summary>
        ///     current ActiveElement
        /// </summary>
        internal IDockElement ActiveElement
        {
            get { return _activeElement; }
            set
            {
                if (_activeElement != value)
                {
                    var oldele = _activeElement;
                    _activeElement = value as DockElement;
                    if (oldele != null)
                    {
                        oldele.IsActive = false;
                    }

                    if (_activeElement != null)
                    {
                        _activeElement.IsActive = true;
                        //这里必须将AutoHideElement设为NULL，保证当前活动窗口只有一个
                        if (AutoHideElement != _activeElement)
                        {
                            AutoHideElement = null;
                        }

                        if (_activeElement.IsDocument)
                        {
                            PushBackwards(_activeElement.ID);
                        }
                    }

                    ActiveDockChanged(this, new EventArgs());

                    var newSelectedDocument = SelectedDocument;
                    if (_selectedDocument != newSelectedDocument)
                    {
                        _selectedDocument = newSelectedDocument;
                        SelectedDocumentChanged(this, new EventArgs());
                    }
                }
            }
        }

        /// <summary>
        ///     自动隐藏窗口的Model
        /// </summary>
        internal IDockElement AutoHideElement
        {
            get { return LayoutRootPanel?.AHWindow.Model; }
            set
            {
                if (LayoutRootPanel.AHWindow.Model != value)
                {
                    if (LayoutRootPanel.AHWindow.Model != null)
                    {
                        LayoutRootPanel.AHWindow.Model.IsVisible = false;
                    }

                    LayoutRootPanel.AHWindow.Model = value as DockElement;
                    if (LayoutRootPanel.AHWindow.Model != null)
                    {
                        LayoutRootPanel.AHWindow.Model.IsVisible = true;
                    }
                }
            }
        }

        internal DragManager DragManager { get; }

        internal DockRoot Root
        {
            get { return _root; }
            set
            {
                if (_root != value)
                {
                    if (_root != null)
                    {
                        _root.Dispose();
                    }

                    _root = value;
                    if (_root != null)
                    {
                        _root.DockManager = this;
                    }
                }
            }
        }

        #endregion

        #region Events

        public event RoutedEventHandler DocumentToEmpty = delegate { };

        public event EventHandler SelectedDocumentChanged = delegate { };

        #endregion

        #region Override members

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (_root.DockManager == this)
            {
                LayoutRootPanel = new LayoutRootPanel(_root);
                LeftSide = new DockBarGroupControl(_root.LeftSide);
                RightSide = new DockBarGroupControl(_root.RightSide);
                BottomSide = new DockBarGroupControl(_root.BottomSide);
                TopSide = new DockBarGroupControl(_root.TopSide);
            }
        }

        #endregion

        #region IDockManager Members

        public int DocumentTabCount
        {
            get { return _root.DocumentModels.Count; }
        }

        /// <summary>
        ///     用于浮动窗口显示，一般用作应用程序的图标
        /// </summary>
        public ImageSource DockImageSource
        {
            set { SetValue(DockImageSourceProperty, value); }
            get { return (ImageSource)GetValue(DockImageSourceProperty); }
        }

        /// <summary>
        ///     用于浮动窗口显示，一般用作应用程序的Title
        /// </summary>
        public string DockTitle
        {
            set { SetValue(DockTitleProperty, value); }
            get { return (string)GetValue(DockTitleProperty); }
        }

        public event EventHandler ActiveDockChanged = delegate { };

        /// <summary>
        ///     当前活动的DockControl
        /// </summary>
        public IDockControl ActiveControl
        {
            get { return _activeElement?.DockControl; }
        }

        /// <summary>
        ///     当前选中的文档
        /// </summary>
        public IDockControl SelectedDocument
        {
            get
            {
                //优先返回活跃的文档
                if (ActiveControl != null && ActiveControl.IsDocument) return ActiveControl;
                if (_root == null) return null;
                var element = (_root?.DocumentModels[0].View as TabControl).SelectedItem as DockElement;
                return element?.DockControl;
            }
        }

        public IDockModel Model
        {
            get { return null; }
        }

        public IDockView DockViewParent
        {
            get { return this; }
        }

        /// <summary>
        ///     all registed DockControl
        /// </summary>
        public IEnumerable<IDockControl> DockControls
        {
            get
            {
                foreach (var ctrl in _dockControls)
                {
                    yield return ctrl.Value;
                }
            }
        }

        /// <summary>
        ///     以选项卡模式向DockManager注册一个DockElement
        /// </summary>
        /// <param name="title">标题栏文字</param>
        /// <param name="content">内容</param>
        /// <param name="imageSource">标题栏图标</param>
        /// <param name="canSelect">是否直接停靠在选项栏中供用户选择(默认为False)</param>
        /// <param name="desiredWidth">期望的宽度</param>
        /// <param name="desiredHeight">期望的高度</param>
        /// <returns></returns>
        public void RegisterDocument(IDockSource content,
                                     bool canSelect = false,
                                     double desiredWidth = Constants.DockDefaultWidthLength,
                                     double desiredHeight = Constants.DockDefaultHeightLength,
                                     double floatLeft = 0.0,
                                     double floatTop = 0.0)
        {
            var element = new DockElement(true)
            {
                ID = id++,
                Title = content.Header,
                Content = content,
                ImageSource = content.Icon,
                Side = DockSide.None,
                Mode = DockMode.Normal,
                CanSelect = canSelect,
                DesiredWidth = desiredWidth,
                DesiredHeight = desiredHeight,
                FloatLeft = floatLeft,
                FloatTop = floatTop
            };
            var ctrl = new DockControl(element);
            AddDockControl(ctrl);
            _root.DocumentModels[0].Attach(element);
            content.DockControl = ctrl;
        }

        /// <summary>
        ///     以DockBar模式（必须指定停靠方向，否则默认停靠在左侧）向DockManager注册一个DockElement
        /// </summary>
        /// <param name="title">标题栏文字</param>
        /// <param name="content">内容</param>
        /// <param name="imageSource">标题栏图标</param>
        /// <param name="side">停靠方向（默认左侧）</param>
        /// <param name="canSelect">是否直接停靠在选项栏中供用户选择(默认为False)</param>
        /// <param name="desiredWidth">期望的宽度</param>
        /// <param name="desiredHeight">期望的高度</param>
        /// <returns></returns>
        public void RegisterDock(IDockSource content,
                                 DockSide side = DockSide.Left,
                                 bool canSelect = false,
                                 double desiredWidth = Constants.DockDefaultWidthLength,
                                 double desiredHeight = Constants.DockDefaultHeightLength,
                                 double floatLeft = 0.0,
                                 double floatTop = 0.0)
        {
            var element = new DockElement
            {
                ID = id++,
                Title = content.Header,
                Content = content,
                ImageSource = content.Icon,
                Side = side,
                Mode = DockMode.DockBar,
                CanSelect = canSelect,
                DesiredWidth = desiredWidth,
                DesiredHeight = desiredHeight,
                FloatLeft = floatLeft,
                FloatTop = floatTop
            };
            switch (side)
            {
                case DockSide.Left:
                case DockSide.Right:
                case DockSide.Top:
                case DockSide.Bottom:
                    _root.AddSideChild(element, side);
                    break;
                default: //其他非法方向返回NULL
                    element.Dispose();
                    break;
            }

            var ctrl = new DockControl(element);
            AddDockControl(ctrl);
            content.DockControl = ctrl;
        }

        /// <summary>
        ///     以Float模式向DockManager注册一个DockElement
        /// </summary>
        /// <param name="title">标题栏文字</param>
        /// <param name="content">内容</param>
        /// <param name="imageSource">标题栏图标</param>
        /// <param name="side">停靠方向（默认左侧）</param>
        /// <param name="desiredWidth">期望的宽度</param>
        /// <param name="desiredHeight">期望的高度</param>
        /// <returns></returns>
        public void RegisterFloat(IDockSource content,
                                  DockSide side = DockSide.Left,
                                  double desiredWidth = Constants.DockDefaultWidthLength,
                                  double desiredHeight = Constants.DockDefaultHeightLength,
                                  double floatLeft = 0.0,
                                  double floatTop = 0.0)
        {
            var element = new DockElement
            {
                ID = id++,
                Title = content.Header,
                Content = content,
                ImageSource = content.Icon,
                Side = side,
                Mode = DockMode.Float,
                DesiredWidth = desiredWidth,
                DesiredHeight = desiredHeight,
                FloatLeft = floatLeft,
                FloatTop = floatTop
            };
            var ctrl = new DockControl(element);
            var group = new LayoutGroup(side, element.Mode, this);
            group.Attach(element);
            AddDockControl(ctrl);
            content.DockControl = ctrl;
        }

        public void HideAll()
        {
            foreach (var dockControl in _dockControls.Values)
            {
                dockControl.Hide();
            }
        }

        public void UpdateTitleAll()
        {
            foreach (var dockControl in _dockControls.Values)
            {
                if (dockControl.Content is IDockSource source)
                {
                    dockControl.Title = source.Header;
                }
            }
        }

        public int GetDocumentTabIndex(IDockControl dockControl)
        {
            var index = -1;
            foreach (var model in _root.DocumentModels)
            {
                index++;
                foreach (var item in model.Children)
                {
                    if (item == dockControl.ProtoType)
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        ///     attach source to target by <see cref="AttachMode" />
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="target">目标</param>
        /// <param name="mode">附加模式</param>
        public void AttachTo(IDockControl source, IDockControl target, AttachMode mode, double ratio = -1)
        {
            if (target.Container.View == null) throw new InvalidOperationException("target must be visible!");
            if (target.IsDisposed) throw new InvalidOperationException("target is disposed!");
            if (source == target) throw new InvalidOperationException("source can not be target!");
            if (source == null || target == null) throw new ArgumentNullException("source or target is null!");
            if (target.Mode == DockMode.DockBar) throw new ArgumentNullException("target is DockBar Mode!");
            if (source.Container != null)
            {
                //DockBar模式下无法合并，故先转换为Normal模式
                //if (target.Mode == DockMode.DockBar)
                //    target.ToDock();

                source.Container.Detach(source.ProtoType);

                double width = (target.Container.View as ILayoutViewWithSize).DesiredWidth, height = (target.Container.View as ILayoutViewWithSize).DesiredHeight;

                if (ratio > 0)
                {
                    if (mode == AttachMode.Right
                        || mode == AttachMode.Left
                        || mode == AttachMode.Left_WithSplit
                        || mode == AttachMode.Right_WithSplit)
                    {
                        width = (target.Container.View as ILayoutViewWithSize).DesiredWidth * ratio;
                    }

                    if (mode == AttachMode.Top
                        || mode == AttachMode.Bottom
                        || mode == AttachMode.Top_WithSplit
                        || mode == AttachMode.Bottom_WithSplit)
                    {
                        height = (target.Container.View as ILayoutViewWithSize).DesiredHeight * ratio;
                    }
                }

                BaseLayoutGroup group;
                BaseGroupControl ctrl;
                if (source.IsDocument)
                {
                    group = new LayoutDocumentGroup(DockMode.Normal, this);
                    ctrl = new LayoutDocumentGroupControl(group, ratio > 0 ? width : source.DesiredWidth, ratio > 0 ? height : source.DesiredHeight);
                }
                else
                {
                    group = new LayoutGroup(source.Side, DockMode.Normal, this);
                    ctrl = new AnchorSideGroupControl(group, ratio > 0 ? width : source.DesiredWidth, ratio > 0 ? height : source.DesiredHeight);
                }

                group.Attach(source.ProtoType);
                var _atsource = target.ProtoType.Container.View as IAttach;
                _atsource.AttachWith(ctrl, mode);
                source.SetActive();
            }
            else
            {
                throw new ArgumentNullException("the container of source is null!");
            }
        }

        /// <summary>
        ///     If name has exist, it will override the current layout,otherwise create a new layout.
        /// </summary>
        /// <param name="name">layout name</param>
        public void SaveCurrentLayout(string name)
        {
            if (_layouts.ContainsKey(name))
            {
                _layouts[name].Layout = _GenerateCurrentLayout();
            }
            else
            {
                _layouts[name] = new LayoutSetting.LayoutSetting(name, _GenerateCurrentLayout());
            }
        }

        public bool ApplyLayout(string name)
        {
            if (_layouts.ContainsKey(name))
            {
                _ApplyLayout(_layouts[name]);
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            foreach (var ctrl in new List<IDockControl>(_dockControls.Values))
            {
                ctrl.Dispose();
            }

            _dockControls.Clear();
            _dockControls = null;
            foreach (var wnd in new List<BaseFloatWindow>(_floatWindows))
            {
                wnd.Close();
            }

            _floatWindows.Clear();
            _floatWindows = null;
            _mainWindow = null;
            Root = null;
            _windows.Clear();
            _windows = null;
            backwards.Clear();
            backwards = null;
            forwards.Clear();
            forwards = null;
        }

        #endregion

        #region Members

        /// <summary>
        ///     向后导航
        /// </summary>
        public void NavigateBackward()
        {
            while (CanNavigateBackward)
            {
                forwards.Push(backwards.Pop());
                var id = backwards.Peek();
                if (_dockControls.ContainsKey(id))
                {
                    _dockControls[id].ToDockAsDocument();
                }
            }
        }

        /// <summary>
        ///     向前导航
        /// </summary>
        public void NavigateForward()
        {
            while (CanNavigateForward)
            {
                var id = forwards.Pop();
                if (_dockControls.ContainsKey(id))
                {
                    backwards.Push(id);
                    _dockControls[id].ToDockAsDocument();
                }
            }
        }

        public void ShowByID(int id)
        {
            if (_dockControls.ContainsKey(id))
            {
                _dockControls[id].ToDockAsDocument();
            }
        }

        public void ShowOrHide(IDockSource source)
        {
            if (source.DockControl.IsVisible)
            {
                source.DockControl.Hide();
            }
            else
            {
                source.DockControl.Show();
            }
        }

        protected virtual void OnBottomSideChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                RemoveLogicalChild(e.OldValue);
            }

            if (e.NewValue != null)
            {
                AddLogicalChild(e.NewValue);
            }
        }

        protected virtual void OnLayoutRootPanelChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                RemoveLogicalChild(e.OldValue);
            }

            if (e.NewValue != null)
            {
                AddLogicalChild(e.NewValue);
            }
        }

        protected virtual void OnLeftSideChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                RemoveLogicalChild(e.OldValue);
            }

            if (e.NewValue != null)
            {
                AddLogicalChild(e.NewValue);
            }
        }

        protected virtual void OnRightSideChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                RemoveLogicalChild(e.OldValue);
            }

            if (e.NewValue != null)
            {
                AddLogicalChild(e.NewValue);
            }
        }

        protected virtual void OnTopSideChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                RemoveLogicalChild(e.OldValue);
            }

            if (e.NewValue != null)
            {
                AddLogicalChild(e.NewValue);
            }
        }

        internal void AddDockControl(IDockControl ctrl)
        {
            if (!_dockControls.ContainsKey(ctrl.ID))
            {
                _dockControls.Add(ctrl.ID, ctrl);
            }
        }

        internal void AddFloatWindow(BaseFloatWindow window)
        {
            _floatWindows.Add(window);
        }

        internal int FindVisibleCtrl()
        {
            foreach (var id in backwards)
            {
                if (_dockControls.ContainsKey(id)
                    && !_dockControls[id].IsActive
                    && _dockControls[id].CanSelect)
                {
                    return id;
                }
            }

            return -1;
        }

        internal IDockControl GetDockControl(int id)
        {
            if (_dockControls.ContainsKey(id))
            {
                return _dockControls[id];
            }

            return null;
        }

        internal bool IsBehindToMainWindow(BaseFloatWindow wnd)
        {
            if (wnd is AnchorGroupWindow)
            {
                return false;
            }

            var index1 = _windows.IndexOf(_mainWindow);
            var index2 = _windows.IndexOf(wnd);
            return index2 > index1;
        }

        internal void MoveFloatTo(BaseFloatWindow wnd, int index = 0)
        {
            _floatWindows.Remove(wnd);
            _floatWindows.Insert(index, wnd);
        }

        internal void PushBackwards(int id)
        {
            if (backwards.Count > 0 && backwards.Peek() == id) return;
            if (id < 0) return;
            backwards.Push(id);
            forwards.Clear();
        }

        internal void RaiseDocumentToEmpty()
        {
            DocumentToEmpty(this, new RoutedEventArgs());
        }

        internal void RemoveDockControl(IDockControl ctrl)
        {
            if (_dockControls.ContainsKey(ctrl.ID))
            {
                _dockControls.Remove(ctrl.ID);
            }
        }

        internal void RemoveFloatWindow(BaseFloatWindow window)
        {
            if (_floatWindows.Contains(window))
            {
                _floatWindows.Remove(window);
            }
        }

        internal void UpdateWindowZOrder()
        {
            _windows.Clear();
            var unsorts = new List<Window>();
            foreach (Window wnd in Application.Current.Windows)
            {
                if (wnd is BaseFloatWindow)
                {
                    unsorts.Add(wnd);
                }
            }

            unsorts.Add(MainWindow);
            _windows.AddRange(SortWindowsTopToBottom(unsorts));
        }

        private void _ApplyLayout(LayoutSetting.LayoutSetting layout)
        {
            HideAll();

            int id;
            var rootNode = layout.Layout;
            foreach (var item in rootNode.Element("Elements").Elements())
            {
                id = int.Parse(item.Attribute("ID").Value);
                if (_dockControls.ContainsKey(id))
                {
                    _dockControls[id].ProtoType.Load(item);
                }
            }

            _root.LoadLayout(rootNode.Element("ToolBar"));

            _LoadRootPanel(rootNode.Element("Panel"));

            _LoadFloatWindows(rootNode.Element("FloatWindows"));

            var node = rootNode.Element("ActiveItem");
            if (node != null)
            {
                id = int.Parse(node.Value);
                if (_dockControls.ContainsKey(id))
                {
                    _dockControls[id].SetActive();
                }
            }
        }

        private XElement _GenerateCurrentLayout()
        {
            var rootNode = new XElement("Layout");

            if (_activeElement != null)
            {
                rootNode.Add(new XElement("ActiveItem", _activeElement.ID));
            }

            // FloatWindows SaveSize
            _floatWindows.ForEach(fw => fw.SaveSize());

            // Elements
            var node = new XElement("Elements");
            foreach (var item in _dockControls.Values.Select(dc => dc.ProtoType))
            {
                node.Add(item.Save());
            }

            rootNode.Add(node);

            // Tool bar
            rootNode.Add(_root.GenerateLayout());

            // Layout Root
            rootNode.Add(LayoutRootPanel.RootGroupPanel.GenerateLayout());

            // FloatWindows
            node = new XElement("FloatWindows");
            foreach (var fw in _floatWindows)
            {
                node.Add(fw.GenerateLayout());
            }

            rootNode.Add(node);

            return rootNode;
        }

        private void _LoadFloatWindows(XElement element)
        {
            foreach (var item in element.Elements())
            {
                var node = item.Element("Panel");
                if (node != null)
                {
                    var panelNode = new PanelNode(null);
                    panelNode.Load(node);
                    panelNode.ApplyLayout(this, true);
                    panelNode.Dispose();
                }
                else
                {
                    node = item.Element("Group");
                    var groupNode = new GroupNode(null);
                    groupNode.Load(node);
                    groupNode.ApplyLayout(this, true);
                    groupNode.Dispose();
                }
            }
        }

        private void _LoadRootPanel(XElement element)
        {
            var rootNode = new PanelNode(null);
            rootNode.Load(element);
            rootNode.ApplyLayout(this);
            rootNode.Dispose();
        }

        private IEnumerable<Window> SortWindowsTopToBottom(IEnumerable<Window> unsorted)
        {
            var byHandle = unsorted.ToDictionary(win =>
                                                     new WindowInteropHelper(win).Handle);

            for (var hWnd = Win32Helper.GetTopWindow(IntPtr.Zero); hWnd != IntPtr.Zero; hWnd = Win32Helper.GetWindow(hWnd, Win32Helper.GW_HWNDNEXT))
            {
                if (byHandle.ContainsKey(hWnd))
                {
                    yield return byHandle[hWnd];
                }
            }
        }

        #endregion
    }
}