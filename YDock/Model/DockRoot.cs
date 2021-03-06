﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Xml.Linq;
using YDock.Enum;
using YDock.Interface;
using YDock.Model.Layout;

namespace YDock.Model
{
    public class DockRoot : DependencyObject,
                            INotifyPropertyChanged,
                            IDockModel
    {
        private DockSideGroup _bottomSide;
        private DockManager _dockManager;
        private DockSideGroup _leftSide;
        private DockSideGroup _rightSide;
        private DockSideGroup _topSide;

        #region Properties

        public DockSideGroup BottomSide
        {
            get { return _bottomSide; }
            set
            {
                if (_bottomSide == value) return;

                _bottomSide?.Dispose();

                _bottomSide = value;

                if (_bottomSide != null)
                {
                    _bottomSide.Root = this;
                    _bottomSide.Side = DockSide.Bottom;
                }

                PropertyChanged(this, new PropertyChangedEventArgs("BottomSide"));
            }
        }

        public DockSideGroup LeftSide
        {
            get { return _leftSide; }
            set
            {
                if (_leftSide == value) return;

                _leftSide?.Dispose();

                _leftSide = value;

                if (_leftSide != null)
                {
                    _leftSide.Root = this;
                    _leftSide.Side = DockSide.Left;
                }

                PropertyChanged(this, new PropertyChangedEventArgs("LeftSide"));
            }
        }

        public DockSideGroup RightSide
        {
            get { return _rightSide; }
            set
            {
                if (_rightSide == value) return;

                _rightSide?.Dispose();

                _rightSide = value;

                if (_rightSide != null)
                {
                    _rightSide.Root = this;
                    _rightSide.Side = DockSide.Right;
                }

                PropertyChanged(this, new PropertyChangedEventArgs("RightSide"));
            }
        }

        public DockSideGroup TopSide
        {
            get { return _topSide; }
            set
            {
                if (_topSide == value) return;

                _topSide?.Dispose();

                _topSide = value;

                if (_topSide != null)
                {
                    _topSide.Root = this;
                    _topSide.Side = DockSide.Top;
                }

                PropertyChanged(this, new PropertyChangedEventArgs("TopSide"));
            }
        }

        internal List<BaseLayoutGroup> DocumentModels { get; set; }

        #endregion

        #region IDockModel Members

        public DockManager DockManager
        {
            get { return _dockManager; }
            set
            {
                _dockManager = value;
                if (_dockManager != null)
                {
                    _InitSide();
                }
            }
        }

        public IDockView View { get; internal set; }

        public DockSide Side
        {
            get { return DockSide.None; }
        }

        public void Dispose()
        {
            DocumentModels.Clear();
            DocumentModels = null;
            LeftSide = null;
            RightSide = null;
            TopSide = null;
            BottomSide = null;
            _dockManager = null;
            PropertyChanged = null;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion

        #region Members

        public XElement GenerateLayout()
        {
            var element = new XElement("ToolBar");

            // Left bar
            var node = new XElement("LeftBar");
            foreach (var item in LeftSide.Children)
            {
                node.Add(new XElement("Item", item.ID));
            }

            element.Add(node);

            // Top bar
            node = new XElement("TopBar");
            foreach (var item in TopSide.Children)
            {
                node.Add(new XElement("Item", item.ID));
            }

            element.Add(node);

            // Right bar
            node = new XElement("RightBar");
            foreach (var item in RightSide.Children)
            {
                node.Add(new XElement("Item", item.ID));
            }

            element.Add(node);

            // Bottom bar
            node = new XElement("BottomBar");
            foreach (var item in BottomSide.Children)
            {
                node.Add(new XElement("Item", item.ID));
            }

            element.Add(node);

            return element;
        }

        public void LoadLayout(XElement root)
        {
            // ReSharper disable once PossibleNullReferenceException
            foreach (var item in root.Element("LeftBar").Elements())
            {
                var id = int.Parse(item.Value);
                var element = _dockManager.GetDockControl(id);
                element.Prototype.ToDockSide(DockSide.Left);
            }
            // ReSharper disable once PossibleNullReferenceException
            foreach (var item in root.Element("TopBar").Elements())
            {
                var id = int.Parse(item.Value);
                var element = _dockManager.GetDockControl(id);
                element.Prototype.ToDockSide(DockSide.Top);
            }
            // ReSharper disable once PossibleNullReferenceException
            foreach (var item in root.Element("RightBar").Elements())
            {
                var id = int.Parse(item.Value);
                var element = _dockManager.GetDockControl(id);
                element.Prototype.ToDockSide(DockSide.Right);
            }
            // ReSharper disable once PossibleNullReferenceException
            foreach (var item in root.Element("BottomBar").Elements())
            {
                var id = int.Parse(item.Value);
                var element = _dockManager.GetDockControl(id);
                element.Prototype.ToDockSide(DockSide.Bottom);
            }
        }

        internal void AddSideChild(IDockElement element, DockSide side)
        {
            switch (side)
            {
                case DockSide.Left:
                    LeftSide.Attach(element);
                    break;
                case DockSide.Right:
                    RightSide.Attach(element);
                    break;
                case DockSide.Top:
                    TopSide.Attach(element);
                    break;
                case DockSide.Bottom:
                    BottomSide.Attach(element);
                    break;
            }
        }

        private void _InitSide()
        {
            LeftSide = new DockSideGroup();
            RightSide = new DockSideGroup();
            TopSide = new DockSideGroup();
            BottomSide = new DockSideGroup();
            DocumentModels = new List<BaseLayoutGroup>();
            DocumentModels.Add(new LayoutDocumentGroup(DockMode.Normal, _dockManager));
        }

        #endregion
    }
}