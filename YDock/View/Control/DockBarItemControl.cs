using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YDock.Interface;
using YDock.Model;

namespace YDock.View
{
    public class DockBarItemControl : ContentControl,
                                      IDockView
    {
        #region Constructors

        static DockBarItemControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockBarItemControl), new FrameworkPropertyMetadata(typeof(DockBarItemControl)));
        }

        internal DockBarItemControl(IDockView dockViewParent)
        {
            DockViewParent = dockViewParent;
        }

        #endregion

        #region Properties

        public ILayoutGroup Container
        {
            get { return DockViewParent?.Model as ILayoutGroup; }
        }

        #endregion

        #region Override members

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var ele = Content as DockElement;
            if (ele == Container.DockManager.AutoHideElement)
            {
                Container.ShowWithActive(null);
            }
            else
            {
                Container.ShowWithActive(ele);
            }

            base.OnMouseLeftButtonDown(e);
        }

        #endregion

        #region IDockView Members

        public IDockModel Model
        {
            get { return null; }
        }

        public IDockView DockViewParent { get; private set; }

        public void Dispose()
        {
            DockViewParent = null;
        }

        #endregion
    }
}