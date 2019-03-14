using System.ComponentModel;
using System.Windows.Media;
using YDock.Enum;
using YDock.Interface;
using YDock.Model.Element;

namespace YDock
{
    public class DockControl : IDockControl
    {
        #region Constructors

        internal DockControl(IDockElement prototype)
        {
            Prototype = prototype;
            ((DockElement)Prototype).DockControl = this;
            prototype.PropertyChanged += OnPrototypePropertyChanged;
        }

        #endregion

        #region IDockControl Members

        public IDockElement Prototype { get; private set; }

        public int ID
        {
            get { return Prototype.ID; }
        }

        public string Title
        {
            get { return Prototype.Title; }
            set { Prototype.Title = value; }
        }

        public ImageSource ImageSource
        {
            get { return Prototype.ImageSource; }
        }

        public object Content
        {
            get { return Prototype.Content; }
        }

        public DockSide Side
        {
            get { return Prototype.Side; }
        }

        public DockManager DockManager
        {
            get { return Prototype.DockManager; }
        }

        public double DesiredWidth
        {
            get { return Prototype.DesiredWidth; }

            set { Prototype.DesiredWidth = value; }
        }

        public double DesiredHeight
        {
            get { return Prototype.DesiredHeight; }

            set { Prototype.DesiredHeight = value; }
        }

        public double FloatLeft
        {
            get { return Prototype.FloatLeft; }

            set { Prototype.FloatLeft = value; }
        }

        public double FloatTop
        {
            get { return Prototype.FloatTop; }

            set { Prototype.FloatTop = value; }
        }

        public bool IsDocument
        {
            get { return Prototype.IsDocument; }
        }

        public DockMode Mode
        {
            get { return Prototype.Mode; }
        }

        public bool IsVisible
        {
            get { return Prototype.IsVisible; }
        }

        public bool IsActive
        {
            get { return Prototype.IsActive; }
        }

        public bool CanSelect
        {
            get { return Prototype.CanSelect; }
        }

        public ILayoutGroup Container
        {
            get { return Prototype.Container; }
        }

        public bool IsDocked
        {
            get { return Prototype.IsDocked; }
        }

        public bool IsFloat
        {
            get { return Prototype.IsFloat; }
        }

        public bool IsAutoHide
        {
            get { return Prototype.IsAutoHide; }
        }

        /// <summary>
        ///     是否可以转为浮动模式
        /// </summary>
        public bool CanFloat
        {
            get { return Prototype != null && Prototype.CanFloat; }
        }

        /// <summary>
        ///     是否可以转为Dock模式
        /// </summary>
        public bool CanDock
        {
            get { return Prototype != null && Prototype.CanDock; }
        }

        /// <summary>
        ///     是否可以转为Document模式
        /// </summary>
        public bool CanDockAsDocument
        {
            get { return Prototype != null && Prototype.CanDockAsDocument; }
        }

        /// <summary>
        ///     是否可以切换自动隐藏状态
        /// </summary>
        public bool CanSwitchAutoHideStatus
        {
            get { return Prototype != null && Prototype.CanSwitchAutoHideStatus; }
        }

        /// <summary>
        ///     是否可以隐藏
        /// </summary>
        public bool CanHide
        {
            get { return Prototype != null && Prototype.CanHide; }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        ///     通用显示的方法。
        ///     显示的模式（Dock，Float，AnchorSide）与当前Status有关
        /// </summary>
        public void Show(bool activate = true)
        {
            if (IsVisible && IsActive) return;
            if (Mode == DockMode.Float)
            {
                if (!IsDocument)
                {
                    ToFloat(activate);
                }
                else
                {
                    ToDockAsDocument(activate);
                }
            }
            else
            {
                Container.ShowWithActive(Prototype, activate);
            }
        }

        /// <summary>
        ///     此方法会直接从用户界面隐藏该项（CanSelect设为False）
        /// </summary>
        public void Hide()
        {
            if (Content is IDockDocSource
                && !(Content as IDockDocSource).AllowClose())
            {
                return;
            }

            Prototype?.Hide();
        }

        /// <summary>
        ///     转为浮动窗口
        /// </summary>
        public void ToFloat(bool isActive = true)
        {
            Prototype?.ToFloat(isActive);
        }

        /// <summary>
        ///     转为Dock模式
        /// </summary>
        public void ToDock(bool isActive = true)
        {
            Prototype?.ToDock(isActive);
        }

        /// <summary>
        ///     转为Document模式
        /// </summary>
        public void ToDockAsDocument(bool isActive = true)
        {
            Prototype?.ToDockAsDocument(isActive);
        }

        /// <summary>
        ///     转为Document模式
        /// </summary>
        public void ToDockAsDocument(int index, bool isActive = true)
        {
            Prototype?.ToDockAsDocument(index, isActive);
        }

        /// <summary>
        ///     在Normal和DockBar模式间切换
        /// </summary>
        public void SwitchAutoHideStatus()
        {
            Prototype?.SwitchAutoHideStatus();
        }

        /// <summary>
        ///     将CanSelect设为False，并从界面移除此项
        ///     对于Normal or Float模式，效果与Hide方法相同
        /// </summary>
        public void Close()
        {
            Hide();
        }

        public void SetActive(bool isActive = true)
        {
            if (isActive)
            {
                Prototype.Container.ShowWithActive(Prototype);
            }
            else if (DockManager.ActiveElement == Prototype)
            {
                DockManager.ActiveElement = null;
            }
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            DockManager.RemoveDockControl(this);
            Prototype.PropertyChanged -= OnPrototypePropertyChanged;
            Prototype.Dispose();
            Prototype = null;
        }

        #endregion

        #region Event handlers

        private void OnPrototypePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged(Prototype, new PropertyChangedEventArgs(e.PropertyName));
        }

        #endregion
    }
}