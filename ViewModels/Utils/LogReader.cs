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

        public LogReader(string path)
        {
            this._filesPath = path;
        }

        public void GetLogs(string path)
        {
            string info = File.ReadAllText(Path.Combine(path, "log.json"));
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
                        // Create article
                        if (log.Info.InfoType == "ArticleInfo")
                        {
                            ArticleInfo local_info = log.Info as ArticleInfo;
                            Article article = new Article(local_info);
                            ArticleRepo repo = new ArticleRepo();

                            // 1. Add article to database
                            repo.SaveArticle(article, user);

                            // 2. Copy file
                            string fileName = repo.GetFileWithTitle(local_info.Title);
                            File.Copy(
                                Path.Combine(_filesPath, local_info.FileName + ".pdf"), 
                                Path.Combine(Path.Combine(Environment.CurrentDirectory, "Files"), fileName + ".pdf"));

                            Article dbArticle = repo.GetArticleWithTitle(local_info.Title);

                            // 3. Add references and bookmarks
                            BookmarkRepo bookmarkRepo = new BookmarkRepo();
                            ReferenceRepo referenceRepo = new ReferenceRepo();

                            // Bookmarks
                            local_info.Bookmarks.ForEach((bookmark) =>
                            {
                                Bookmark dbBookmark = bookmarkRepo.GetBookmark(bookmark.Name, user);
                                bookmarkRepo.AddArticleToBookmark(dbBookmark, dbArticle);
                            });

                            // References
                            local_info.References.ForEach((reference) =>
                            {
                                Reference dbReference = referenceRepo.GetReference(reference.Name);
                                referenceRepo.AddArticleToReference(dbReference, dbArticle);
                            });
                        }
                        // Create bookmark
                        else if (log.Info.InfoType == "BookmarkInfo")
                        {
                            BookmarkInfo local_info = log.Info as BookmarkInfo;
                            new BookmarkRepo().AddBookmark(local_info.Name, local_info.Global, user);
                        }
                        // Create reference
                        else if (log.Info.InfoType == "ReferenceInfo")
                        {
                            ReferenceInfo local_info = log.Info as ReferenceInfo;
                            new ReferenceRepo().AddReference(local_info.Name);
                        }
                        break;
                    case "Update":
                        // Update article
                        if (log.Info.InfoType == "ArticleInfo")
                        {
                            ArticleInfo local_info = (ArticleInfo)log.Info;
                            ArticleRepo repo = new ArticleRepo();
                            Article existingArticle = repo.GetFullArticleWithTitle(user, log.Changed);

                            Article newArticle = new Article(local_info);
                            newArticle.ID = existingArticle.ID;
                            repo.UpdateArticle(newArticle, user);
                        }
                        // Update bookmark
                        else if(log.Info.InfoType == "BookmarkInfo")
                        {
                            BookmarkInfo local_info = (BookmarkInfo)log.Info;
                            BookmarkRepo repo = new BookmarkRepo();
                            Bookmark existingBookmark = repo.GetBookmark(log.Changed, user);

                            Bookmark newBookmark = new Bookmark(local_info);
                            newBookmark.ID = existingBookmark.ID;
                            repo.UpdateBookmark(newBookmark);
                        }
                        // Update reference
                        else if (log.Info.InfoType == "ReferenceInfo")
                        {
                            ReferenceInfo local_info = (ReferenceInfo)log.Info;
                            ReferenceRepo repo = new ReferenceRepo();
                            Reference existingReference = repo.GetReference(log.Changed);

                            Reference newReference = new Reference(local_info);
                            newReference.ID = existingReference.ID;
                            if (local_info.Title != null)
                                newReference.ArticleID = (int)new ArticleRepo().GetArticleWithTitle(local_info.Title).ID;
                            repo.UpdateReference(newReference);
                        }
                        break;
                    case "Coupling":
                        Couple info = (Couple)log.Info;
                        // Couple bookmark
                        if (info.CollectionType == "Bookmark")
                        {
                            BookmarkRepo repo = new BookmarkRepo();
                            // Add
                            if (info.ActionType == "Add")
                            {
                                repo.AddArticleToBookmark(repo.GetBookmark(info.Name, user), new ArticleRepo().GetArticleWithTitle(info.Title));
                            }
                            // Remove
                            else if (info.ActionType == "Remove")
                            {
                                repo.RemoveArticleFromBookmark(repo.GetBookmark(info.Name, user), new ArticleRepo().GetArticleWithTitle(info.Title));
                            }
                        }
                        // Couple reference
                        else if(info.CollectionType == "Reference")
                        {
                            ReferenceRepo repo = new ReferenceRepo();
                            // Add
                            if (info.ActionType == "Add")
                            {
                                repo.AddArticleToReference(repo.GetReference(info.Name), new ArticleRepo().GetArticleWithTitle(info.Title));
                            }
                            // Remove
                            else if (info.ActionType == "Remove")
                            {
                                repo.RemoveArticleFromReference(repo.GetReference(info.Name), new ArticleRepo().GetArticleWithTitle(info.Title));
                            }
                        }
                        break;
                    case "Delete":
                        DeleteInfo local_info1 = (DeleteInfo)log.Info;
                        // Delete article
                        if (local_info1.ObjectType == "Article")
                        {
                            ArticleRepo repo = new ArticleRepo();
                            Article article = repo.GetArticleWithTitle(local_info1.Name);
                            string file = repo.GetFileWithTitle(local_info1.Name);
                            repo.DeleteArticle(article);
                            File.Delete(Path.Combine(Environment.CurrentDirectory, "Files", file + ".pdf"));
                        }
                        // Delete bookmark
                        else if (local_info1.ObjectType == "Bookmark")
                        {
                            BookmarkRepo repo = new BookmarkRepo();
                            Bookmark bookmark = repo.GetBookmark(local_info1.Name, user);
                            repo.DeleteBookmark(bookmark);
                        }
                        // Delete reference
                        else if (local_info1.ObjectType == "Reference")
                        {
                            ReferenceRepo repo = new ReferenceRepo();
                            Reference reference = repo.GetReference(local_info1.Name);
                            repo.DeleteReference(reference);
                        }
                        break;
                    case "Personal":
                        PersonalInfo local_info2 = (PersonalInfo)log.Info;
                        ArticleRepo repo1 = new ArticleRepo();
                        Article article3 = repo1.GetArticleWithTitle(local_info2.Title);
                        article3.PersonalComment = local_info2.PersonalComment;
                        article3.SIC = local_info2.SIC;

                        repo1.UpdatePersonal(article3, user);
                        break;
                    default:
                        break;
                }
            });
        }
    }
}
