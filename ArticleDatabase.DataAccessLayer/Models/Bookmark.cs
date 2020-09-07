﻿using ArticleDatabase.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleDatabase.DataAccessLayer.Models
{
    public class Bookmark : BaseModel
    {
        /**
         * Public properties
         */
        public int ID { get; set; }
        public string Name { get; set; }
        public int UserID { get; set; }
        public int Global { get; set; }
        public ObservableCollection<Article> Articles { get; set; }
        public int ArticlesCount { get; set; }

        // Blank constructor
        public Bookmark()
        {
            Articles = new ObservableCollection<Article>();
        }

        /**
         * Public methods
         *  - Populate articles
         *  [Populates articles collection from database]
         *  - Get articles count
         *  [Calculate number of articles in bookmark]
         *  - Copy by value
         *  [Copys all property values to another bookmark]
         */
        public void PopulateArticles(User user)
        {
            Articles.Clear();
            foreach (Article article in (new BookmarkRepo()).LoadArticlesForBookmark(user, this))
                Articles.Add(article);
        }
        public void GetArticleCount(User user)
        {
            this.ArticlesCount = (new BookmarkRepo()).CountArticlesInBookmark(this);
        }
        public void CopyByValue(Bookmark bookmark)
        {
            this.ID = bookmark.ID;
            this.Name = bookmark.Name;
            this.UserID = bookmark.UserID;
            this.Global = bookmark.Global;
        }
    }
}
