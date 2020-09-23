using Lib.ViewModels.Services.Windows;
using Lib.Views.Services.Browser;
using Lib.Views.Services.Dialogs;
using Lib.Views.Services.Windows;
using SectionLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Section
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            new WindowService().OpenWindow(new NavigationViewModel(new DialogService(), new WindowService(), new BrowserService()), WindowType.MainWindow, false, false, false);
        }
    }
}
