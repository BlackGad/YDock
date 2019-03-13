using System;
using System.Windows.Data;
using System.Windows.Media;
using YDock.Enum;

namespace YDock.Converters
{
    [ValueConversion(typeof(DockMode), typeof(Transform))]
    public class DockModeToTransformConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Equals(value, DockMode.DockBar) ? new RotateTransform(90) : Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}