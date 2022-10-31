using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Commands;
using MainLib.ViewModels.Main;
using MainLib.ViewModels.Popups;
using MainLib.ViewModels.Utils;
using System.Windows.Input;

namespace MainLib.ViewModels
{
    public class NavigationViewModel : BaseViewModel
    {
        private BaseViewModel _selectedViewModel;
        private User _user;
        private bool _isBusy;
        private Shared services;
        private string _workLabel;

        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set { _selectedViewModel = value; OnPropertyChanged("SelectedViewModel"); }
        }
        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged("IsBusy"); }
        }
        public string WorkLabel
        {
            get { return _workLabel; }
            set { _workLabel = value; OnPropertyChanged("WorkLabel"); }
        }


        public ICommand UpdateViewCommand { get; set; }
        public ICommand OpenSettingsCommand { get; set; }

        public NavigationViewModel()
        {
            this.services = Shared.GetInstance();
            services.SetWorkingStatusAction(WorkStatus);
            this.User = services.User;
            services.ThemeService.ChangeTheme(Properties.Settings.Default.Theme);

            UpdateViewCommand = new UpdateViewCommand(Navigate);
            UpdateViewCommand.Execute(ViewType.Home);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            // Set admin/user status
            User.Admin = new UserRepo().IsAdmin(User);
            OnPropertyChanged("User");
            this.Title = User.Username;
        }

        public void Navigate(BaseViewModel viewModel)
        {
            this.SelectedViewModel = viewModel;
        }
        public void OpenSettings(object input = null)
        {
            services.WindowService.OpenWindow(new SettingsViewModel(), passWindow: true);
        }

        public void WorkStatus(bool isWorking, string label = "Working...")
        {
            this.IsBusy = isWorking;
            this.WorkLabel = label;
        }
    }
}
