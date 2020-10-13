using Lib.DataAccessLayer.Info;
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
    public class LogReader
    {
        private string _filesPath;
        private List<Log<IInfo>> _logs;

        public LogReader()
        {
            this._filesPath = Path.Combine(Environment.CurrentDirectory, "Sync", "Files");
        }

        public void GetLogs(string path)
        {
            string info = File.ReadAllText(path);
            this._logs = JsonConvert.DeserializeObject<List<Log<IInfo>>>(info, new LogConverter());
        }

        public void Sync()
        {
            _logs.ForEach((log) =>
            {
                User user = new UserRepo().GetUserByName(log.Username);

                switch (log.Type)
                {
                    case "Create":
                        if (log.Info.InfoType == "ArticleInfo")
                        {
                            ArticleInfo info = log.Info as ArticleInfo;
                            Article article = new Article(info);
                            ArticleRepo repo = new ArticleRepo();

                            // 1. Add article to database
                            repo.SaveArticle(article, user);

                            // 2. Copy file
                            string fileName = repo.GetFileWithTitle(info.Title);
                            File.Copy(
                                Path.Combine(_filesPath, info.FileName + ".pdf"), 
                                Path.Combine(Path.Combine(Environment.CurrentDirectory, "Files"), fileName + ".pdf"));

                            Article dbArticle = repo.GetArticleWithTitle(info.Title);

                            // 3. Add references and bookmarks
                            BookmarkRepo bookmarkRepo = new BookmarkRepo();
                            ReferenceRepo referenceRepo = new ReferenceRepo();

                            // Bookmarks
                            info.Bookmarks.ForEach((bookmark) =>
                            {
                                Bookmark dbBookmark = bookmarkRepo.GetBookmark(bookmark.Name, user);
                                bookmarkRepo.AddArticleToBookmark(dbBookmark, dbArticle);
                            });

                            // References
                            info.References.ForEach((reference) =>
                            {
                                Reference dbReference = referenceRepo.GetReference(reference.Name);
                                referenceRepo.AddArticleToReference(dbReference, dbArticle);
                            });
                        }
                        else if (log.Info.InfoType == "BookmarkInfo")
                        {
                            BookmarkInfo info = log.Info as BookmarkInfo;
                            new BookmarkRepo().AddBookmark(info.Name, info.Global, user);
                        }
                        else if (log.Info.InfoType == "ReferenceInfo")
                        {
                            ReferenceInfo info = log.Info as ReferenceInfo;
                            new ReferenceRepo().AddReference(info.Name);
                        }
                        else
                        {
                            Console.WriteLine("Garbage code");
                        }
                        break;
                    case "Update":
                        break;
                    case "Coupling":
                        break;
                    case "Delete":
                        break;
                    default:
                        break;
                }
            });
        }
    }
}
