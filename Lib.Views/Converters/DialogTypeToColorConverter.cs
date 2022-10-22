using Lib.ViewModels.Services.Dialogs;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Lib.Views.Converters
{
    public class DialogTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DialogType type = (DialogType)value;
            switch (type)
            {
                case DialogType.Warning:
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#eac85d");  // #ffbb34
                case DialogType.Question:
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#17a2b8");  // #3094C0
                case DialogType.Error:
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#FFB94A69");
                case DialogType.Information:
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#3094C0");
                case DialogType.Success:
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#01a79b");
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
