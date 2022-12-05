using Lib.DataAccessLayer.Info;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.UIStructs;
using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainLib.ViewModels.Main
{
    public class AbstractsEntryViewModel : BaseViewModel
    {
        // Private attributes
        private string _sortDirection;
        private bool isSortAscending;

        // Public proeprties
        public ObservableCollection<AbstractEntryItem> Abstracts { get; set; }
        public string SortDirection
        {
            get { return _sortDirection; }
            set { _sortDirection = value; OnPropertyChanged("SortDirection"); }
        }

        // Commands
        public ICommand LoadAbstractsCommand { get; set; }
        public ICommand OpenArticleFileCommand { get; set; }
        public ICommand AddAbstractCommand { get; set; }
        public ICommand ChangeSortDirectionCommand { get; set; }

        // Constructor
        public AbstractsEntryViewModel()
        {
            // Init
            Abstracts = new ObservableCollection<AbstractEntryItem>();
            isSortAscending = true;
            SortDirection = "File name ascending";

            // Set up commands
            LoadAbstractsCommand = new RelayCommand(LoadAbstracts);
            OpenArticleFileCommand = new RelayCommand(OpenArticleFile);
            AddAbstractCommand = new RelayCommand(AddAbstract);
            ChangeSortDirectionCommand = new RelayCommand(ChangeSortDirection);

            // Load abstracts
            LoadAbstractsCommand.Execute(null);
        }

        // Command actions
        public async void LoadAbstracts(object input = null)
        {
            await PopulateAbstracts();
        }
        public async void OpenArticleFile(object input)
        {
            AbstractEntryItem selectedAbstract = input as AbstractEntryItem;
            Shared services = Shared.GetInstance();

            // 1. Get article file name
            string fileName = "";
            services.IsWorking(true, "Searching file...");
            await Task.Run(() =>
            {
                fileName = new ArticleRepo().GetArticleWithTitle(selectedAbstract.Title).FileName;
            });
            services.IsWorking(false);

            string full_path = Path.Combine(Environment.CurrentDirectory, "Files", fileName + ".pdf");
            // 2. Open file with default program
            try
            {
                Process.Start(full_path);
            }
            // 3. Catch if file doesn't exist physically
            catch
            {
                services.DialogService.OpenDialog(new DialogOkViewModel("File was not found", "Error", DialogType.Error));
            }
        }
        public async void AddAbstract(object input)
        {
            AbstractEntryItem currentAbstract = input as AbstractEntryItem;

            // Check for unusual characters
            Regex unusualCharacters = new Regex("^[A-Za-z0-9 .,'()+/_?:\"\\&*%$#@<>{}!=;-]+$");
            if (!unusualCharacters.IsMatch(currentAbstract.Body))
            {
                Shared.GetInstance().DialogService.OpenDialog(new DialogOkViewModel("This input contains unusual characters, please retype it manually. (Don't copy & paste!)", "Error", DialogType.Error));
                currentAbstract.Body = null;
                return;
            }

            Shared.GetInstance().IsWorking(true, "Adding abstract...");
            await Task.Run(() =>
            {
                // Get article id
                Article currentArticle = new ArticleRepo().GetArticleWithTitle(currentAbstract.Title);
                // Create abstract
                new AbstractRepo().AddAbstract((int)currentArticle.ID, currentAbstract.Body);
                // Track abstract creation
                new Tracker(Shared.GetInstance().User).TrackAbstract(new AbstractInfo() { ArticleTitle = currentAbstract.Title, AbstractBody = currentAbstract.Body });
            });
            // Remove from current list (as we only need artilces with no abstract)
            Shared.GetInstance().IsWorking(false);

            await PopulateAbstracts();
        }
        public async void ChangeSortDirection(object input = null)
        {
            isSortAscending = !isSortAscending;
            SortDirection = $"File name {(isSortAscending ? "ascending" : "descending")}";
            await PopulateAbstracts();
        }

        // Private helper functions
        private async Task PopulateAbstracts()
        {
            Abstracts.Clear();
            List<Abstract> _abstractList = new List<Abstract>();
            Shared.GetInstance().IsWorking(true, "Fetching abstracts");
            await Task.Run(() =>
            {
                _abstractList = new AbstractRepo().GetArticleTitlesWithNoAbstracts(isSortAscending ? "ASC" : "DESC");
            });
            foreach (AbstractEntryItem a in _abstractList.Select(a => new AbstractEntryItem() { Title = a.ArticleTitle, FileName = a.ArticleFileName, Body = a.Body }))
            {
                Abstracts.Add(a);
            }
            Shared.GetInstance().IsWorking(false);
        }
    }
}
