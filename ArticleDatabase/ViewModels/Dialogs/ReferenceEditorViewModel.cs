﻿using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.ViewModels.Base;
using ArticleDatabase.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArticleDatabase.ViewModels.Dialogs
{
    public class ReferenceEditorViewModel : BaseWindow
    {
        // Private members
        private ReferenceListViewModel _parent;

        // Public properties
        public Reference Reference { get; set; }
        public string MainArticleTitle { get; set; }

        // Commands
        public RelayCommand SaveReferenceCommand { get; set; }

        // Constructor
        public ReferenceEditorViewModel(Reference reference, ReferenceListViewModel parent, Window window) : base(window)
        {
            this.Reference = new Reference();
            this.Reference.CopyByValue(reference);
            this._parent = parent;
            if (Reference.ArticleID != 0 && Reference.ArticleID != null)
                this.MainArticleTitle = SqliteDataAccess.GetArticleWithId(Reference.ArticleID).Title;

            SaveReferenceCommand = new RelayCommand(SaveReference, CanSaveReference);
        }

        // Command actions
        public void SaveReference(object input = null)
        {
            Reference.ArticleID = SqliteDataAccess.CheckArticleWithTitle(MainArticleTitle);

            SqliteDataAccess.UpdateReference(Reference);

            _parent.PopulateReferences();
        }
        public bool CanSaveReference(object input = null)
        {
            return !String.IsNullOrWhiteSpace(Reference.Name) && SqliteDataAccess.CheckArticleWithTitle(MainArticleTitle) != 0;
        }
    }
}
