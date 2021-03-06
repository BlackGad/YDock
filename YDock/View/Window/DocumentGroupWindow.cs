﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using YDock.Enum;
using YDock.Global.Commands;
using YDock.Interface;
using YDock.View.Control;

namespace YDock.View.Window
{
    public class DocumentGroupWindow : BaseFloatWindow
    {
        private readonly Timeline _backgroundAnimation;
        private readonly Storyboard _board;
        private readonly Timeline _borderBrushAnimation;

        private readonly Timeline _thicknessAnimation;

        private DockPanel _header;

        #region Constructors

        static DocumentGroupWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DocumentGroupWindow), new FrameworkPropertyMetadata(typeof(DocumentGroupWindow)));
        }

        public DocumentGroupWindow(DockManager dockManager) : base(dockManager)
        {
            _thicknessAnimation = new ThicknessAnimation(new Thickness(1), new Duration(TimeSpan.FromMilliseconds(1)))
            {
                BeginTime = TimeSpan.FromSeconds(0.4)
            };
            _backgroundAnimation = new ColorAnimation(Colors.WhiteSmoke, ResourceManager.SplitterBrushVertical.Color, new Duration(TimeSpan.FromMilliseconds(1)));
            _borderBrushAnimation = new ColorAnimation(Colors.WhiteSmoke, ResourceManager.WindowBorderBrush.Color, new Duration(TimeSpan.FromMilliseconds(1)))
            {
                BeginTime = TimeSpan.FromSeconds(0.2)
            };
            _board = new Storyboard();
            _board.Children.Add(_thicknessAnimation);
            _board.Children.Add(_backgroundAnimation);
            _board.Children.Add(_borderBrushAnimation);
        }

        #endregion

        #region Override members

        protected override void OnInitialized(EventArgs e)
        {
            CommandBindings.Add(new CommandBinding(GlobalCommands.MinimizeCommand, OnMinimizeExecute, OnMinimizeCanExecute));
            base.OnInitialized(e);
        }

        public override void AttachChild(IDockView child, AttachMode mode, int index)
        {
            //The next 2 is border Thickness
            _widthExceed += Constants.DocumentWindowPadding * 2 + 2;
            _heightExceed += Constants.DocumentWindowPadding * 2 + Constants.FloatWindowHeaderHeight + 2;
            base.AttachChild(child, mode, index);
        }

        public override void Recreate()
        {
            if (Child == null) return;
            if (_needReCreate)
            {
                NeedReCreate = false;
                var layoutControl = (BaseGroupControl)Child;
                layoutControl.IsDraggingFromDock = false;
                _header.Visibility = Visibility.Visible;
                Storyboard.SetTarget(_thicknessAnimation, layoutControl);
                Storyboard.SetTargetProperty(_thicknessAnimation, new PropertyPath(BorderThicknessProperty));
                Storyboard.SetTarget(_backgroundAnimation, this);
                Storyboard.SetTargetProperty(_backgroundAnimation, new PropertyPath("(0).(1)", BackgroundProperty, SolidColorBrush.ColorProperty));
                Storyboard.SetTarget(_borderBrushAnimation, this);
                Storyboard.SetTargetProperty(_borderBrushAnimation, new PropertyPath("(0).(1)", BorderBrushProperty, SolidColorBrush.ColorProperty));
                _board.Begin(this);

                Top -= Constants.FloatWindowHeaderHeight;
            }
            else
            {
                NeedReCreate = true;
                Background = Brushes.WhiteSmoke;
                BorderBrush = Brushes.WhiteSmoke;
                var layoutControl = (BaseGroupControl)Child;
                layoutControl.BorderThickness = new Thickness(1);
                layoutControl.IsDraggingFromDock = true;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _header = (DockPanel)GetTemplateChild("PART_Header");
            if (_needReCreate && _header != null)
            {
                _header.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Event handlers

        private void OnMinimizeCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OnMinimizeExecute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        #endregion
    }
}