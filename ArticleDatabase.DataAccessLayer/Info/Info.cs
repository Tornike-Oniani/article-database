using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;

namespace Lib.DataAccessLayer.Info
{
    public class IInfo
    {
        public string InfoType { get; set; }
    }

    public class ArticleInfo : IInfo
    {
        public int ID;
        public string Title;
        public string Authors;
        public string Keywords;
        public int? Year;
        public string FileName;
        public string PersonalComment;
        public int SIC;
        public List<BookmarkInfo> Bookmarks;
        public List<ReferenceInfo> References;
        public string ChangedFile;

        public ArticleInfo()
        {
            this.InfoType = "ArticleInfo";
        }

        public ArticleInfo(User user, string title, List<Bookmark> bookmarks = null, List<Reference> references = null, string changedFile = null)
        {
            this.InfoType = "ArticleInfo";
            Article article = new ArticleRepo().GetFullArticleWithTitle(user, title);
            this.ID = (int)article.ID;
            this.Title = article.Title;
            this.Authors = article.Authors;
            this.Keywords = article.Keywords;
            this.Year = article.Year;
            this.FileName = article.FileName;
            this.PersonalComment = article.PersonalComment;
            this.SIC = article.SIC;
            this.Bookmarks = new List<BookmarkInfo>();
            this.References = new List<ReferenceInfo>();
            this.ChangedFile = null;

            if (bookmarks != null)
            {
                bookmarks.ForEach((cur) =>
                {
                    BookmarkInfo info = new BookmarkInfo(cur.Name, user);
                    this.Bookmarks.Add(info);
                });
            }

            if (bookmarks != null)
            {
                references.ForEach((cur) =>
                {
                    ReferenceInfo info = new ReferenceInfo(cur.Name);
                    this.References.Add(info);
                });
            }
        }

        public void SetChangedFile(string changedFile)
        {
            this.ChangedFile = changedFile;
        }
    }
    public class BookmarkInfo : IInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Global { get; set; }

        public BookmarkInfo()
        {
            this.InfoType = "BookmarkInfo";
        }

        public BookmarkInfo(string Name, User user)
        {
            this.InfoType = "BookmarkInfo";
            Bookmark bookmark = new BookmarkRepo().GetBookmark(Name, user);
            this.ID = bookmark.ID;
            this.Name = bookmark.Name;
            this.Global = bookmark.Global;
        }
    }
    public class ReferenceInfo : IInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int? ArticleID { get; set; }
        public string Title { get; set; }

        public ReferenceInfo()
        {
            this.InfoType = "ReferenceInfo";
        }

        public ReferenceInfo(string name)
        {
            this.InfoType = "ReferenceInfo";
            Reference reference = new ReferenceRepo().GetReference(name);
            this.ID = reference.ID;
            this.Name = reference.Name;
            this.ArticleID = reference.ArticleID;
            this.Title = new ArticleRepo().GetArticleWithId(ArticleID).Title;
        }
    }
    public class Couple : IInfo
    {
        private string _collectionType;
        private string _actionType;

        // Either Bookmark or Reference
        public string CollectionType
        {
            get { return _collectionType; }
            set
            {
                if (value.ToString() == "Bookmark" || value.ToString() == "Reference")
                    _collectionType = value;
                else
                    throw new ArgumentException("Invalid argument. It has to be Bookmark or Reference");
            }
        }
        // Either Add or Remove
        public string ActionType
        {
            get { return _actionType; }
            set
            {
                if (value.ToString() == "Add" || value.ToString() == "Remove")
                    _actionType = value;
                else
                    throw new ArgumentException("Invalid argument. It has to be Add or Remove");
            }
        }

        public string Title;
        public string Name;

        public Couple()
        {
            this.InfoType = "Couple";
        }

        public Couple(string collectionType, string actionType, string title, string name)
        {
            this.InfoType = "Couple";
            this.CollectionType = collectionType;
            this.ActionType = actionType;
            this.Title = title;
            this.Name = name;
        }
    }
    public class DeleteInfo : IInfo
    {
        private string _objectType;

        // Either Article, Bookmark or Reference
        public string ObjectType
        {
            get { return _objectType; }
            set
            {
                if (value.ToString() == "Article" ||
                    value.ToString() == "Bookmark" ||
                    value.ToString() == "Reference")
                    _objectType = value;
                else
                    throw new ArgumentException("Invalid argument. It has to be Article, Bookmark or Reference");
            }
        }
        public string Name;

        public DeleteInfo()
        {
            this.InfoType = "DeleteInfo";
        }

        public DeleteInfo(string objectType, string name)
        {
            this.InfoType = "DeleteInfo";
            this.ObjectType = objectType;
            this.Name = name;
        }
    }
    public class PersonalInfo : IInfo
    {
        public string Title { get; set; }
        public string PersonalComment { get; set; }
        public int SIC { get; set; }

        public PersonalInfo()
        {
            this.InfoType = "Personal";
        }

        public PersonalInfo(string title, string personalComment, int sic)
        {
            this.Title = title;
            this.PersonalComment = personalComment;
            this.SIC = sic;
        }
    }
    public class PendingInfo : IInfo
    {
        public string Section { get; set; }

        public PendingInfo(string section)
        {
            this.InfoType = "PendingInfo";
            this.Section = section;
        }
    }
}
