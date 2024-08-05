﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MainLib.Views.Converters
{
    public class KeywordMatchColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null)
            {
                return (SolidColorBrush)new BrushConverter().ConvertFrom("#e5e7eb");
            }

            string highlights = values[0].ToString();
            string currentKeywords = values[1].ToString();

            if (highlights.ToLower().Contains(currentKeywords.ToLower()))
            {
                return (SolidColorBrush)new BrushConverter().ConvertFrom("#fff4be");
            }

            return (SolidColorBrush)new BrushConverter().ConvertFrom("#e5e7eb");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}