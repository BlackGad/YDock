using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using YDock.Enum;
using YDock.Interface;
using YDock.Model;

namespace YDock.View
{
    public class AnchorGroupWindow : BaseFloatWindow,
                                     INotifyPropertyChanged
    {
        private DockMenu menu;

        #region Constructors

        static AnchorGroupWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnchorGroupWindow), new FrameworkPropertyMetadata(typeof(AnchorGroupWindow)));
        }

        public AnchorGroupWindow(DockManager dockManager) : base(dockManager)
        {
            ShowInTaskbar = false;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     是否Content为<see cref="ILayoutGroupControl" />
        /// </summary>
        public bool IsSingleMode
        {
            get { return Content != null && Content is ILayoutGroupControl; }
        }

        /// <summary>
        ///     是否需要Border
        /// </summary>
        public bool NoBorder
        {
            get { return IsSingleMode && (Content as BaseGroupControl).Items.Count == 1; }
        }

        #endregion

        #region Override members

        public override void AttachChild(IDockView child, AttachMode mode, int index)
        {
            if (child is ILayoutPanel)
            {
                _heightEceeed += Constants.FloatWindowHeaderHeight;
            }

            Owner = DockManager.MainWindow;
            base.AttachChild(child, mode, index);
            if (child is BaseGroupControl)
            {
                (((child as BaseGroupControl).Model as BaseLayoutGroup).Children as ObservableCollection<IDockElement>).CollectionChanged += OnCollectionChanged;
            }
        }

        public override void DetachChild(IDockView child, bool force = true)
        {
            if (child is BaseGroupControl)
            {
                (((child as BaseGroupControl).Model as BaseLayoutGroup).Children as ObservableCollection<IDockElement>).CollectionChanged -= OnCollectionChanged;
            }

            if (force)
            {
                Owner = null;
            }

            base.DetachChild(child, force);
            UpdateSize();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            if (newContent != null)
            {
                UpdateTemplate();
            }
        }

        protected override IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32Helper.WM_NCLBUTTONDOWN:
                case Win32Helper.WM_NCRBUTTONDOWN:
                    ActiveSelf();
                    if (IsSingleMode && msg == Win32Helper.WM_NCRBUTTONDOWN)
                    {
                        if (menu == null)
                        {
                            _ApplyMenu((Child as BaseGroupControl).SelectedItem as IDockItem);
                        }

                        menu.IsOpen = true;
                    }

                    break;
            }

            return base.FilterMessage(hwnd, msg, wParam, lParam, ref handled);
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonDown(e);
            if (menu != null)
            {
                var p = e.GetPosition(this);
                if (p.Y < 20)
                {
                    menu.IsOpen = true;
                }
            }
        }

        public override void Recreate()
        {
            if (_needReCreate)
            {
                NeedReCreate = false;
                if (Child != null)
                {
                    var layoutCtrl = Child as BaseGroupControl;
                    layoutCtrl.IsDraggingFromDock = false;
                }
            }
        }

        protected override void OnMaximizeCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((e.OriginalSource as Button)?.Name == "Maximize")
            {
                e.CanExecute = IsSingleMode && WindowState == WindowState.Normal;
            }
            else
            {
                e.CanExecute = true;
            }
        }

        protected override void OnRestoreCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((e.OriginalSource as Button)?.Name == "Restore")
            {
                e.CanExecute = IsSingleMode && WindowState == WindowState.Maximized;
            }
            else
            {
                e.CanExecute = true;
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion

        #region Event handlers

        private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTemplate();
        }

        #endregion

        #region Members

        public void UpdateSize()
        {
            if (IsSingleMode)
            {
                _heightEceeed = 0;
            }
            else
            {
                _heightEceeed = Constants.FloatWindowHeaderHeight;
            }
        }

        public void UpdateTemplate()
        {
            PropertyChanged(this, new PropertyChangedEventArgs("IsSingleMode"));
            PropertyChanged(this, new PropertyChangedEventArgs("NoBorder"));
            CommandManager.InvalidateRequerySuggested();

            if (menu != null)
            {
                menu.Dispose();
                menu = null;
            }
        }

        private void _ApplyMenu(IDockItem item)
        {
            menu = new DockMenu(item);
            menu.Placement = PlacementMode.MousePoint;
        }

        private void ActiveSelf()
        {
            if (IsSingleMode)
            {
                ((Content as ILayoutGroupControl).Model as ILayoutGroup).ShowWithActive((Content as BaseGroupControl).SelectedIndex);
            }

            DockManager.MainWindow.Activate();
        }

        #endregion
    }
}