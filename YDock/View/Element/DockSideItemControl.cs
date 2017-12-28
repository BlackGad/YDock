﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YDock.Enum;
using YDock.Interface;
using YDock.Model;

namespace YDock.View
{
    public class DockSideItemControl : ContentControl, ILayoutElement
    {
        static DockSideItemControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockSideItemControl), new FrameworkPropertyMetadata(typeof(DockSideItemControl)));
            FocusableProperty.OverrideMetadata(typeof(DockSideItemControl), new FrameworkPropertyMetadata(false));
        }

        public DockSideItemControl(ILayoutContainer container)
        {
            _container = container;
        }

        private ILayoutContainer _container;
        public ILayoutContainer Container
        {
            get
            {
                return _container;
            }

            set
            {
                if (_container != value)
                    _container = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var parent = _container as YDockSideControl;
            var dock = parent.Parent as YDock;
            switch (parent.Model.Side)
            {
                case DockSide.Left:
                    if (dock.CenterGird.LeftSideContent.Model == Content)
                        dock.CenterGird.LeftSideContent.Model = null;
                    else dock.CenterGird.LeftSideContent.Model = Content as ILayoutElement;
                    break;
                case DockSide.Right:
                    if (dock.CenterGird.RightSideContent.Model == Content)
                        dock.CenterGird.RightSideContent.Model = null;
                    else dock.CenterGird.RightSideContent.Model = Content as ILayoutElement;
                    break;
                case DockSide.Top:
                    if (dock.CenterGird.TopSideContent.Model == Content)
                        dock.CenterGird.TopSideContent.Model = null;
                    else dock.CenterGird.TopSideContent.Model = Content as ILayoutElement;
                    break;
                case DockSide.Bottom:
                    if (dock.CenterGird.BottomSideContent.Model == Content)
                        dock.CenterGird.BottomSideContent.Model = null;
                    else dock.CenterGird.BottomSideContent.Model = Content as ILayoutElement;
                    break;
            }

            base.OnMouseLeftButtonDown(e);
        }
    }
}