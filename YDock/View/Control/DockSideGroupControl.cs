﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using YDock.Enum;
using YDock.Interface;
using YDock.Model;

namespace YDock.View
{
    public class DockSideGroupControl : ItemsControl, IView
    {
        static DockSideGroupControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockSideGroupControl), new FrameworkPropertyMetadata(typeof(DockSideGroupControl)));
            FocusableProperty.OverrideMetadata(typeof(DockSideGroupControl), new FrameworkPropertyMetadata(false));
        }

        public DockSideGroupControl(ILayoutGroup model)
        {
            Model = model;

            SetBinding(ItemsSourceProperty, new Binding("Model.Children_CanSelect") { Source = this });

            var transform = new RotateTransform();
            switch ((Model as ILayout).Side)
            {
                case DockSide.Left:
                    transform.Angle = 270;
                    break;
                case DockSide.Right:
                    transform.Angle = 90;
                    break;
            }
            LayoutTransform = transform;
        }



        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private ILayoutGroup _model;

        public IModel Model
        {
            get
            {
                return _model;
            }
            set
            {
                if (_model != null) _model.View = null;
                if (_model != value)
                {
                    _model = value as ILayoutGroup;
                    _model.View = this;
                }
            }
        }

        public IEnumerable<ILayoutElement> Children
        {
            get
            {
                return Items.Cast<ILayoutElement>();
            }
        }

        public DockSide Side
        {
            get
            {
                return _model.Side;
            }
        }

        public YDock DockManager
        {
            get
            {
                return _model.DockManager;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DockSideItemControl(this._model);
        }
    }
}