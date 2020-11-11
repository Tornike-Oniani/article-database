using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SectionBrowser.ViewModels
{
    public class BrowserViewModel : BaseViewModel
    {
        private string _sectionTitle;
        private List<string> _columns;
        private IDialogService _dialogService;
        private IBrowserService _browserService;

        public string SectionTitle
        {
            get { return _sectionTitle; }
            set { _sectionTitle = value; OnPropertyChanged("SectionTitle"); }
        }
        public List<string> Columns
        {
            get { return _columns; }
            set { _columns = value; OnPropertyChanged("Columns"); }
        }
        public ObservableCollection<Article> Articles { get; set; }

        public ICommand SelectSectionCommand { get; set; }

        public BrowserViewModel()
        {
            this.Title = "Section Browser";
            this.Columns = new List<string>()
            {
                "Authors",
                "Keywords",
                "Year",
                "PersonalComment",
                "SIC"
            };
            this.Articles = new ObservableCollection<Article>();

            this.SelectSectionCommand = new RelayCommand(SelectSection);
        }

        public void SelectSection(object input = null)
        {
            string destination = null;

            destination = _browserService.OpenFolderDialog();

            if (destination == null)
                return;

            // Check if destination is valid
            if (!File.Exists(Path.Combine(destination, "NikasDB.sqlite3")) || 
                !File.Exists(Path.Combine(destination, "user.sqlite3")))
            {
                _dialogService.OpenDialog(new DialogOkViewModel("Please select a valid section", "Select section", DialogType.Error));
                return;
            }


        }
    }
}
