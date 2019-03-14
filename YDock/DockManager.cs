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
using YDock.Model.Element;
using YDock.Model.Layout;
using YDock.View.Control;
using YDock.View.Layout;
using YDock.View.Window;

namespace YDock
{
    [ContentProperty("Root")]
    public class DockManager : Control,
                               IDockView
    {
        #region Property definitions

        public static readonly DependencyProperty DockImageSourceProperty =
            DependencyProperty.Register("DockImageSource",
                                        typeof(ImageSource),
                                        typeof(DockManager));

        public static readonly DependencyProperty DockTitleProperty =
            DependencyProperty.Register("DockTitle",
                                        typeof(string),
                                        typeof(DockManager),
                                        new FrameworkPropertyMetadata("YDock"));

        public static readonly DependencyProperty DocumentHeaderTemplateProperty =
            DependencyProperty.Register("DocumentHeaderTemplate",
                                        typeof(ControlTemplate),
                                        typeof(DockManager));

        public static readonly DependencyProperty LeftSideProperty =
            DependencyProperty.Register("LeftSide",
                                        typeof(DockBarGroupControl),
                                        typeof(DockManager),
                                        new FrameworkPropertyMetadata(null, OnLeftSideChanged));

        public static readonly DependencyProperty RightSideProperty =
            DependencyProperty.Register("RightSide",
                                        typeof(DockBarGroupControl),
                                        typeof(DockManager),
                                        new FrameworkPropertyMetadata(null, OnRightSideChanged));

        public static readonly DependencyProperty TopSideProperty =
            DependencyProperty.Register("TopSide",
                                        typeof(DockBarGroupControl),
                                        typeof(DockManager),
                                        new FrameworkPropertyMetadata(null, OnTopSideChanged));

        public static readonly DependencyProperty BottomSideProperty =
            DependencyProperty.Register("BottomSide",
                                        typeof(DockBarGroupControl),
                                        typeof(DockManager),
                                        new FrameworkPropertyMetadata(null, OnBottomSideChanged));

        public static readonly DependencyProperty LayoutRootPanelProperty =
            DependencyProperty.Register("LayoutRootPanel",
                                        typeof(LayoutRootPanel),
                                        typeof(DockManager),
                                        new FrameworkPropertyMetadata(null, OnLayoutRootPanelChanged));

        #endregion

        #region Static members

        internal static void ChangeDockMode(IDockView view, DockMode mode)
        {
            if (view is BaseGroupControl)
            {
                ((BaseLayoutGroup)view.Model).Mode = mode;
            }

            if (view is LayoutGroupPanel panel)
            {
                foreach (var childView in panel.Children.OfType<IDockView>())
                {
                    ChangeDockMode(childView, mode);
                }
            }
        }

        internal static void ChangeSide(IDockView view, DockSide side)
        {
            if (view.Model != null && view.Model.Side == side) return;
            if (view is BaseGroupControl control && control.Model is BaseLayoutGroup layoutGroup)
            {
                layoutGroup.Side = side;
            }

            if (view is LayoutGroupPanel panel)
            {
                panel.Side = side;
            }
        }

        internal static void ClearAttachObject(IDockView view)
        {
            if (view is AnchorSideGroupControl control && control.Model is LayoutGroup layoutGroup)
            {
                layoutGroup.AttachObject?.Dispose();
            }

            if (view is LayoutGroupPanel panel)
            {
                foreach (var dockView in panel.Children.OfType<IDockView>())
                {
                    ClearAttachObject(dockView);
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

        private readonly SortedDictionary<string, LayoutSetting.LayoutSetting> _layouts;

        internal Stack<int> _backwards;
        internal Stack<int> _forwards;
        internal int _id;

        internal List<Window> _windows;
        private DockElement _activeElement;
        private SortedDictionary<int, IDockControl> _dockControls;

        private List<BaseFloatWindow> _floatWindows;
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
            _dockControls = new SortedDictionary<int, IDockControl>();
            _windows = new List<Window>();
            _floatWindows = new List<BaseFloatWindow>();

            Root = new DockRoot();
            DragManager = new DragManager(this);
            _backwards = new Stack<int>();
            _forwards = new Stack<int>();

            _layouts = new SortedDictionary<string, LayoutSetting.LayoutSetting>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Currently active DockControl
        /// </summary>
        public IDockControl ActiveControl
        {
            get { return _activeElement?.DockControl; }
        }

        public DockBarGroupControl BottomSide
        {
            get { return (DockBarGroupControl)GetValue(BottomSideProperty); }
            set { SetValue(BottomSideProperty, value); }
        }

        public bool CanNavigateBackward
        {
            get { return _backwards.Count > 1; }
        }

        public bool CanNavigateForward
        {
            get { return _forwards.Count > 0; }
        }

        /// <summary>
        ///     All registered DockControl
        /// </summary>
        public IEnumerable<IDockControl> DockControls
        {
            get
            {
                foreach (var control in _dockControls)
                {
                    yield return control.Value;
                }
            }
        }

        /// <summary>
        ///     Used for floating window display, generally used as an icon for an application
        /// </summary>
        public ImageSource DockImageSource
        {
            set { SetValue(DockImageSourceProperty, value); }
            get { return (ImageSource)GetValue(DockImageSourceProperty); }
        }

        /// <summary>
        ///     Used for floating window display, generally used as the title of the application
        /// </summary>
        public string DockTitle
        {
            set { SetValue(DockTitleProperty, value); }
            get { return (string)GetValue(DockTitleProperty); }
        }

        public ControlTemplate DocumentHeaderTemplate
        {
            internal set { SetValue(DocumentHeaderTemplateProperty, value); }
            get { return (ControlTemplate)GetValue(DocumentHeaderTemplateProperty); }
        }

        public int DocumentTabCount
        {
            get { return _root.DocumentModels.Count; }
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
            get { return _mainWindow ?? (_mainWindow = Window.GetWindow(this)); }
        }

        public DockBarGroupControl RightSide
        {
            get { return (DockBarGroupControl)GetValue(RightSideProperty); }
            set { SetValue(RightSideProperty, value); }
        }

        /// <summary>
        ///     Currently selected document
        /// </summary>
        public IDockControl SelectedDocument
        {
            get
            {
                //Return active documents first
                if (ActiveControl != null && ActiveControl.IsDocument) return ActiveControl;
                if (_root == null) return null;
                var element = ((TabControl)_root.DocumentModels[0].View).SelectedItem as DockElement;
                return element?.DockControl;
            }
        }

        public DockBarGroupControl TopSide
        {
            get { return (DockBarGroupControl)GetValue(TopSideProperty); }
            set { SetValue(TopSideProperty, value); }
        }

        /// <summary>
        ///     Current ActiveElement
        /// </summary>
        internal IDockElement ActiveElement
        {
            get { return _activeElement; }
            set
            {
                if (_activeElement != value)
                {
                    var oldElement = _activeElement;
                    _activeElement = value as DockElement;
                    if (oldElement != null)
                    {
                        oldElement.IsActive = false;
                    }

                    if (_activeElement != null)
                    {
                        _activeElement.IsActive = true;
                        //Here you must set AutoHideElement to NULL to ensure that there is only one active window.
                        if (!Equals(AutoHideElement, _activeElement))
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
        ///     Automatically hide the model of the window
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
                if (_root == value) return;

                _root?.Dispose();
                _root = value;

                if (_root != null) _root.DockManager = this;
            }
        }

        #endregion

        #region Events

        public event EventHandler ActiveDockChanged = delegate { };

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

        #region IDockView Members

        public IDockModel Model
        {
            get { return null; }
        }

        public IDockView DockViewParent
        {
            get { return this; }
        }

        public void Dispose()
        {
            foreach (var control in new List<IDockControl>(_dockControls.Values))
            {
                control.Dispose();
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
            _backwards.Clear();
            _backwards = null;
            _forwards.Clear();
            _forwards = null;
        }

        #endregion

        #region Members

        public bool ApplyLayout(string name)
        {
            if (_layouts.ContainsKey(name))
            {
                ApplyLayout(_layouts[name]);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Attach source to target by <see cref="AttachMode" />
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="target">target</param>
        /// <param name="mode">Additional mode</param>
        /// <param name="ratio">Ratio</param>
        public void AttachTo(IDockControl source, IDockControl target, AttachMode mode, double ratio = -1)
        {
            if (target.Container.View == null) throw new InvalidOperationException("target must be visible!");
            if (target.IsDisposed) throw new InvalidOperationException("target is disposed!");
            if (source == target) throw new InvalidOperationException("source can not be target!");
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (target.Mode == DockMode.DockBar) throw new ArgumentException("target is DockBar Mode!");
            if (source.Container == null) throw new ArgumentException("source");
            //Cannot be merged in DockBar mode, so it is converted to Normal mode first.
            //if (target.Mode == DockMode.DockBar)
            //    target.ToDock();

            source.Container.Detach(source.Prototype);

            var layoutViewWithSize = (ILayoutViewWithSize)target.Container.View;
            double width = layoutViewWithSize.DesiredWidth, height = layoutViewWithSize.DesiredHeight;

            if (ratio > 0)
            {
                if (mode == AttachMode.Right
                    || mode == AttachMode.Left
                    || mode == AttachMode.Left_WithSplit
                    || mode == AttachMode.Right_WithSplit)
                {
                    width = layoutViewWithSize.DesiredWidth * ratio;
                }

                if (mode == AttachMode.Top
                    || mode == AttachMode.Bottom
                    || mode == AttachMode.Top_WithSplit
                    || mode == AttachMode.Bottom_WithSplit)
                {
                    height = layoutViewWithSize.DesiredHeight * ratio;
                }
            }

            BaseLayoutGroup group;
            BaseGroupControl control;
            if (source.IsDocument)
            {
                group = new LayoutDocumentGroup(DockMode.Normal, this);
                control = new LayoutDocumentGroupControl(group, ratio > 0 ? width : source.DesiredWidth, ratio > 0 ? height : source.DesiredHeight);
            }
            else
            {
                group = new LayoutGroup(source.Side, DockMode.Normal, this);
                control = new AnchorSideGroupControl(group, ratio > 0 ? width : source.DesiredWidth, ratio > 0 ? height : source.DesiredHeight);
            }

            group.Attach(source.Prototype);
            var attach = (IAttach)target.Prototype.Container.View;
            attach.AttachWith(control, mode);
            source.SetActive();
        }

        public int GetDocumentTabIndex(IDockControl dockControl)
        {
            var index = -1;
            foreach (var model in _root.DocumentModels)
            {
                index++;
                foreach (var item in model.Children)
                {
                    if (item == dockControl.Prototype)
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        public void HideAll()
        {
            foreach (var dockControl in _dockControls.Values)
            {
                dockControl.Hide();
            }
        }

        /// <summary>
        ///     Navigate backwards
        /// </summary>
        public void NavigateBackward()
        {
            while (CanNavigateBackward)
            {
                _forwards.Push(_backwards.Pop());
                var id = _backwards.Peek();
                if (_dockControls.ContainsKey(id))
                {
                    _dockControls[id].ToDockAsDocument();
                }
            }
        }

        /// <summary>
        ///     Navigate forward
        /// </summary>
        public void NavigateForward()
        {
            while (CanNavigateForward)
            {
                var id = _forwards.Pop();
                if (_dockControls.ContainsKey(id))
                {
                    _backwards.Push(id);
                    _dockControls[id].ToDockAsDocument();
                }
            }
        }

        /// <summary>
        ///     Register a DockElement with the DockManager in DockBar mode (you must specify the docking direction, otherwise the
        ///     default docked to the left)
        /// </summary>
        /// <param name="content">content</param>
        /// <param name="side">Docking direction (default left)</param>
        /// <param name="canSelect">Whether to dock directly in the options bar for user selection (default is False)</param>
        /// <param name="desiredWidth">Desired width</param>
        /// <param name="desiredHeight">Desired height</param>
        /// <param name="floatLeft"></param>
        /// <param name="floatTop"></param>
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
                ID = _id++,
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
                default: //Other illegal directions return NULL
                    element.Dispose();
                    break;
            }

            var control = new DockControl(element);
            AddDockControl(control);
            content.DockControl = control;
        }

        /// <summary>
        ///     Register a DockElement with the DockManager in tab mode
        /// </summary>
        /// <param name="content">content</param>
        /// <param name="canSelect">Whether to dock directly in the options bar for user selection (default is False)</param>
        /// <param name="desiredWidth">Desired width</param>
        /// <param name="desiredHeight">Desired height</param>
        /// <param name="floatLeft"></param>
        /// <param name="floatTop"></param>
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
                ID = _id++,
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
            var control = new DockControl(element);
            AddDockControl(control);
            _root.DocumentModels[0].Attach(element);
            content.DockControl = control;
        }

        /// <summary>
        ///     Register a DockElement with the DockManager in Float mode
        /// </summary>
        /// <param name="content">content</param>
        /// <param name="side">Docking direction (default left)</param>
        /// <param name="desiredWidth">Desired width</param>
        /// <param name="desiredHeight">Desired height</param>
        /// <param name="floatLeft"></param>
        /// <param name="floatTop"></param>
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
                ID = _id++,
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
            var control = new DockControl(element);
            var group = new LayoutGroup(side, element.Mode, this);
            group.Attach(element);
            AddDockControl(control);
            content.DockControl = control;
        }

        /// <summary>
        ///     If name has exist, it will override the current layout,otherwise create a new layout.
        /// </summary>
        /// <param name="name">layout name</param>
        public void SaveCurrentLayout(string name)
        {
            if (_layouts.ContainsKey(name))
            {
                _layouts[name].Layout = GenerateCurrentLayout();
            }
            else
            {
                _layouts[name] = new LayoutSetting.LayoutSetting(name, GenerateCurrentLayout());
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

        internal void AddDockControl(IDockControl control)
        {
            if (!_dockControls.ContainsKey(control.ID))
            {
                _dockControls.Add(control.ID, control);
            }
        }

        internal void AddFloatWindow(BaseFloatWindow window)
        {
            _floatWindows.Add(window);
        }

        internal int FindVisibleControl()
        {
            foreach (var id in _backwards)
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
            if (_backwards.Count > 0 && _backwards.Peek() == id) return;
            if (id < 0) return;
            _backwards.Push(id);
            _forwards.Clear();
        }

        internal void RaiseDocumentToEmpty()
        {
            DocumentToEmpty(this, new RoutedEventArgs());
        }

        internal void RemoveDockControl(IDockControl control)
        {
            if (_dockControls.ContainsKey(control.ID))
            {
                _dockControls.Remove(control.ID);
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
            var unsorted = new List<Window>();
            foreach (Window wnd in Application.Current.Windows)
            {
                if (wnd is BaseFloatWindow)
                {
                    unsorted.Add(wnd);
                }
            }

            unsorted.Add(MainWindow);
            _windows.AddRange(SortWindowsTopToBottom(unsorted));
        }

        private void ApplyLayout(LayoutSetting.LayoutSetting layout)
        {
            HideAll();

            var rootNode = layout.Layout;
            foreach (var item in rootNode.Element("Elements").Elements())
            {
                int.TryParse(item.Attribute("ID")?.Value, out var id);
                if (_dockControls.ContainsKey(id))
                {
                    _dockControls[id].Prototype.Load(item);
                }
            }

            _root.LoadLayout(rootNode.Element("ToolBar"));
            LoadRootPanel(rootNode.Element("Panel"));
            LoadFloatWindows(rootNode.Element("FloatWindows"));

            var node = rootNode.Element("ActiveItem");
            if (node != null)
            {
                int.TryParse(node.Value, out var id);
                if (_dockControls.ContainsKey(id))
                {
                    _dockControls[id].SetActive();
                }
            }
        }

        private XElement GenerateCurrentLayout()
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
            foreach (var item in _dockControls.Values.Select(dc => dc.Prototype))
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

        private void LoadFloatWindows(XElement element)
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

        private void LoadRootPanel(XElement element)
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

            for (var hWindow = Win32Helper.GetTopWindow(IntPtr.Zero); hWindow != IntPtr.Zero; hWindow = Win32Helper.GetWindow(hWindow, Win32Helper.GW_HWNDNEXT))
            {
                if (byHandle.ContainsKey(hWindow))
                {
                    yield return byHandle[hWindow];
                }
            }
        }

        #endregion
    }
}