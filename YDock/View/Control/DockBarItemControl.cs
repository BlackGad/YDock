using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YDock.Interface;
using YDock.Model.Element;

namespace YDock.View.Control
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
            var element = Content as DockElement;
            if (element == Container.DockManager.AutoHideElement)
            {
                Container.ShowWithActive(null);
            }
            else
            {
                Container.ShowWithActive(element);
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