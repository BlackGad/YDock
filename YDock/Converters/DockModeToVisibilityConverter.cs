using System;
using System.Windows;
using System.Windows.Data;
using YDock.Enum;

namespace YDock.Converters
{
    [ValueConversion(typeof(DockMode), typeof(Visibility))]
    public class DockModeToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Equals(value, DockMode.Float) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}