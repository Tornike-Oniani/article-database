using MainLib.ViewModels.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MainLib.Views.Converters
{
    public class RecentSearchToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SearchEntry searchEntry = value as SearchEntry;
            string result = "";
            if (!String.IsNullOrEmpty(searchEntry.Terms))
            {
                result += searchEntry.Terms;
            }
            if (!String.IsNullOrEmpty(searchEntry.Authors))
            {
                result += String.IsNullOrEmpty(result) ? "" : " | ";
                result += searchEntry.Authors;
            }
            if (!String.IsNullOrEmpty(searchEntry.Year))
            {
                result += String.IsNullOrEmpty(result) ? "" : " | ";
                result += searchEntry.Year;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
