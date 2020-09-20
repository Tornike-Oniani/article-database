using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Lib.ViewModels.Services.Dialogs;
using MaterialDesignThemes.Wpf;

namespace Lib.Views.Converters
{
    public class DialogTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DialogType? kind = (DialogType?)value;

            switch (kind)
            {
                case DialogType.Error:
                    return PackIconKind.CloseCircle;
                case DialogType.Warning:
                    return PackIconKind.Warning;
                case DialogType.Question:
                    return PackIconKind.QuestionMarkCircle;
                case DialogType.Information:
                    return PackIconKind.InfoCircle;
                case DialogType.Success:
                    return PackIconKind.CheckCircle;
                default:
                    return PackIconKind.ErrorOutline;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
