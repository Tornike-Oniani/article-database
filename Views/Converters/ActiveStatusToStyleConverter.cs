using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MainLib.Views.Converters
{
    internal class ActiveStatusToStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isActive = (bool)value;
            if (isActive)
            {
                return Application.Current.Resources["MaterialDesignFlatAccentBgButton"];
            }

            return Application.Current.Resources["MaterialDesignOutlinedButton"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
