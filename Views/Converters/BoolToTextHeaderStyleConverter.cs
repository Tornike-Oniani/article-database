using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MainLib.Views.Converters
{
    public class BoolToTextHeaderStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if ((bool)value)
            {
                return Application.Current.Resources["MediumHeader"];
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
