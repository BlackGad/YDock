using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using YDock.Enum;
using YDock.Interface;
using YDock.Model.Element;

namespace YDock.Model.Layout
{
    public abstract class BaseLayoutGroup : ILayoutGroup
    {
        protected ObservableCollection<IDockElement> _children = new ObservableCollection<IDockElement>();

        protected DockMode _mode;

        protected DockSide _side;

        #region Constructors

        protected BaseLayoutGroup()
        {
            _children.CollectionChanged += OnChildrenCollectionChanged;
        }

        #endregion

        #region Properties

        public IReadOnlyList<IDockElement> Children
        {
            get { return _children; }
        }

        public IReadOnlyList<IDockElement> SelectableChildren
        {
            get
            {
                if (_children == null) return Enumerable.Empty<IDockElement>().ToList();
                return _children.Where(c => c.CanSelect).ToList();
            }
        }

        #endregion

        #region ILayoutGroup Members

        IReadOnlyList<IDockElement> ILayoutGroup.Children
        {
            get { return _children; }
        }

        public abstract DockManager DockManager { get; }

        public DockSide Side
        {
            get { return _side; }
            internal set
            {
                if (_side != value)
                {
                    _side = value;
                    foreach (var child in _children.OfType<DockElement>())
                    {
                        child.Side = value;
                    }
                }
            }
        }

        public IDockView View { get; protected internal set; }

        public DockMode Mode
        {
            get { return _mode; }
            internal set
            {
                if (_mode != value)
                {
                    _mode = value;
                    foreach (var child in _children.OfType<DockElement>())
                    {
                        if (child.Mode != _mode)
                        {
                            child.Mode = _mode;
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public int IndexOf(IDockElement child)
        {
            if (child == null) return -1;
            return _children.IndexOf(child as DockElement);
        }

        public void MoveTo(int src, int des)
        {
            if (src < _children.Count && src >= 0
                                      && des < _children.Count && des >= 0)
            {
                _children.Move(src, des);
            }
        }

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void ShowWithActive(IDockElement element, bool activate = true)
        {
            if (element != null && !element.CanSelect)
            {
                ((DockElement)element).CanSelect = true;
            }

            if (View != null)
            {
                DockManager.ActiveElement = element;
            }
        }

        public virtual void ShowWithActive(int index, bool toActive = true)
        {
            if (index < 0 || index >= _children.Count) throw new ArgumentOutOfRangeException("index");
            ShowWithActive(_children[index], toActive);
        }

        public virtual void Detach(IDockElement element)
        {
            if (element == null || !_children.Contains(element))
            {
                throw new InvalidOperationException("Detach Failed!");
            }

            _children.Remove(element);
            ((DockElement)element).IsVisible = false;
        }

        public virtual void Attach(IDockElement element, int index = -1)
        {
            if (element == null || element.Container != null)
            {
                throw new InvalidOperationException("Attach Failed!");
            }

            if (index < 0)
            {
                _children.Add(element);
            }
            else
            {
                _children.Insert(index, element);
            }

            ((DockElement)element).Mode = _mode;
        }

        public void CloseAll()
        {
            foreach (var child in _children.ToList())
            {
                child.Hide();
            }
        }

        public void CloseAllExcept(IDockElement element)
        {
            foreach (var child in _children.ToList())
            {
                if (child != element)
                {
                    child.Hide();
                }
            }
        }

        public abstract void ToFloat();

        public virtual void Dispose()
        {
            _children.CollectionChanged -= OnChildrenCollectionChanged;
            foreach (var child in _children)
            {
                child.PropertyChanged -= OnChildrenPropertyChanged;
                ((DockElement)child).Container = null;
            }

            _children.Clear();
            _children = null;
            PropertyChanged = null;
            View?.Dispose();
            View = null;
        }

        #endregion

        #region Event handlers

        protected virtual void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (DockElement item in e.OldItems)
                {
                    item.Container = null;
                    item.PropertyChanged -= OnChildrenPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (DockElement item in e.NewItems)
                {
                    item.Container = this;
                    item.Side = _side;
                    item.PropertyChanged += OnChildrenPropertyChanged;
                }
            }

            PropertyChanged(this, new PropertyChangedEventArgs("SelectableChildren"));
        }

        protected virtual void OnChildrenPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CanSelect" && sender is DockElement dockElement)
            {
                _children.CollectionChanged -= OnChildrenCollectionChanged;

                if (dockElement.CanSelect)
                {
                    //Re-add the element to the first
                    _children.Remove(dockElement);
                    _children.Insert(0, dockElement);
                    RaisePropertyChanged("SelectableChildren");
                }
                else
                {
                    //Re-add the element to the last one
                    _children.Remove(dockElement);
                    _children.Add(dockElement);
                }

                _children.CollectionChanged += OnChildrenCollectionChanged;
            }
        }

        #endregion
    }
}