using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private Action<bool> _workStatus;
        private IDialogService _dialogService;

        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }

        public RelayCommand RegisterCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        public RegisterViewModel(Action<bool> workStatus, IDialogService dialogService)
        {
            this._workStatus = workStatus;
            this._dialogService = dialogService;

            this.RegisterCommand = new RelayCommand(Register);
            this.CancelCommand = new RelayCommand(Cancel);
        }

        public async void Register(object input = null)
        {
            Window.Close();

            _workStatus(true);

            int admin = Username.Contains("_adminGfK") ? 1 : 0;
            Username = Username.Replace("_adminGfK", "");
            bool register = false;

            await Task.Run(() =>
            {
                User user = new User() { Username = this.Username, Password = this.Password };
                register = new UserRepo().Register(user, admin);
            });

            if (register)
            {
                _dialogService.OpenDialog(new DialogOkViewModel("New user created successfuly", "Result", DialogType.Success));
            }
            else
            {
                _dialogService.OpenDialog(new DialogOkViewModel("Username is already taken", "Warning", DialogType.Warning));
            }

            _workStatus(false);
        }

        public void Cancel(object input = null)
        {
            Window.Close();
        }
    }
}
