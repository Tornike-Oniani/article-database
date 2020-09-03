using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ArticleDatabase.Converters
{
    public class CheckToListConverter : IValueConverter
    {
        List<string> _columns;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _columns = value as List<string>;

            if (_columns.Contains(parameter.ToString()))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;

            if (isChecked)
                _columns.Add(parameter.ToString());
            else
                _columns.Remove(parameter.ToString());

            return _columns;
        }
    }
}
