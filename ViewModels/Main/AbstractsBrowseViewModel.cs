using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.UIStructs;
using MainLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainLib.ViewModels.Main
{
    public class AbstractsBrowseViewModel : BaseViewModel
    {
        // Private attributed
        private List<AbstractBrowseItem> _abstracts;
        private string _searchText;

        // Public properties
        public List<AbstractBrowseItem> Abstracts
        {
            get { return _abstracts; }
            set { _abstracts = value; OnPropertyChanged("Abstracts"); }
        }
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; OnPropertyChanged("SearchText"); }
        }


        // Commands
        public ICommand SearchAbstractsCommand { get; set; }
        public ICommand OpenArticleFileCommand { get; set; }
        public ICommand ChangeAbstractToEditModeCommand { get; set; }
        public ICommand ChangeAbstractToDisplayModeCommand { get; set; }
        public ICommand UpdateAbstractCommand { get; set; }

        public AbstractsBrowseViewModel()
        {
            SearchAbstractsCommand = new RelayCommand(SearchAbstracts);
            OpenArticleFileCommand = new RelayCommand(OpenArticleFile);
            ChangeAbstractToEditModeCommand = new RelayCommand(ChangeAbstractToEditMode);
            ChangeAbstractToDisplayModeCommand = new RelayCommand(ChangeAbstractToDisplayMode);
            UpdateAbstractCommand = new RelayCommand(UpdateAbstract);
        }

        // Command actions
        public async void SearchAbstracts(object input = null)
        {
            await PopulateAbstracts();
        }
        public async void OpenArticleFile(object input)
        {
            AbstractBrowseItem selectedAbstract = input as AbstractBrowseItem;
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
        public void ChangeAbstractToEditMode(object input)
        {
            (input as AbstractBrowseItem).IsInEditMode = true;
        }
        public void ChangeAbstractToDisplayMode(object input)
        {
            (input as AbstractBrowseItem).IsInEditMode = false;
        }
        public async void UpdateAbstract(object input)
        {
            AbstractBrowseItem selectedAbstract = input as AbstractBrowseItem;
            Shared services = Shared.GetInstance();

            services.IsWorking(true, "Updating abstract...");
            await Task.Run(() =>
            {
                // If abstract body was deleted (null or white space) delete it from database
                // We do this because we keep abstract in separate table as one to one relationship with Articles
                if (String.IsNullOrWhiteSpace(selectedAbstract.Body))
                {
                    // Delete abstract from databse
                    new AbstractRepo().DeleteAbstract(selectedAbstract.Id);
                    // Remove from UI list
                    Abstracts.Remove(selectedAbstract);
                    OnPropertyChanged("Abstracts");
                    return;
                }

                // Otherwise update abstract body
                new AbstractRepo().UpdateAbstract(selectedAbstract.Id, selectedAbstract.Body);
            });
            services.IsWorking(false);

            // Return to display mode
            ChangeAbstractToDisplayModeCommand.Execute(selectedAbstract);
        }

        // Private helper functions
        private async Task PopulateAbstracts()
        {
            Shared.GetInstance().IsWorking(true, "Fetching abstracts");
            await Task.Run(() =>
            {
                Abstracts = new AbstractRepo().GetAllAbstracts().Select(a => new AbstractBrowseItem() { Id = a.Id, Title = a.ArticleTitle, Body = a.Body, Match = 55 }).ToList();
            });
            OnPropertyChanged("Abstracts");
            Shared.GetInstance().IsWorking(false);
        }
    }
}
