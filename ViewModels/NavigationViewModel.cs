﻿using Lib.DataAccessLayer.Models;
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
        private readonly Shared services;
        private string _workLabel;
        private bool _isShowingDialog;

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
        public bool IsShowingDialog
        {
            get { return _isShowingDialog; }
            set { _isShowingDialog = value; OnPropertyChanged("IsShowingDialog"); }
        }

        public ICommand UpdateViewCommand { get; set; }

        public NavigationViewModel()
        {
            this.services = Shared.GetInstance();
            this.services.SetWorkingStatusAction(WorkStatus);
            this.services.SetShowDialogAction(SetDialogOverlayStatus);
            this.User = services.User;

            UpdateViewCommand = new UpdateViewCommand(Navigate);
            UpdateViewCommand.Execute(ViewType.Home);
            // Set admin/user status
            User.Admin = new UserRepo().IsAdmin(User);
            OnPropertyChanged("User");
            this.Title = User.Username;
        }

        public void Navigate(BaseViewModel viewModel)
        {
            this.SelectedViewModel = viewModel;
        }

        public void WorkStatus(bool isWorking, string label = "Working...")
        {
            this.IsBusy = isWorking;
            this.WorkLabel = label;
        }
        public void SetDialogOverlayStatus(bool isShowingDialog)
        {
            this.IsShowingDialog = isShowingDialog;
        }
    }
}
