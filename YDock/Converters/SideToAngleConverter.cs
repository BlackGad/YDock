using System;
using System.Windows.Data;
using YDock.Enum;

namespace YDock.Converters
{
    [ValueConversion(typeof(DockSide), typeof(double))]
    public class SideToAngleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (Equals(value, DockSide.Right)) return 90.0;
            if (Equals(value, DockSide.Left)) return 270;

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}