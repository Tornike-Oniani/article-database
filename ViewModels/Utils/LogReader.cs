﻿using Lib.DataAccessLayer.Info;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace MainLib.ViewModels.Utils
{
    public class LogReader
    {
        private string _filesPath;
        private string _logsPath;
        private string _path;
        private List<Log<IInfo>> _logs;
        private List<string> _mismatches;

        public bool NoErrors;

        public LogReader(string path)
        {
            this._filesPath = path;
            this._logsPath = Path.Combine(Environment.CurrentDirectory, "Logs");
            this.NoErrors = true;
            this._mismatches = new List<string>();
        }

        public void GetLogs(string path)
        {
            string info = File.ReadAllText(Path.Combine(path, "log.json"));
            this._logs = JsonConvert.DeserializeObject<List<Log<IInfo>>>(info, new LogConverter());
        }

        public void Sync()
        {
            // 1. Sync information
            Shared.GetInstance().IsWorking(true, $"Syncing steps 1/{_logs.Count}");
            int currentLogCount = 1;
            _logs.ForEach((log) =>
            {
                if (currentLogCount % 10 == 0)
                    Shared.GetInstance().IsWorking(true, $"Syncing steps {currentLogCount}/{_logs.Count}");
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

                            // Edge case: Article already exists
                            if (repo.GetArticleWithTitle(article.Title) != null)
                            {
                                string mismatch = $"Article '{local_info.Title}' already exists.";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }

                            // 1. Add article to database
                            repo.SaveArticle(article, user);

                            // Get freshly added article
                            Article dbArticle = repo.GetArticleWithTitle(local_info.Title);

                            // Add Abstract
                            if (!String.IsNullOrWhiteSpace(local_info.AbstractBody))
                            {
                                AbstractRepo abs_repo = new AbstractRepo();
                                int articleId = (int)new ArticleRepo().GetArticleWithTitle(local_info.Title).ID;
                                abs_repo.AddAbstract(articleId, local_info.AbstractBody);
                            }                           

                            // 2. Copy file
                            string fileName = repo.GetFileWithTitle(local_info.Title);
                            File.Copy(
                                Path.Combine(_filesPath, local_info.FileName + ".pdf"),
                                Path.Combine(Path.Combine(Environment.CurrentDirectory, "Files"), fileName + ".pdf"));


                            // 3. Add references and bookmarks
                            BookmarkRepo bookmarkRepo = new BookmarkRepo();
                            ReferenceRepo referenceRepo = new ReferenceRepo();

                            // Bookmarks
                            foreach (BookmarkInfo bookmarkInfo in local_info.Bookmarks)
                            {
                                // Get bookmark from database
                                Bookmark dbBookmark = bookmarkRepo.GetBookmark(bookmarkInfo.Name, user);
                                // Edge case: if there is no bookmark in database create one
                                if (dbBookmark == null)
                                {
                                    bookmarkRepo.AddBookmark(bookmarkInfo.Name, bookmarkInfo.Global, user);
                                    dbBookmark = bookmarkRepo.GetBookmark(bookmarkInfo.Name, user);
                                }
                                // Couple article with bookmark
                                bookmarkRepo.AddArticleToBookmark(dbBookmark, dbArticle);
                            }

                            // References
                            foreach (ReferenceInfo referenceInfo in local_info.References)
                            {
                                // Get reference from database
                                Reference dbReference = referenceRepo.GetReference(referenceInfo.Name);

                                // Edge case: if there is no reference in database create one and log mismatch for checking main article
                                if (dbReference == null)
                                {
                                    referenceRepo.AddReference(referenceInfo.Name);
                                    dbReference = referenceRepo.GetReference(referenceInfo.Name);
                                    string mismatch = $"Reference - '{referenceInfo.Name}' was created automatically but main article needs to be set manually.";
                                    _mismatches.Add(mismatch);
                                }

                                // Couple artilce with reference
                                referenceRepo.AddArticleToReference(dbReference, dbArticle);
                            }

                        }
                        // Create bookmark
                        else if (log.Info.InfoType == "BookmarkInfo")
                        {
                            BookmarkInfo local_info = log.Info as BookmarkInfo;
                            BookmarkRepo repo = new BookmarkRepo();

                            // Edge case: Bookmark already exists
                            if (repo.GetBookmark(local_info.Name, user) != null)
                            {
                                string mismatch = $"Bookmark '{local_info.Name}' already exists.";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }

                            repo.AddBookmark(local_info.Name, local_info.Global, user);
                        }
                        // Create reference
                        else if (log.Info.InfoType == "ReferenceInfo")
                        {
                            ReferenceInfo local_info = log.Info as ReferenceInfo;
                            ReferenceRepo repo = new ReferenceRepo();

                            // Edge case: Reference already exists
                            if (repo.GetReference(local_info.Name) != null)
                            {
                                string mismatch = $"Reference '{local_info.Name}' already exists.";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }

                            new ReferenceRepo().AddReference(local_info.Name);
                        }
                        // Create abstract
                        else if (log.Info.InfoType == "AbstractInfo")
                        {
                            AbstractInfo local_info = log.Info as AbstractInfo;
                            AbstractRepo repo = new AbstractRepo();

                            // Edge case: title already has an abstract
                            int articleId = (int)new ArticleRepo().GetArticleWithTitle(local_info.ArticleTitle).ID;
                            if (repo.GetAbstractByArticleId(articleId) != null)
                            {
                                string mismatch = $"Article '{local_info.ArticleTitle}' already has an abstract.";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }

                            repo.AddAbstract(articleId, local_info.AbstractBody);
                        }
                        break;
                    case "Update":
                        // Update article
                        if (log.Info.InfoType == "ArticleInfo")
                        {
                            ArticleInfo local_info = (ArticleInfo)log.Info;
                            ArticleRepo repo = new ArticleRepo();
                            Article existingArticle = repo.GetFullArticleWithTitle(user, log.Changed);

                            // Edge case: Article I am trying to update doesn't exist
                            if (existingArticle == null)
                            {
                                string mismatch = $"Article '{log.Changed}' doesn't exist and can't be updated.";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }

                            Article newArticle = new Article(local_info);
                            newArticle.ID = existingArticle.ID;
                            repo.UpdateArticle(newArticle, user);

                            // If file was changed during update switch the old file with the new one
                            if (!String.IsNullOrEmpty(local_info.ChangedFile))
                            {
                                string fileName = repo.GetFileWithTitle(local_info.Title);
                                File.Copy(
                                    Path.Combine(_filesPath, local_info.ChangedFile),
                                    Path.Combine(Path.Combine(Environment.CurrentDirectory, "Files"), fileName + ".pdf"), true);
                            }
                        }
                        // Update bookmark
                        else if (log.Info.InfoType == "BookmarkInfo")
                        {
                            BookmarkInfo local_info = (BookmarkInfo)log.Info;
                            BookmarkRepo repo = new BookmarkRepo();
                            Bookmark existingBookmark = repo.GetBookmark(log.Changed, user);

                            // Edge case: Bookmark I am trying to update doesn't exist
                            if (existingBookmark == null)
                            {
                                string mismatch = $"Bookmark '{local_info.Name}' doesn't exist and can't be updated.";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }

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

                            // Edge case: Reference I am trying to update doesn't exist
                            if (existingReference == null)
                            {
                                string mismatch = $"Reference '{local_info.Name}' doesn't exist and can't be updated.";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }

                            Reference newReference = new Reference(local_info);
                            newReference.ID = existingReference.ID;
                            bool has = false;
                            if (local_info.Title != null)
                            {
                                newReference.ArticleID = (int)new ArticleRepo().GetArticleWithTitle(local_info.Title).ID;
                                has = true;
                            }

                            repo.UpdateReference(newReference, has);
                        }
                        // Update abstract
                        else if (log.Info.InfoType == "AbstractInfo")
                        {
                            AbstractInfo local_info = log.Info as AbstractInfo;
                            AbstractRepo repo = new AbstractRepo();

                            int articleId = (int)new ArticleRepo().GetArticleWithTitle(local_info.ArticleTitle).ID;
                            Abstract hasAbstract = repo.GetAbstractByArticleId(articleId);
                            // Edge case article whose abstract we are trying to update has no abstract.
                            if (hasAbstract == null || String.IsNullOrEmpty(hasAbstract.Body))
                            {
                                string mismatch = $"Article with id of '{articleId}' has no abstract to update.";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }
                            repo.UpdateAbstractByArticleId(articleId, local_info.AbstractBody);
                        }
                        break;
                    case "Coupling":
                        Couple info = (Couple)log.Info;
                        // Couple bookmark
                        if (info.CollectionType == "Bookmark")
                        {
                            BookmarkRepo bookmarkRepo = new BookmarkRepo();
                            ArticleRepo articleRepo = new ArticleRepo();

                            Bookmark bookmark = bookmarkRepo.GetBookmark(info.Name, user);
                            Article article = articleRepo.GetArticleWithTitle(info.Title);

                            // Edge case: Article or bookmark doesn't exist
                            if (bookmark == null)
                            {
                                // Automatically add bookmark but log mismatch bevause we don't know if its global or nor
                                bookmarkRepo.AddBookmark(info.Name, 0, user);
                                bookmark = bookmarkRepo.GetBookmark(info.Name, user);
                                string mismatch = $"Bookmark - '{info.Name}' was added automatically, but you need to set global status manually.";
                                _mismatches.Add(mismatch);
                            }
                            else if (article == null)
                            {
                                string mismatch = $"Can't couple article - '{info.Title}' with bookmark '{info.Name}' because article doesn't exist";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }

                            // Add
                            if (info.ActionType == "Add")
                            {
                                // Edge case: Article is already in bookmark
                                if (bookmarkRepo.CheckArticleInBookmark(bookmark, article))
                                {
                                    string mismatch = $"Article - '{info.Title}' is already in bookmark '{info.Name}'";
                                    _mismatches.Add(mismatch);
                                    currentLogCount++;
                                    return;
                                }
                                bookmarkRepo.AddArticleToBookmark(bookmark, article);
                            }
                            // Remove
                            else if (info.ActionType == "Remove")
                            {
                                // Edge case: Article is not in bookmark
                                if (!bookmarkRepo.CheckArticleInBookmark(bookmark, article))
                                {
                                    string mismatch = $"Article - '{info.Title}' can not be removed from bookmark '{info.Name}' (Its not there)";
                                    _mismatches.Add(mismatch);
                                    currentLogCount++;
                                    return;
                                }
                                bookmarkRepo.RemoveArticleFromBookmark(bookmark, article);
                            }
                        }
                        // Couple reference
                        else if (info.CollectionType == "Reference")
                        {
                            ReferenceRepo referenceRepo = new ReferenceRepo();
                            ArticleRepo articleRepo = new ArticleRepo();

                            Reference reference = referenceRepo.GetReference(info.Name);
                            Article article = articleRepo.GetArticleWithTitle(info.Title);

                            // Edge case: Article or bookmark doesn't exist
                            if (reference == null)
                            {
                                string mismatch = $"Can't couple article - '{info.Title}' with reference '{info.Name}' because reference doesn't exist";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }
                            else if (article == null)
                            {
                                string mismatch = $"Can't couple article - '{info.Title}' with reference '{info.Name}' because article doesn't exist";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }

                            // Add
                            if (info.ActionType == "Add")
                            {
                                if (referenceRepo.CheckArticleInReference(reference, article))
                                {
                                    string mismatch = $"Article - '{info.Title}' is already in reference '{info.Name}'";
                                    _mismatches.Add(mismatch);
                                    currentLogCount++;
                                    return;
                                }
                                referenceRepo.AddArticleToReference(reference, article);
                            }
                            // Remove
                            else if (info.ActionType == "Remove")
                            {
                                if (!referenceRepo.CheckArticleInReference(reference, article))
                                {
                                    string mismatch = $"Article - '{info.Title}' is already in bookmark '{info.Name}'";
                                    _mismatches.Add(mismatch);
                                    currentLogCount++;
                                    return;
                                }
                                referenceRepo.RemoveArticleFromReference(reference, article);
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

                            // Edge case: Article I am trying to delete doesn't exist
                            if (article == null)
                            {
                                string mismatch = $"Can't delete article '{local_info1.Name}' because it doesn't exist";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }

                            string file = repo.GetFileWithTitle(local_info1.Name);
                            repo.DeleteArticle(article);
                            File.Delete(Path.Combine(Environment.CurrentDirectory, "Files", file + ".pdf"));
                        }
                        // Delete bookmark
                        else if (local_info1.ObjectType == "Bookmark")
                        {
                            BookmarkRepo repo = new BookmarkRepo();
                            Bookmark bookmark = repo.GetBookmark(local_info1.Name, user);

                            // Edge case: Bookmark I am trying to delete doesn't exist
                            if (bookmark == null)
                            {
                                string mismatch = $"Can't delete bookmark '{local_info1.Name}' because it doesn't exist";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }

                            repo.DeleteBookmark(bookmark);
                        }
                        // Delete reference
                        else if (local_info1.ObjectType == "Reference")
                        {
                            ReferenceRepo repo = new ReferenceRepo();
                            Reference reference = repo.GetReference(local_info1.Name);

                            // Edge case: Reference I am trying to delete doesn't exist
                            if (reference == null)
                            {
                                string mismatch = $"Can't delete reference '{local_info1.Name}' because it doesn't exist";
                                _mismatches.Add(mismatch);
                                currentLogCount++;
                                return;
                            }

                            repo.DeleteReference(reference);
                        }
                        break;
                    case "Personal":
                        PersonalInfo local_info2 = (PersonalInfo)log.Info;
                        ArticleRepo repo1 = new ArticleRepo();
                        Article article3 = repo1.GetArticleWithTitle(local_info2.Title);

                        // Edge case: Article I am trying to add personal doesn't exist
                        if (article3 == null)
                        {
                            string mismatch = $"Can't add personal to article '{local_info2.Title}' because it doesn't exist";
                            _mismatches.Add(mismatch);
                            currentLogCount++;
                            return;
                        }

                        article3.PersonalComment = local_info2.PersonalComment;
                        article3.SIC = local_info2.SIC;

                        repo1.UpdatePersonal(article3, user);
                        break;
                    case "Pending":
                        PendingInfo l_info = (PendingInfo)log.Info;

                        // 1. Remove pending section status from db
                        new GlobalRepo().RemovePending(l_info.Section);

                        // 2. Remove section from json
                        string path = Path.Combine(Environment.CurrentDirectory, "sections.json");
                        string textInfo = File.ReadAllText(path);
                        List<string> sections = JsonConvert.DeserializeObject<List<string>>(textInfo);
                        sections.Remove(l_info.Section);
                        textInfo = JsonConvert.SerializeObject(sections);
                        File.WriteAllText(path, textInfo);
                        break;
                    default:
                        break;
                }
                currentLogCount++;
            });

            // 2. Write mismatches
            WriteMismatches();
        }

        private void WriteMismatches()
        {
            // 1. Set error status
            this.NoErrors = !(_mismatches.Count > 0);

            if (_mismatches.Count == 0)
                return;

            // 2. Start writing
            // File properties
            string name = "Sync mismatch --- " + DateTime.Today.ToLongDateString() + ".txt";
            this._path = Path.Combine(_logsPath, name);
            DateTime currentTime = DateTime.Now;

            //[Obsolete, File.Append already creates file if it doesn't exist and this causes another process exception]
            /*
            // Create if it doesn't exist
            if (!File.Exists(_path))
                File.Create(_path);
            */

            // Write mismatch into file
            using (StreamWriter sw = File.AppendText(_path))
            {
                // Start writing log starting with current hour as timestamp
                sw.WriteLine("");
                sw.WriteLine(currentTime.ToLongTimeString() + ":");
                sw.WriteLine("");

                _mismatches.ForEach((mismatch) =>
                {
                    sw.WriteLine(" - " + mismatch);
                });
            }
        }
    }
}
