using System;
using System.ComponentModel;
using System.Windows;
using YDock.Model.Element;

namespace YDock.View.Element
{
    public class LayoutContentControl : System.Windows.Controls.Control,
                                        INotifyPropertyChanged,
                                        IDisposable
    {
        #region Property definitions

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(DockElement), typeof(LayoutContentControl));

        #endregion

        #region Constructors

        static LayoutContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutContentControl), new FrameworkPropertyMetadata(typeof(LayoutContentControl)));
        }

        #endregion

        #region Properties

        public DockElement Model
        {
            get { return (DockElement)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Model = null;
            DataContext = null;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion
    }
}