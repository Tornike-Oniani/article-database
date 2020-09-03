﻿using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.UIStructs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleDatabase.UIStructs
{
    public class ReferenceBox : BaseStruct
    {
        // Private members
        private bool _isChecked;
        // I don't like having article in each BookmarkBox I might try to pass this to command via parameter from xaml (with ancestor or something like that)
        private Article _article;

        // Public properties
        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; OnPropertyChanged("IsChecked"); }
        }
        public Reference Reference { get; set; }

        public RelayCommand CheckChangedCommand { get; set; }

        // Constructor
        public ReferenceBox(Reference reference, Article article)
        {
            this.Reference = reference;
            IsChecked = HasArticle(article);
            this._article = article;

            CheckChangedCommand = new RelayCommand(CheckChanged);
        }

        // Public methods
        public bool HasArticle(Article article)
        {
            //return Bookmark.Articles.Contains(article);
            return SqliteDataAccess.CheckArticleInReference(Reference, article);
        }

        public void CheckChanged(object input = null)
        {
            // 1. If user checked add article to bookmark
            if (IsChecked)
            {
                SqliteDataAccess.AddArticleToReference(Reference, _article);
            }
            // 2. If user unchecked remove article from bookmark
            else
            {
                SqliteDataAccess.RemoveArticleFromReference(Reference, _article);
            }
        }
    }
}
