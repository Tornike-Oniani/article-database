using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Utils;
using System;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MainLib.ViewModels.Popups
{
    public class AbstractEditorViewModel : BaseViewModel
    {
        private Article _article;
        private string _abstractBody;

        public string AbstractBody
        {
            get { return _abstractBody; }
            set { _abstractBody = value; OnPropertyChanged("AbstractBody"); }
        }

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public AbstractEditorViewModel(Article article)
        {
            this._article = article;
            this.AbstractBody = article.AbstractBody;

            this.SaveCommand = new RelayCommand(Save);
            this.CancelCommand = new RelayCommand(Cancel);
        }

        public void Save(object input = null)
        {
            // Switch multiple spaces with one
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            AbstractBody = regex.Replace(AbstractBody, " ");
            AbstractBody = AbstractBody.Trim();

            // Check for unusual characters
            if (!String.IsNullOrWhiteSpace(AbstractBody))
            {
                Regex unusualCharacters = new Regex("^[A-Za-z0-9 .,'()+/_?:\"\\&*%$#@<>{}!=;-]+$");
                if (!unusualCharacters.IsMatch(AbstractBody))
                {
                    Shared.GetInstance().DialogService.OpenDialog(new DialogOkViewModel("This input contains unusual characters, please retype it manually. (Don't copy & paste!)", "Error", DialogType.Error));
                    AbstractBody = null;
                    return;
                }
            }

            // If article has abstract change it (We have to do this check because we keep abstracts in separate table)
            if (!String.IsNullOrEmpty(_article.AbstractBody)) 
            { 
                new AbstractRepo().UpdateAbstract((int)_article.ID, AbstractBody);
            }
            else
            // Otherwise create new abstract article relationshitp
            {
                new AbstractRepo().AddAbstract((int)_article.ID, AbstractBody);
            }
            _article.AbstractBody = AbstractBody;
            this.Window.Close();
        }
        public void Cancel(object input = null)
        {
            this.Window.Close();
        }
    }
}
