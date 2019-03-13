using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using YDock.Enum;
using YDock.Interface;

namespace YDock.Model
{
    public class LayoutDocumentGroup : LayoutGroup
    {
        private bool _isActive;

        #region Constructors

        public LayoutDocumentGroup(DockMode mode, DockManager dockManager) : base(DockSide.None, mode, dockManager)
        {
        }

        #endregion

        #region Properties

        public IEnumerable<IDockElement> ChildrenSorted
        {
            get
            {
                var listSorted = Children_CanSelect.ToList();
                listSorted.Sort();
                return listSorted;
            }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    RaisePropertyChanged("IsActive");
                }
            }
        }

        #endregion

        #region Override members

        protected override void OnChildrenPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnChildrenPropertyChanged(sender, e);
            if (e.PropertyName == "IsActive")
            {
                IsActive = (sender as IDockElement).IsActive;
            }
        }

        public override void Attach(IDockElement element, int index = -1)
        {
            if (element.Side != DockSide.None)
            {
                throw new ArgumentException("Side is illegal!");
            }

            base.Attach(element, index);
            if (element.IsActive) IsActive = true;
        }

        public override void Detach(IDockElement element)
        {
            base.Detach(element);
            if (element.IsActive) IsActive = false;
        }

        #endregion
    }
}