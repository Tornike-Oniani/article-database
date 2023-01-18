using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace MainLib.Views.Converters
{
    public class SortConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return null;
            }

            string selectedSort = value.ToString();

            if (selectedSort.Contains(parameter.ToString()))
            {
                if (selectedSort.Contains("ASC")) { return "Ascending"; }
                if (selectedSort.Contains("DESC")) { return "Descending"; }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
