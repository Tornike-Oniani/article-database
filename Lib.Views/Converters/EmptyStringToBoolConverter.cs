using System;
using System.Globalization;
using System.Windows.Data;

namespace Lib.Views.Converters
{
    public class EmptyStringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { return false; }
            return !String.IsNullOrWhiteSpace(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
