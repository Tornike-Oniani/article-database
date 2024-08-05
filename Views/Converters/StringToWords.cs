using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MainLib.Views.Converters
{
    internal class StringToWords : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new string[0];
            }

            string[] words = value.ToString().Split(',').Select(word => word.ToString().Trim()).ToArray();

            string separator = "";
            if (parameter == null)
            {
                separator = ",";
            }
            else if (parameter.ToString() == "NoSeparator")
            {
                separator = "";
            }
            else
            {
                separator = ",";
            }

            for (int i = 0; i < words.Length - 1; i++)
            {
                words[i] = words[i] + separator;
            }

            return words;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
