using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using YDock.Enum;
using YDock.Interface;
using YDock.Model.Layout;

namespace YDock.View.Control
{
    public class DockBarGroupControl : ItemsControl,
                                       IDockView
    {
        private ILayoutGroup _model;

        #region Constructors

        static DockBarGroupControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockBarGroupControl), new FrameworkPropertyMetadata(typeof(DockBarGroupControl)));
            FocusableProperty.OverrideMetadata(typeof(DockBarGroupControl), new FrameworkPropertyMetadata(false));
        }

        internal DockBarGroupControl(ILayoutGroup model)
        {
            Model = model;

            SetBinding(ItemsSourceProperty, new Binding("Model.SelectableChildren") { Source = this });

            var transform = new RotateTransform();
            switch (Model.Side)
            {
                case DockSide.Left:
                case DockSide.Right:
                    transform.Angle = 90;
                    break;
            }

            LayoutTransform = transform;
        }

        #endregion

        #region Properties

        public IEnumerable<IDockElement> Children
        {
            get { return Items.Cast<IDockElement>(); }
        }

        public DockSide Side
        {
            get { return _model.Side; }
        }

        #endregion

        #region Override members

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DockBarItemControl(this);
        }

        #endregion

        #region IDockView Members

        public IDockModel Model
        {
            get { return _model; }
            set
            {
                if (_model == value) return;
                if (_model is DockSideGroup oldModel) oldModel.View = null;
                _model = value as ILayoutGroup;
                if (_model is DockSideGroup newModel) newModel.View = this;
            }
        }

        public IDockView DockViewParent
        {
            get { return _model.DockManager; }
        }

        public void Dispose()
        {
            BindingOperations.ClearBinding(this, ItemsSourceProperty);
            Items.Clear();
            Model = null;
        }

        #endregion
    }
}