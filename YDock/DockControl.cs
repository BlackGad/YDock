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
        ///     Can switch to floating mode
        /// </summary>
        public bool CanFloat
        {
            get { return Prototype != null && Prototype.CanFloat; }
        }

        /// <summary>
        ///     Can switch to Dock mode
        /// </summary>
        public bool CanDock
        {
            get { return Prototype != null && Prototype.CanDock; }
        }

        /// <summary>
        ///     Can switch to Document mode
        /// </summary>
        public bool CanDockAsDocument
        {
            get { return Prototype != null && Prototype.CanDockAsDocument; }
        }

        /// <summary>
        ///     Is it possible to switch the auto hide state
        /// </summary>
        public bool CanSwitchAutoHideStatus
        {
            get { return Prototype != null && Prototype.CanSwitchAutoHideStatus; }
        }

        /// <summary>
        ///     Can hide
        /// </summary>
        public bool CanHide
        {
            get { return Prototype != null && Prototype.CanHide; }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        ///     The method of universal display.
        ///     The displayed mode (Dock, Float, AnchorSide) is related to the current Status
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
        ///     This method hides the item directly from the user interface (CanSelect is set to False)
        /// </summary>
        public void Hide()
        {
            if (Content is IDockDocSource source && !source.AllowClose()) return;
            Prototype?.Hide();
        }

        /// <summary>
        ///     Turn to floating window
        /// </summary>
        public void ToFloat(bool isActive = true)
        {
            Prototype?.ToFloat(isActive);
        }

        /// <summary>
        ///     Switch to Dock mode
        /// </summary>
        public void ToDock(bool isActive = true)
        {
            Prototype?.ToDock(isActive);
        }

        /// <summary>
        ///     Switch to Document mode
        /// </summary>
        public void ToDockAsDocument(bool isActive = true)
        {
            Prototype?.ToDockAsDocument(isActive);
        }

        /// <summary>
        ///     Switch to Document mode
        /// </summary>
        public void ToDockAsDocument(int index, bool isActive = true)
        {
            Prototype?.ToDockAsDocument(index, isActive);
        }

        /// <summary>
        ///     Switch between Normal and DockBar modes
        /// </summary>
        public void SwitchAutoHideStatus()
        {
            Prototype?.SwitchAutoHideStatus();
        }

        /// <summary>
        ///     Set CanSelect to False and remove the item from the interface
        ///     For Normal or Float mode, the effect is the same as the Hide method.
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