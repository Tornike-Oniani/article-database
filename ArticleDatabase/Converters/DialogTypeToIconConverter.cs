using ArticleDatabase.Dialogs.DialogService;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ArticleDatabase.Converters
{
    class DialogTypeToIconConverter : IValueConverter
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
