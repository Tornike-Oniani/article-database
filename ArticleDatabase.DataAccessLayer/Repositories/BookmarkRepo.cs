using Dapper;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories.Base;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace Lib.DataAccessLayer.Repositories
{
    public class BookmarkRepo : BaseRepo
    {
        // Add new bookmark to database
        public bool AddBookmark(string name, int global, User user)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // 1. Check for duplicates (Name AND UserID has to be unique)
                    int check = conn.QuerySingleOrDefault<int>("SELECT COUNT(ID) FROM tblBookmark WHERE lower(Name) = lower(@Name) AND UserID = @UserID;",
                        new { Name = name, UserID = user.ID }, transaction: transaction);

                    if (check > 0)
                        return false;

                    // 2. Add bookmark to database
                    conn.Execute("INSERT INTO tblBookmark (Name, UserID, Global) VALUES (@Name, @UserID, @Global);",
                        new { Name = name, UserID = user.ID, Global = global });

                    transaction.Commit();
                }
            }

            return true;
        }
        // Edit bookmark
        public void UpdateBookmark(Bookmark bookmark)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Edit bookmark values in database
                    conn.Execute("UPDATE tblBookmark SET Name=@Name, Global=@Global WHERE ID=@ID;",
                        new { Name = bookmark.Name, Global = bookmark.Global, ID = bookmark.ID }, transaction: transaction);

                    transaction.Commit();
                }
            }
        }
        // Delete bookmark from database
        public void DeleteBookmark(Bookmark bookmark)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // 1. Delete article relationships 
                    conn.Execute("DELETE FROM tblBookmarkArticles WHERE BookmarkID=@BookmarkID;",
                        new { BookmarkID = bookmark.ID }, transaction: transaction);

                    // 2. Delete bookmark from database
                    conn.Execute("DELETE FROM tblBookmark WHERE ID=@ID;",
                        new { ID = bookmark.ID }, transaction: transaction);

                    transaction.Commit();
                }
            }
        }
        // Get bookmark by name
        public Bookmark GetBookmark(string name, User user)
        {
            Bookmark result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.QuerySingleOrDefault<Bookmark>($"SELECT ID, Name, Global FROM tblBookmark WHERE UserID = @UserID AND Name = @Name;",
                        new { UserID = user.ID, Name = name }, transaction: transaction);

                    transaction.Commit();
                }
            }

            return result;
        }
        // Get bookmark name from Id (Used in update tracking)
        public string GetBookmarkNameWithId(int id)
        {
            string result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.QuerySingleOrDefault<string>($"SELECT Name FROM tblBookmark WHERE ID=@ID;",
                        new { ID = id }, transaction: transaction);

                    transaction.Commit();
                }
            }

            return result;
        }
        // Fetch bookmarks of the user
        public List<Bookmark> LoadBookmarks(User user, bool global = false)
        {
            List<Bookmark> result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    if (global)
                    {
                        result = conn.Query<Bookmark>($"SELECT ID, Name, Global FROM tblBookmark WHERE UserID = @UserID ORDER BY Name;",
                            new { UserID = user.ID }, transaction: transaction).ToList();
                    }
                    else
                    {
                        result = conn.Query<Bookmark>($"SELECT ID, Name, Global FROM tblBookmark WHERE UserID = @UserID AND Global = 0 ORDER BY Name;",
                            new { UserID = user.ID }, transaction: transaction).ToList();
                    }
                    transaction.Commit();
                }
            }

            return result;
        }
        // Fetch global bookmarks
        public List<Bookmark> LoadGlobalBookmarks()
        {
            List<Bookmark> result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.Query<Bookmark>($"SELECT ID, Name, Global FROM tblBookmark WHERE Global = 1 ORDER BY Name;", transaction: transaction).ToList();
                    transaction.Commit();
                }
            }

            return result;
        }
        // Add article to bookmark
        public void AddArticleToBookmark(Bookmark bookmark, Article article)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Execute("INSERT INTO tblBookmarkArticles (BookmarkID, ArticleID) VALUES (@BookmarkID, @ArticleID);",
                        new { BookmarkID = bookmark.ID, ArticleID = article.ID });

                    transaction.Commit();
                }
            }
        }
        // Remove article from bookmark
        public void RemoveArticleFromBookmark(Bookmark bookmark, Article article)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Execute("DELETE FROM tblBookmarkArticles WHERE BookmarkID = @BookmarkID AND ArticleID = @ArticleID;",
                        new { BookmarkID = bookmark.ID, ArticleID = article.ID });

                    transaction.Commit();
                }
            }
        }
        // Check if article is in bookmark
        public bool CheckArticleInBookmark(Bookmark bookmark, Article article)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    int check = conn.QuerySingleOrDefault<int>("SELECT COUNT(ID) FROM tblBookmarkArticles WHERE BookmarkID = @BookmarkID AND ArticleID = @ArticleID;",
                        new { BookmarkID = bookmark.ID, ArticleID = article.ID });

                    if (check > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.Commit();
                        return false;
                    }
                }
            }
        }
        // Load articles for bookmark
        public List<Article> LoadArticlesForBookmark(User user, Bookmark bookmark)
        {
            List<Article> results;

            string query = $@"
SELECT final.ID, final.Title, final.Authors, final.Keywords, final.Year, final.FileName, final.PersonalComment, final.SIC, abst.Body AS [AbstractBody], ba.ID AS AddedID
FROM
(SELECT cmp.ID, cmp.Article AS Title, cmp.Authors, cmp.Keywords, cmp.Year, cmp.FileName AS [FileName], per.PersonalComment, IFNULL(per.SIC, 0) AS SIC
FROM
(SELECT art_ath.ID, art_ath.Article, art_ath.Authors, art_kwd.Keywords, art_ath.Year, art_ath.FileName
FROM
(SELECT art.ID as ID, art.Title AS Article, group_concat(ath.Name, "", "") AS Authors, art.Year AS Year, art.File AS FileName
FROM tblArticle AS art
LEFT JOIN jntArticleAuthor AS aa ON art.ID = aa.Article_ID
LEFT JOIN tblAuthor AS ath ON aa.Author_ID = ath.ID
GROUP BY art.Title) AS art_ath
JOIN
(SELECT art.Title AS Article, group_concat(kwd.Keyword, "", "") AS Keywords
FROM tblArticle AS art
LEFT JOIN jntArticleKeyword AS ak ON art.ID = ak.Article_ID
LEFT JOIN tblKeyword AS kwd ON ak.Keyword_ID = kwd.ID
GROUP BY art.Title) AS art_kwd
ON art_ath.Article = art_kwd.Article) AS cmp
LEFT JOIN
(SELECT ArticleID, PersonalComment, SIC
FROM tblUserPersonal WHERE UserID = {user.ID}) AS per ON cmp.ID = per.ArticleID) AS final
JOIN tblBookmarkArticles as ba ON final.ID = ba.ArticleID
JOIN tblBookmark as b ON b.ID = ba.BookmarkID 
LEFT JOIN tblAbstract AS abst On abst.Article_ID = final.ID
WHERE b.ID = {bookmark.ID}
ORDER BY AddedID; 
";
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    results = conn.Query<Article>(query, new { BookmarkID = bookmark.ID }, transaction: transaction).ToList();

                    transaction.Commit();
                }
            }

            return results;
        }
        // Count articls in bookmark
        public int CountArticlesInBookmark(Bookmark bookmark)
        {
            int result;

            string query = $@"SELECT COUNT(ID) FROM user.tblBookmarkArticles WHERE BookmarkID=@BookmarkID;";
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    result = conn.QuerySingleOrDefault<int>(query, new { BookmarkID = bookmark.ID }, transaction: transaction);

                    transaction.Commit();
                }
            }

            return result;
        }
        // Add list of articles in bookmark
        public void AddListOfArticlesToBookmark(Bookmark bookmark, List<Article> articles)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    foreach (Article article in articles)
                    {
                        conn.Execute("INSERT INTO tblBookmarkArticles (BookmarkID, ArticleID) VALUES (@BookmarkID, @ArticleID);",
                            new { BookmarkID = bookmark.ID, ArticleID = article.ID });
                    }

                    transaction.Commit();
                }
            }
        }
    }
}
