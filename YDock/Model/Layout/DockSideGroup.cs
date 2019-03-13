using System.Linq;
using System.Windows.Markup;
using YDock.Enum;
using YDock.Interface;

namespace YDock.Model.Layout
{
    [ContentProperty("Children")]
    public class DockSideGroup : BaseLayoutGroup
    {
        private DockRoot _root;

        #region Constructors

        public DockSideGroup()
        {
            _mode = DockMode.DockBar;
        }

        #endregion

        #region Properties

        public override DockManager DockManager
        {
            get { return _root.DockManager; }
        }

        public DockRoot Root
        {
            get { return _root; }
            set
            {
                if (_root != value)
                {
                    _root = value;
                }
            }
        }

        #endregion

        #region Override members

        public override void ShowWithActive(IDockElement element, bool toActive = true)
        {
            DockManager.AutoHideElement = element;
            base.ShowWithActive(element, toActive);
        }

        public override void Detach(IDockElement element)
        {
            base.Detach(element);
            if (DockManager.AutoHideElement == element)
            {
                DockManager.AutoHideElement = null;
            }
        }

        public override void ToFloat()
        {
            foreach (var child in _children.ToList())
            {
                child.ToFloat();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _root = null;
        }

        #endregion
    }
}