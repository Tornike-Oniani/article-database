using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Models;
using ArticleDatabase.DataAccessLayer.Repositories;
using ArticleDatabase.UIStructs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleDatabase.UIStructs
{
    public class BookmarkBox : BaseStruct
    {
        // Private members
        private bool _isChecked;

        // Public properties
        public Bookmark Bookmark { get; set; }
        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; OnPropertyChanged("IsChecked"); }
        }

        // Constructor
        public BookmarkBox(Bookmark bookmark, Article article)
        {
            this.Bookmark = bookmark;
        }

        // Public methods
        public void HasArticle(Article article)
        {
            this.IsChecked = (new BookmarkRepo()).CheckArticleInBookmark(Bookmark, article);
        }
    }
}
