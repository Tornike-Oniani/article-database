using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Utils
{
    interface ILog { }

    public class ArticleInfo : ILog
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

        public ArticleInfo(User user, string title, List<Bookmark> bookmarks = null, List<Reference> references = null)
        {
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
    }

    public class BookmarkInfo : ILog
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Global { get; set; }

        public BookmarkInfo(string Name, User user)
        {
            Bookmark bookmark = new BookmarkRepo().GetBookmark(Name, user);
            this.ID = bookmark.ID;
            this.Name = bookmark.Name;
            this.Global = bookmark.Global;
        }
    }

    public class ReferenceInfo : ILog
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int? ArticleID { get; set; }
        public string Title { get; set; }

        public ReferenceInfo(string name)
        {
            Reference reference = new ReferenceRepo().GetReference(name);
            this.ID = reference.ID;
            this.Name = reference.Name;
            this.ArticleID = reference.ArticleID;
            this.Title = new ArticleRepo().GetArticleWithId(ArticleID).Title;
        }
    }

    public class Couple : ILog
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

        public Couple(string collectionType, string actionType, string title, string name)
        {
            this.CollectionType = collectionType;
            this.ActionType = actionType;
            this.Title = title;
            this.Name = name;
        }
    }

    public class DeleteInfo : ILog
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

        public DeleteInfo(string objectType, string name)
        {
            this.ObjectType = objectType;
            this.Name = name;
        }
    }

    public class Log<T>
    {
        private string _type;

        // [Create, Update, Delete, Coupling]
        public string Type
        {
            get { return _type; }
            set 
            {
                if (value.ToString() == "Create" 
                    || value.ToString() == "Update" 
                    || value.ToString() == "Delete"
                    || value.ToString() == "Coupling")
                    _type = value;
                else
                    throw new ArgumentException("Invalid argument. It has to be Create, Update, Delete or Coupling");
            }
        }
        public string Username { get; set; }
        public string Changed { get; set; }
        public T Info { get; set; }

        public Log(string type, string username, T info, string changed = null)
        {
            this.Type = type;
            this.Username = username;
            this.Info = info;
            this.Changed = changed;
        }
    }

    public class Tracker
    {
        private User _user;
        private string syncPath;
        private string logPath;

        public Tracker(User user)
        {
            this._user = user;
            syncPath = Path.Combine(Environment.CurrentDirectory, "Sync");
            logPath = Path.Combine(syncPath, "log.json");
        }

        public string GetFilesPath()
        {
            return Path.Combine(syncPath, "Files");
        }

        // Create necessary files and folders for tracking
        public void init()
        {
            // 1. Create sync directory if it doesn't exist
            if (!Directory.Exists(syncPath))
            {
                Directory.CreateDirectory(syncPath);
                Directory.CreateDirectory(Path.Combine(syncPath, "Files"));
                File.Create(logPath);
            }
        }

        // Track creation of Article, Bookmark or Reference (TODO: User)
        public void TrackCreate<T>(T instance)
        {
            Track<T>("Create", instance);
        }

        public void TrackUpdate<T>(T instance, string id)
        {
            Track<T>("Update", instance, id);
        }

        public void TrackCoupling<T>(T instance)
        {
            Track<T>("Coupling", instance);
        }

        public void TrackDelete(string objectType, string name)
        {
            DeleteInfo info = new DeleteInfo(objectType, name);
            Track<DeleteInfo>("Delete", info);
        }

        private void Track<T>(string action, T instance, string id = null)
        {
            Log<T> log = new Log<T>(action, _user.Username, instance, id);
            string info = JsonConvert.SerializeObject(log);
            using (StreamWriter sw = new StreamWriter(logPath, true))
            {
                sw.WriteLine(info + ",");
            }
        }
    }
}
