using MainLib.ViewModels;
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
    public class CurrentPageToBoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CurrentPage currentPage = (CurrentPage)value;
            string sender = (string)parameter;
            if ((currentPage == CurrentPage.Login && sender == "Login") || (currentPage == CurrentPage.Register && sender == "Register"))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
