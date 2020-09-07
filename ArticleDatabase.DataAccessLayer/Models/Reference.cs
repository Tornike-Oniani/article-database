using ArticleDatabase.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleDatabase.DataAccessLayer.Models
{
    public class Reference : BaseModel
    {
        /** Public properties
         */
        public int ID { get; set; }
        public string Name { get; set; }
        public int? ArticleID { get; set; }
        public ObservableCollection<Article> Articles { get; set; }
        public Article Article { get; set; }

        // Constructor
        public Reference()
        {
            Articles = new ObservableCollection<Article>();
        }

        /** Public methods:
         *  - Populate articles
         *  [Populates articles collection from database]
         *  - Set main article
         *  [Fetch article from database with ArticleID]
         *  - Copy by value
         *  [Copys all property values to another bookmark]
         */
        public void PopulateArticles()
        {
            Articles.Clear();
            foreach (Article article in (new ReferenceRepo()).LoadArticlesForReference(this))
                Articles.Add(article);
        }
        public void SetMainArticle()
        {
            this.Article = (new ArticleRepo()).GetArticleWithId(ArticleID);
        }
        public void CopyByValue(Reference reference)
        {
            this.ID = reference.ID;
            this.Name = reference.Name;
            this.ArticleID = reference.ArticleID;
        }
    }
}
