using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Utils
{
    class UserInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Admin { get; set; }

        public UserInfo(string userName)
        {
            User user = new UserRepo().GetUserByName(userName);
            this.Username = user.Username;
            this.Password = user.Password;
            this.Admin = user.Admin;
        }
    }

    class ArticleInfo
    {
        public long? Id { get; set; }
        public string Title { get; set; }
        public string Authors { get; set; }
        public string Keywords { get; set; }
        public int? Year { get; set; }
        public string FileName { get; set; }
        public string PersonalComment { get; set; }
        public int Sic { get; set; }
    }

    class BookmarkInfo
    {

    }

    class ReferenceInfo
    {

    }


    public class Tracker
    {
        private string syncPath;
        private string userPath;
        private string articlePath;
        private string bookmarkPath;
        private string referencePath;

        public Tracker()
        {
            this.syncPath = Path.Combine(Directory.GetCurrentDirectory(), "Sync");
            this.userPath = Path.Combine(syncPath, "User");
            this.articlePath = Path.Combine(syncPath, "Article");
            this.bookmarkPath = Path.Combine(syncPath, "Bookmark");
            this.referencePath = Path.Combine(syncPath, "Reference");
        }

        // Create necessary files and folders to track changes
        public void init()
        {
            if (!Directory.Exists(syncPath))
            {
                // 1. Create sync folder if doesn't exists
                Directory.CreateDirectory(syncPath);

                // 2. Create json files and files folder

                // 2.1 Users
                Directory.CreateDirectory(userPath);
                File.Create(Path.Combine(userPath, "create.json"));

                // 2.2 Articles
                Directory.CreateDirectory(articlePath);
                createCRUDFiles(articlePath);
                Directory.CreateDirectory(Path.Combine(articlePath, "Files"));

                // 2.3 Bookmarks
                Directory.CreateDirectory(bookmarkPath);
                createCRUDFiles(bookmarkPath);

                // 2.4 References
                Directory.CreateDirectory(referencePath);
                createCRUDFiles(referencePath);
            }
        }

        public void TrackCreate(Article article)
        {
            string info = JsonConvert.SerializeObject(article);
            using (StreamWriter sw = new StreamWriter(Path.Combine(syncPath, "create.json")))
            {
                sw.WriteLine(info);
            }
        }

        public void TrackUpdate()
        {

        }

        public void TrackDelete()
        {

        }

        private void createCRUDFiles(string path)
        {
            File.Create(Path.Combine(path, "create.json"));
            File.Create(Path.Combine(path, "update.json"));
            File.Create(Path.Combine(path, "delete.json"));
        }
    }
}
