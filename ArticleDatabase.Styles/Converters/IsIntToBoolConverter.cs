using System;
using System.Globalization;
using System.Windows.Data;

namespace Lib.Styles.Converters
{
    public class IsIntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.TryParse((string)value, out _);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
