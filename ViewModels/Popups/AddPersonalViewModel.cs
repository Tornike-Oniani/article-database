using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.DataAccessLayer.Models;
using MainLib.ViewModels.Utils;
using Lib.DataAccessLayer.Info;
using Lib.ViewModels.Services.Dialogs;

namespace MainLib.ViewModels.Popups
{
    public class AddPersonalDialogViewModel : BaseViewModel
    {
        private IDialogService _dialogService;

        public Article SelectedArticle { get; set; }
        public User User { get; set; }
        public string PersonalComment { get; set; }
        public int SIC { get; set; }

        public RelayCommand AddPersonalCommand { get; set; }

        public AddPersonalDialogViewModel(Article selectedArticle, User user, IDialogService dialogService)
        {
            this.Title = "Add comment & SIC";
            this._dialogService = dialogService;

            // 1. Set attributes from parent
            this.SelectedArticle = selectedArticle;
            this.User = user;
            PersonalComment = selectedArticle.PersonalComment;
            SIC = selectedArticle.SIC;

            // 2. Initialize commands
            AddPersonalCommand = new RelayCommand(AddPersonal);

        }

        public void AddPersonal(object input)
        {
            try
            {
                if (PersonalComment != SelectedArticle.PersonalComment || SIC != SelectedArticle.SIC)
                {
                    // 1. Send new comment and SIC values to parent
                    SelectedArticle.PersonalComment = PersonalComment;
                    SelectedArticle.SIC = SIC;
                    OnPropertyChanged("SelectedArticle");

                    // 2. Update record in database
                    (new ArticleRepo()).UpdatePersonal(SelectedArticle, User);

                    // 3. Track update record
                    PersonalInfo info = new PersonalInfo(SelectedArticle.Title, PersonalComment, SIC);
                    new Tracker(User).TrackPersonal(info);

                    // 3. Close window
                    (input as ICommand).Execute(null);
                }
            }
            catch(Exception e)
            {
                new BugTracker().Track("Data View (sub window)", "Add Personal", e.Message);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }
    }
}
