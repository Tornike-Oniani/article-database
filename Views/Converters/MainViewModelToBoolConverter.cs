using Lib.ViewModels.Base;
using MainLib.ViewModels.Main;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;

namespace MainLib.Views.Converters
{
    public class MainViewModelToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BaseViewModel vm = value as BaseViewModel;
            ViewType expectedType = (ViewType)parameter;

            return GetViewTypeFromViewModel(vm) == expectedType;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static ViewType GetViewTypeFromViewModel(BaseViewModel vm)
        {
            if (vm is HomeViewModel)
            {
                return ViewType.Home;
            }
            else if (vm is BrowseViewModel)
            {
                return ViewType.Browse;
            }
            else if (vm is DataEntryViewModel)
            {
                return ViewType.DataEntry;
            }
            else if (vm is DataViewViewModel)
            {
                return ViewType.DataView;
            }
            else if (vm is BookmarksViewModel)
            {
                return ViewType.Bookmarks;
            }
            else if (vm is ReferencesViewModel)
            {
                return ViewType.References;
            }
            else if (vm is SQLViewModel)
            {
                return ViewType.SQL;
            }
            else if (vm is SettingsViewModel)
            {
                return ViewType.Settings;
            }

            throw new ArgumentException("Not supported view model type");
        }
    }
}
