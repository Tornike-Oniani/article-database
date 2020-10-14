using Lib.DataAccessLayer.Info;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.DataAccessLayer.Models
{
    public class Reference : BaseModel
    {
        /** Public properties
         */
        public int ID { get; set; }
        public string Name { get; set; }
        public int? ArticleID { get; set; }
        public Article Article { get; set; }
        public int ArticlesCount { get; set; }

        // Constructor
        public Reference() { }

        public Reference(ReferenceInfo info)
        {
            this.ID = info.ID;
            this.Name = info.Name;
            this.ArticleID = info.ArticleID;
        }

        /** Public methods:
         *  - Populate articles
         *  [Populates articles collection from database]
         *  - Set main article
         *  [Fetch article from database with ArticleID]
         *  - Copy by value
         *  [Copys all property values to another bookmark]
         */
        public void GetArticleCount()
        {
            this.ArticlesCount = new ReferenceRepo().CountArticlesInReference(this);
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
