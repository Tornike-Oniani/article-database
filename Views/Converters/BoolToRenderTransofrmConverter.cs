using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MainLib.Views.Converters
{
    internal class BoolToRenderTransofrmConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TranslateTransform transform = new TranslateTransform();
            transform.X = 0;
            transform.Y = 0;

            if (value == null || (bool)value)
            {
                return transform;
            }
            else
            {
                transform.Y = int.Parse(parameter.ToString());
                return transform;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
