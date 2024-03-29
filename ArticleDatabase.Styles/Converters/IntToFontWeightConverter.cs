﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lib.Styles.Converters
{
    public class IntToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == 1)
            {
                return FontWeights.Bold;
            }
            else
            {
                return FontWeights.SemiBold;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
