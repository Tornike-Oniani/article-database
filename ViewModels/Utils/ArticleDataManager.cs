﻿using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.Main;
using MainLib.ViewModels.Popups;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MainLib.ViewModels.Utils
{
    public class ArticleDataManager : INotifyPropertyChanged
    {
        // Public property fields
        private Article _selectedArticle;

        // Private helpers
        private Shared _services;
        private User _user;

        // Public properties
        public Article SelectedArticle
        {
            get { return _selectedArticle; }
            set { _selectedArticle = value; OnPropertyChanged("SelectedArticle"); }
        }

        // Global commands
        public ICommand LoadArticlesCommand { get; set; }
        // Article commands
        public ICommand OpenFileCommand { get; set; }
        public ICommand DeleteArticleCommand { get; set; }
        public ICommand CopyTitleCommand { get; set; }
        // Dialog commands
        public ICommand OpenAddPersonalEditorCommand { get; set; }
        public ICommand OpenEditorCommand { get; set; }
        public ICommand OpenAbstractEditorCommand { get; set; }
        public ICommand OpenBookmarkManagerCommand { get; set; }
        public ICommand OpenReferenceManagerCommand { get; set; }

        // Constructor
        public ArticleDataManager(ICommand loadArticlesCommand)
        {
            this._services = Shared.GetInstance();
            this._user = _services.User;
            this.LoadArticlesCommand = loadArticlesCommand;

            this.OpenFileCommand = new RelayCommand(OpenFile);
            this.DeleteArticleCommand = new RelayCommand(DeleteArticle, IsArticleSelected);
            this.CopyTitleCommand = new RelayCommand(CopyTitle);
            this.OpenAddPersonalEditorCommand = new RelayCommand(OpenAddPersonalEditor, IsArticleSelected);
            this.OpenEditorCommand = new RelayCommand(OpenEditorDialog, IsArticleSelected);
            this.OpenAbstractEditorCommand = new RelayCommand(OpenAbstractEditor, IsArticleSelected);
            this.OpenBookmarkManagerCommand = new RelayCommand(OpenBookmarkManager, IsArticleSelected);
            this.OpenReferenceManagerCommand = new RelayCommand(OpenReferenceManager, IsArticleSelected);
        }

        // Article command actions
        public void OpenFile(object input)
        {
            // 1. If no item was selected return
            if (SelectedArticle == null && input == null)
                return;

            string full_path = "";
            if (input != null)
            {
                // 2. Get full path of the file
                full_path = Environment.CurrentDirectory + "\\Files\\" + ((Article)input).FileName + ".pdf";
            }
            else
            {
                full_path = Environment.CurrentDirectory + "\\Files\\" + SelectedArticle.FileName + ".pdf";
            }

            // 3. Open file with default program
            try
            {
                Process.Start(full_path);
            }
            // 4. Catch if file doesn't exist physically
            catch
            {
                _services.DialogService.OpenDialog(new DialogOkViewModel("File was not found", "Error", DialogType.Error));
            }
        }
        public void DeleteArticle(object input = null)
        {
            try
            {
                // 1. Ask user if they are sure
                if (_services.DialogService.OpenDialog(
                    new DialogYesNoViewModel("Are you sure you want to delete selected article?", "Warning", DialogType.Question)
                   ))
                {
                    // 2. Delete article record from database
                    new ArticleRepo().DeleteArticle(SelectedArticle);

                    // 2.2 Track article delete
                    new Tracker(_user).TrackDelete("Article", SelectedArticle.Title);

                    // 3. Delete physical .pdf file
                    try
                    {
                        File.Delete(Path.Combine(Environment.CurrentDirectory, "Files\\") + SelectedArticle.FileName + ".pdf");
                    }

                    catch
                    {
                        _services.DialogService.OpenDialog(
                            new DialogOkViewModel("The file is missing, validate your database.", "Error", DialogType.Error));
                    }

                    // 4. Refresh the data grid
                    LoadArticlesCommand.Execute(null);
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("Data View", "Delete Article", e.Message, e.StackTrace);
                _services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
        }
        public void CopyTitle(object input = null)
        {
            Clipboard.SetText(SelectedArticle.Title);
        }
        // Dialog command actions
        public void OpenAddPersonalEditor(object input)
        {
            _services.WindowService.OpenWindow(new MainLib.ViewModels.Popups.AddPersonalDialogViewModel(SelectedArticle));
        }
        public void OpenBookmarkManager(object input = null)
        {
            _services.WindowService.OpenWindow(new BookmarkManagerViewModel(ViewType.DataView, SelectedArticle));
        }
        public void OpenEditorDialog(object input = null)
        {
            _services.WindowService.OpenWindow(new MainLib.ViewModels.Popups.ArticleEditorViewModel(SelectedArticle));
        }
        public void OpenAbstractEditor(object input)
        {
            _services.WindowService.OpenWindow(new AbstractEditorViewModel(SelectedArticle), passWindow: true);
        }
        public void OpenReferenceManager(object input)
        {
            _services.WindowService.OpenWindow(new ReferenceManagerViewModel(ViewType.DataView, SelectedArticle));
        }
        // Command validators
        public bool IsArticleSelected(object input = null)
        {
            if (this == null)
                return false;

            if (SelectedArticle != null)
                return true;

            return false;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
