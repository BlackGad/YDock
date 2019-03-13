using System.ComponentModel;
using System.Windows.Media;
using YDock.Enum;
using YDock.Interface;
using YDock.Model;
using YDock.Model.Element;

namespace YDock
{
    public class DockControl : IDockControl
    {
        #region Constructors

        internal DockControl(IDockElement prototype)
        {
            ProtoType = prototype;
            (ProtoType as DockElement).DockControl = this;
            prototype.PropertyChanged += OnPrototypePropertyChanged;
        }

        #endregion

        #region IDockControl Members

        public IDockElement ProtoType { get; private set; }

        public int ID
        {
            get { return ProtoType.ID; }
        }

        public string Title
        {
            get { return ProtoType.Title; }
            set { ProtoType.Title = value; }
        }

        public ImageSource ImageSource
        {
            get { return ProtoType.ImageSource; }
        }

        public object Content
        {
            get { return ProtoType.Content; }
        }

        public DockSide Side
        {
            get { return ProtoType.Side; }
        }

        public DockManager DockManager
        {
            get { return ProtoType.DockManager; }
        }

        public double DesiredWidth
        {
            get { return ProtoType.DesiredWidth; }

            set { ProtoType.DesiredWidth = value; }
        }

        public double DesiredHeight
        {
            get { return ProtoType.DesiredHeight; }

            set { ProtoType.DesiredHeight = value; }
        }

        public double FloatLeft
        {
            get { return ProtoType.FloatLeft; }

            set { ProtoType.FloatLeft = value; }
        }

        public double FloatTop
        {
            get { return ProtoType.FloatTop; }

            set { ProtoType.FloatTop = value; }
        }

        public bool IsDocument
        {
            get { return ProtoType.IsDocument; }
        }

        public DockMode Mode
        {
            get { return ProtoType.Mode; }
        }

        public bool IsVisible
        {
            get { return ProtoType.IsVisible; }
        }

        public bool IsActive
        {
            get { return ProtoType.IsActive; }
        }

        public bool CanSelect
        {
            get { return ProtoType.CanSelect; }
        }

        public ILayoutGroup Container
        {
            get { return ProtoType.Container; }
        }

        public bool IsDocked
        {
            get { return ProtoType.IsDocked; }
        }

        public bool IsFloat
        {
            get { return ProtoType.IsFloat; }
        }

        public bool IsAutoHide
        {
            get { return ProtoType.IsAutoHide; }
        }

        /// <summary>
        ///     是否可以转为浮动模式
        /// </summary>
        public bool CanFloat
        {
            get { return ProtoType != null && ProtoType.CanFloat; }
        }

        /// <summary>
        ///     是否可以转为Dock模式
        /// </summary>
        public bool CanDock
        {
            get { return ProtoType != null && ProtoType.CanDock; }
        }

        /// <summary>
        ///     是否可以转为Document模式
        /// </summary>
        public bool CanDockAsDocument
        {
            get { return ProtoType != null && ProtoType.CanDockAsDocument; }
        }

        /// <summary>
        ///     是否可以切换自动隐藏状态
        /// </summary>
        public bool CanSwitchAutoHideStatus
        {
            get { return ProtoType != null && ProtoType.CanSwitchAutoHideStatus; }
        }

        /// <summary>
        ///     是否可以隐藏
        /// </summary>
        public bool CanHide
        {
            get { return ProtoType != null && ProtoType.CanHide; }
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
                Container.ShowWithActive(ProtoType, activate);
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

            ProtoType?.Hide();
        }

        /// <summary>
        ///     转为浮动窗口
        /// </summary>
        public void ToFloat(bool isActive = true)
        {
            ProtoType?.ToFloat(isActive);
        }

        /// <summary>
        ///     转为Dock模式
        /// </summary>
        public void ToDock(bool isActive = true)
        {
            ProtoType?.ToDock(isActive);
        }

        /// <summary>
        ///     转为Document模式
        /// </summary>
        public void ToDockAsDocument(bool isActive = true)
        {
            ProtoType?.ToDockAsDocument(isActive);
        }

        /// <summary>
        ///     转为Document模式
        /// </summary>
        public void ToDockAsDocument(int index, bool isActive = true)
        {
            ProtoType?.ToDockAsDocument(index, isActive);
        }

        /// <summary>
        ///     在Normal和DockBar模式间切换
        /// </summary>
        public void SwitchAutoHideStatus()
        {
            ProtoType?.SwitchAutoHideStatus();
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
                ProtoType.Container.ShowWithActive(ProtoType);
            }
            else if (DockManager.ActiveElement == ProtoType)
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
            ProtoType.PropertyChanged -= OnPrototypePropertyChanged;
            ProtoType.Dispose();
            ProtoType = null;
        }

        #endregion

        #region Event handlers

        private void OnPrototypePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged(ProtoType, new PropertyChangedEventArgs(e.PropertyName));
        }

        #endregion
    }
}