using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;
using System.Configuration;
using ArticleDatabase.DataAccessLayer.Models;

namespace ArticleDatabase.DataAccessLayer
{
    public class SqliteDataAccess
    {

        // Login
        public static bool Login(User user)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Check if Username and password match
                    User result = conn.QuerySingleOrDefault<User>("SELECT Id, Username, Password FROM tblUser WHERE Username LIKE @Username AND Password = @Password;",
                        new { Username = user.Username, Password = user.Password }, transaction);

                    if (result != null)
                    {
                        user.ID = result.ID;
                        transaction.Commit();
                        return true;
                    }

                    transaction.Commit();
                    return false;
                }
            }
        }

        // Register
        public static bool Register(User user, int admin = 0)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {

                    // Check if username is available
                    int result = conn.QuerySingleOrDefault<int>("SELECT COUNT(Username) FROM tblUser WHERE Username LIKE @Username;",
                        new { Username = user.Username }, transaction);

                    if (result > 0)
                    {
                        transaction.Commit();
                        return false;
                    }

                    // Register the user (Insert info into User.sqlite3)
                    conn.Execute("INSERT INTO tblUser (Username, Password, Admin) VALUES (@Username, @Password, @Admin);",
                        new { Username = user.Username, Password = user.Password, Admin = admin }, transaction);

                    transaction.Commit();
                    return true;
                }
            }
        }

        // Get admin status
        public static int IsAdmin(User user)
        {
            int result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Check if Username and password match
                    result = conn.QuerySingleOrDefault<int>("SELECT Admin FROM tblUser WHERE Username LIKE @Username AND Password = @Password;",
                        new { Username = user.Username, Password = user.Password }, transaction);
                }
            }

            return result;
        }

        // Get list of all registered users
        public static List<User> GetUsers()
        {
            List<User> results;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    results = conn.Query<User>("SELECT Id, Username FROM tblUser;").ToList();
                }
            }

            return results;
        }

        // Save article to database
        public static void SaveArticle(Article article, User user)
        {
            string count = "";
            string model = "000000";

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    count = conn.QuerySingleOrDefault<int>("SELECT seq + 1 FROM sqlite_sequence WHERE name = 'tblArticle';").ToString();

                    string concat = model.Substring(0, model.Length - count.Length) + count;
                    string final = concat.Substring(0, concat.Length / 2) + "-" + concat.Substring(concat.Length / 2, concat.Length / 2);
                    article.FileName = final;

                    // Insert Article into tblArticle
                    conn.Execute(@"INSERT INTO tblArticle (Title, Year, File) VALUES (@Title, @Year, @FileName);",
                        new { Title = article.Title, Year = article.Year, FileName = article.FileName });
                    article.ID = conn.LastInsertRowId;

                    // Insert PersonalComment and SIC into tblUser
                    conn.Execute("INSERT INTO user.tblUserPersonal (UserID, ArticleID, PersonalComment, SIC) VALUES (@UserID, @ArticleID, @PersonalComment, @SIC);",
                        new { UserID = user.ID, ArticleID = article.ID, PersonalComment = article.PersonalComment, SIC = article.SIC }, transaction);

                    // Insert ArticleID in tblUserArticles so we know which user added this article
                    conn.Execute("INSERT INTO user.tblUserArticles (UserID, ArticleID) VALUES (@UserID, @ArticleID);",
                        new { UserId = user.ID, ArticleID = article.ID }, transaction);

                    AddAuthors(conn, transaction, article);
                    AddKeywords(conn, transaction, article);

                    transaction.Commit();
                }
            }
        }

        // Update article fields in database
        public static void UpdateArticle(Article article, User user)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    List<long> listAuthorID = new List<long>();
                    List<long> listKeywordID = new List<long>();
                    long author_id;
                    long keyword_id;

                    // 1. Update article's direct attributes (title, year)
                    conn.Execute(@"UPDATE tblArticle SET Title=@Title, Year=@Year WHERE ID=@ID;",
                        new { Title = article.Title, Year = article.Year, ID = article.ID }, transaction);

                    // 2. Update or insert comment and SIC
                    int check = conn.QuerySingleOrDefault<int>(@"SELECT ID FROM user.tblUserPersonal WHERE UserID=@UserID AND ArticleID=@ArticleID;",
                        new { UserID = user.ID, ArticleID = article.ID }, transaction);
                    if (check > 0)
                    {
                        conn.Execute(@"UPDATE user.tblUserPersonal SET PersonalComment=@PersonalComment, SIC=@SIC WHERE UserID=@UserID AND ArticleID=@ArticleID;",
                            new { PersonalComment = article.PersonalComment, SIC = article.SIC, UserID = user.ID, ArticleID = article.ID }, transaction);
                    }
                    else
                    {
                        conn.Execute(@"INSERT INTO  user.tblUserPersonal (UserID, ArticleID, PersonalComment, SIC) VALUES(@UserID, @ArticleID, @PersonalComment, @SIC);",
                            new { UserID = user.ID, ArticleID = article.ID, PersonalComment = article.PersonalComment, SIC = article.SIC }, transaction);
                    }

                    // Update authors and keywords

                    // 3. First delete all relationships for current article
                    conn.Execute(@"DELETE FROM jntArticleAuthor WHERE Article_ID=@ArticleID;",
                        new { ArticleID = article.ID }, transaction);

                    // 4. Add new relationships for each author
                    foreach (string author in article.AuthorsCollection)
                    {
                        // If author already exists add its id to listAuthorID then to add realtionship with foreach
                        author_id = conn.QuerySingleOrDefault<long>(@"SELECT ID FROM tblAuthor WHERE Name=@Name;",
                            new { Name = author }, transaction);
                        if (author_id != 0)
                        {
                            listAuthorID.Add(author_id);
                        }
                        else
                        {
                            // If author doesn't exist add it to the database and retrieve last added id
                            conn.Execute(@"INSERT INTO tblAuthor (Name) VALUES (@Name);",
                                new { Name = author }, transaction);
                            listAuthorID.Add(conn.LastInsertRowId);
                        }
                    }

                    // 5. Foreach authorID add it and ArticleID in jntArticleAuthor
                    foreach (long authorID in listAuthorID)
                    {
                        conn.Execute(@"INSERT INTO jntArticleAuthor (Article_ID, Author_ID) VALUES (@ArticleID, @AuthorID);",
                            new { ArticleID = article.ID, AuthorID = authorID }, transaction);
                    }

                    // 6. Do exactly the same as authors just with keywords
                    conn.Execute(@"DELETE FROM jntArticleKeyword WHERE Article_ID=@ArticleID;",
                        new { ArticleID = article.ID });

                    foreach (string keyword in article.KeywordsCollection)
                    {
                        keyword_id = conn.QuerySingleOrDefault<long>(@"SELECT ID FROM tblKeyword WHERE Keyword=@Keyword;",
                            new { Keyword = keyword }, transaction);
                        if (keyword_id != 0)
                        {
                            listKeywordID.Add(keyword_id);
                        }
                        else
                        {
                            conn.Execute(@"INSERT INTO tblKeyword (Keyword) VALUES (@Keyword);",
                                new { Keyword = keyword }, transaction);
                            listKeywordID.Add(conn.LastInsertRowId);
                        }
                    }

                    foreach (long keywordID in listKeywordID)
                    {
                        conn.Execute(@"INSERT INTO jntArticleKeyword (Article_ID, Keyword_ID) VALUES (@ArticleID, @KeywordID)",
                            new { ArticleID = article.ID, KeywordID = keywordID }, transaction);
                    }

                    transaction.Commit();
                }
            }

            // 7. Remove any leftover author or keyword that are not bound to any article
            ConsolidateReferences();
        }

        // Update/Insert only comment and sic
        public static void UpdatePersonal(Article article, User user)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    // Insert PersonalComment and SIC into User database file -> 'tblUsername'
                    int check = conn.QuerySingleOrDefault<int>(@"SELECT ID FROM user.tblUserPersonal WHERE UserID=@UserID AND ArticleID=@ArticleID;",
                        new { UserID = user.ID, ArticleID = article.ID }, transaction);
                    if (check > 0)
                    {
                        conn.Execute(@"UPDATE user.tblUserPersonal SET PersonalComment=@PersonalComment, SIC=@SIC WHERE UserID=@UserID AND ArticleID=@ArticleID;",
                            new { PersonalComment = article.PersonalComment, SIC = article.SIC, UserID = user.ID, ArticleID = article.ID }, transaction);
                    }
                    else
                    {
                        conn.Execute(@"INSERT INTO user.tblUserPersonal (UserID, ArticleID, PersonalComment, SIC) VALUES(@UserID, @ArticleID, @PersonalComment, @SIC);",
                            new { UserID = user.ID, ArticleID = article.ID, PersonalComment = article.PersonalComment, SIC = article.SIC }, transaction);
                    }

                    transaction.Commit();
                }
            }
        }

        // Delete article from database
        public static void DeleteArticle(Article article)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);
                    // Clear author relationships
                    conn.Execute(@"DELETE FROM jntArticleAuthor WHERE Article_ID=@ArticleID;",
                        new { ArticleID = article.ID }, transaction);
                    // Clear keyword relationships
                    conn.Execute(@"DELETE FROM jntArticleKeyword WHERE Article_ID=@ArticleID;",
                        new { ArticleID = article.ID }, transaction);
                    // Remove actual article
                    conn.Execute(@"DELETE FROM tblArticle WHERE ID=@ID;",
                        new { ID = article.ID }, transaction);
                    // Remove from user Article table
                    conn.Execute(@"DELETE FROM user.tblUserArticles WHERE ArticleID=@ArticleID;",
                        new { ArticleID = article.ID }, transaction);
                    // Remove from user Personal table (where PersonalComment and SIC are stored)
                    conn.Execute(@"DELETE FROM user.tblUserPersonal WHERE ArticleID=@ArticleID",
                        new { ArticleID = article.ID }, transaction);
                    transaction.Commit();
                }
            }

            // Remove any leftofer authors and keywords which have no relationships
            ConsolidateReferences();
        }

        // Check if article exists and return the file
        public static bool Exists(string title, out string file)
        {
            bool check;
            string result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.QueryFirstOrDefault<string>(@"SELECT File FROM tblArticle WHERE Title LIKE @Title;", new { Title = title });

                    if (result != null)
                    {
                        check = true;
                        file = result;
                    }
                    else
                    {
                        check = false;
                        file = null;
                    }
                    transaction.Commit();
                }
            }

            return check;
        }

        // Fetch list of articles from database
        public static List<Article> LoadArticles(User user, string title, List<string> authors, List<string> keywords, int offset, int itemsPerPage)
        {
            // Results
            List<Article> results;

            // Template query
            StringBuilder queryBuilder = new StringBuilder(@"
SELECT final.ID, final.Title, final.Authors, final.Keywords, final.Year, final.FileName, final.PersonalComment, final.SIC
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
FROM tblUserPersonal WHERE UserID = #UserID) AS per ON cmp.ID = per.ArticleID) AS final
");
            // 1. Add filters to template query
            string query = AddFilter(queryBuilder, user, title, authors, keywords);

            // 2. Add Pagination
            query += " LIMIT " + itemsPerPage.ToString() + " OFFSET " + offset.ToString() + ";";

            // 3.Fetch articles
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    results = conn.Query<Article>(query, transaction: transaction).ToList();

                    transaction.Commit();
                }
            }

            return results;

        }

        // Get count of fetched articles
        public static int GetRecordCount(User user, string title, List<string> authors, List<string> keywords)
        {
            int result = 1;

            // Template query
            StringBuilder queryBuilder = new StringBuilder(@"
SELECT COUNT(final.ID)
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
FROM tblUserPersonal WHERE UserID = #UserID) AS per ON cmp.ID = per.ArticleID) AS final
");

            // 1. Add filters to template query
            string query = AddFilter(queryBuilder, user, title, authors, keywords);


            // 2. Fetch count
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    result = conn.QuerySingleOrDefault<int>(query, transaction: transaction);

                    transaction.Commit();
                }
            }

            return result;
        }

        // Import section database records
        public static List<CompFile> ImportSection(string importQuery, string filesQuery, string duplicatesQuery, out List<ExistCheck> duplicates)
        {
            List<CompFile> files;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Execute query to merge .sqlite3 files
                    conn.Execute(importQuery, transaction);
                    // Execute query to retrieve list of files to copy
                    files = conn.Query<CompFile>(filesQuery, transaction).ToList();
                    duplicates = conn.Query<ExistCheck>(duplicatesQuery, transaction).ToList();
                    transaction.Commit();
                }
            }

            return files;
        }

        // Get all file names from database
        public static string[] GetFileNames()
        {
            int size;
            List<string> files_from_query;
            string[] fetched_files = null;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    size = conn.QuerySingleOrDefault<int>(@"SELECT COUNT(File) from tblArticle WHERE File IS NOT NULL;");
                    if (size != 0)
                    {
                        fetched_files = new string[size];
                    }
                    else
                    {
                        return null;
                    }

                    // Cycle through each file record and add it to fetched_files array
                    files_from_query = conn.Query<string>(@"SELECT File FROM tblArticle WHERE File IS NOT NULL;").ToList();
                    int i = 0;
                    foreach (string file in files_from_query)
                    {
                        fetched_files[i] = file + ".pdf";
                        i++;
                    }
                    transaction.Commit();
                }
            }

            return fetched_files;
        }

        // Add new bookmark to database
        public static bool AddBookmark(string name, int global, User user)
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
        public static void UpdateBookmark(Bookmark bookmark)
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
        public static void DeleteBookmark(Bookmark bookmark)
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

        // Fetch bookmarks of the user
        public static List<Bookmark> LoadBookmarks(User user, bool global = false)
        {
            List<Bookmark> result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    if (global)
                    {
                        result = conn.Query<Bookmark>($"SELECT ID, Name, Global FROM tblBookmark WHERE UserID = @UserID;",
                            new { UserID = user.ID }, transaction: transaction).ToList();
                    }
                    else
                    {
                        result = conn.Query<Bookmark>($"SELECT ID, Name, Global FROM tblBookmark WHERE UserID = @UserID AND Global = 0;",
                            new { UserID = user.ID }, transaction: transaction).ToList();
                    }
                    transaction.Commit();
                }
            }

            return result;
        }

        // Fetch global bookmarks
        public static List<Bookmark> LoadGlobalBookmarks()
        {
            List<Bookmark> result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.Query<Bookmark>($"SELECT ID, Name, Global FROM tblBookmark WHERE Global = 1;", transaction: transaction).ToList();
                    transaction.Commit();
                }
            }

            return result;
        }

        // Add article to bookmark
        public static void AddArticleToBookmark(Bookmark bookmark, Article article)
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
        public static void RemoveArticleFromBookmark(Bookmark bookmark, Article article)
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

        // Fetch bookmark articles from database
        public static void LoadBookmarkArticles(Bookmark bookmark)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {

                    // !!! We have to load all articles as default (Joined with personal comments, SIC etc. and then we have to inner join it with bookmark table)

                    transaction.Commit();
                }
            }
        }

        // Check if article is in bookmark
        public static bool CheckArticleInBookmark(Bookmark bookmark, Article article)
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
        public static List<Article> LoadArticlesForBookmark(User user, Bookmark bookmark)
        {
            List<Article> results;

            string query = $@"
SELECT final.ID, final.Title, final.Authors, final.Keywords, final.Year, final.FileName, final.PersonalComment, final.SIC
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
WHERE b.ID = {bookmark.ID}; 
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
        public static int CountArticlesInBookmark(Bookmark bookmark)
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

        // Add reference
        public static bool AddReference(string name)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // 1. Check for duplicates (Name to be unique)
                    int check = conn.QuerySingleOrDefault<int>("SELECT COUNT(ID) FROM tblReference WHERE lower(Name) = lower(@Name);",
                        new { Name = name}, transaction: transaction);

                    if (check > 0)
                        return false;

                    // 2. Add reference to database
                    conn.Execute("INSERT INTO tblReference (Name) VALUES (@Name);",
                        new { Name = name });

                    transaction.Commit();
                }
            }

            return true;
        }

        // Update reference
        public static void UpdateReference(Reference reference)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Edit bookmark values in database
                    conn.Execute("UPDATE tblReference SET Name=@Name, ArticleID=@ArticleID WHERE ID=@ID;",
                        new { Name = reference.Name, ArticleID = reference.ArticleID, ID = reference.ID }, transaction: transaction);

                    transaction.Commit();
                }
            }
        }

        // Delete reference
        public static void DeleteReference(Reference reference)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // 1. Delete article relationships 
                    conn.Execute("DELETE FROM tblReferenceArticle WHERE ReferenceID=@ReferenceID;",
                        new { ReferenceID = reference.ID }, transaction: transaction);

                    // 2. Delete bookmark from database
                    conn.Execute("DELETE FROM tblReference WHERE ID=@ID;",
                        new { ID = reference.ID }, transaction: transaction);

                    transaction.Commit();
                }
            }
        }

        // Fetch references
        public static List<Reference> LoadReferences()
        {
            List<Reference> result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.Query<Reference>("SELECT ID, Name, ArticleID FROM tblReference;", transaction: transaction).ToList();
                    transaction.Commit();
                }
            }

            return result;
        }

        public static void AddArticleToReference(Reference reference, Article article)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Execute("INSERT INTO tblReferenceArticle(ReferenceID, ArticleID) VALUES (@ReferenceID, @ArticleID);",
                        new { ReferenceID = reference.ID, ArticleID = article.ID });

                    transaction.Commit();
                }
            }
        }

        // Remove article from reference
        public static void RemoveArticleFromReference(Reference reference, Article article)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Execute("DELETE FROM tblReferenceArticle WHERE ReferenceID = @ReferenceID AND ArticleID = @ArticleID;",
                        new { ReferenceID = reference.ID, ArticleID = article.ID });

                    transaction.Commit();
                }
            }
        }

        // Fetch reference articles from database
        public static void LoadReferenceArticles(Reference reference)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {

                    // !!! We have to load all articles as default (Joined with personal comments, SIC etc. and then we have to inner join it with bookmark table)

                    transaction.Commit();
                }
            }
        }

        // Check if article is in reference
        public static bool CheckArticleInReference(Reference reference, Article article)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    int check = conn.QuerySingleOrDefault<int>("SELECT COUNT(ID) FROM tblReferenceArticle WHERE ReferenceID = @ReferenceID AND ArticleID = @ArticleID;",
                        new { ReferenceID = reference.ID, ArticleID = article.ID });

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

        // Load articles for reference
        public static List<Article> LoadArticlesForReference(Reference reference)
        {
            List<Article> results;

            string query = @"
SELECT cmp.ID, cmp.Article AS Title, cmp.Authors, cmp.Keywords, cmp.Year, cmp.FileName AS [FileName]
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
JOIN tblReferenceArticle as ra ON cmp.ID = ra.ArticleID
JOIN tblReference as r ON r.ID = ra.ReferenceID
WHERE r.ID = @ReferenceID; 
";
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    results = conn.Query<Article>(query, new { ReferenceID = reference.ID }, transaction: transaction).ToList();

                    transaction.Commit();
                }
            }

            return results;
        }

        public static Article GetArticleWithId(int? id)
        {
            // Null escape
            if (id == null)
                return new Article();

            Article result;

            string query = @"
SELECT cmp.ID, cmp.Article AS Title, cmp.Authors, cmp.Keywords, cmp.Year, cmp.FileName AS [FileName]
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
WHERE cmp.ID = @ID;
";

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.QuerySingleOrDefault<Article>(query, new { ID = id }, transaction: transaction);
                    transaction.Commit();
                }
            }

            return result;
        }

        public static int CheckArticleWithTitle(string title)
        {
            int result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.QuerySingleOrDefault<int>(@"SELECT ID FROM tblArticle WHERE Title=@Title;", new { Title = title }, transaction: transaction);
                    transaction.Commit();
                }
            }

            return result;
        }

        /**
         * Private helpers
         */
        #region Private helpers
        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
        private static string AttachUser()
        {
            return ConfigurationManager.AppSettings["Attach"];
        }
        private static void AddAuthors(SQLiteConnection conn, SQLiteTransaction transaction, Article article)
        {
            long author_id;
            List<long> lstAuthorID = new List<long>();
            // Insert Authors
            if (article.AuthorsCollection.Count > 0)
            {
                // Iterate through Authors collection
                foreach (string author in article.AuthorsCollection)
                {
                    // Check if the author is already in database
                    author_id = conn.QueryFirstOrDefault<long>(@"SELECT ID FROM tblAuthor WHERE Name LIKE @Name;",
                        new { Name = author }, transaction);
                    if (author_id != 0)
                    {
                        // If the author is in database retrieve its id and add it to lstAuthorID
                        lstAuthorID.Add(author_id);
                    }
                    else
                    {
                        conn.Execute(@"INSERT INTO tblAuthor (Name) VALUES (@Name);",
                            new { Name = author }, transaction);
                        lstAuthorID.Add(conn.LastInsertRowId);
                    }
                }

                //Foreach author Ids insert ArticleID and AuthorIds from list in joint table for many-to-many relationship
                foreach (long authorID in lstAuthorID)
                {
                    conn.Execute(@"INSERT into jntArticleAuthor (Article_ID, Author_ID) VALUES (@ArticleID, @AuthorID);",
                        new { ArticleID = article.ID, AuthorID = authorID }, transaction);
                }
            }
        }
        private static void AddKeywords(SQLiteConnection conn, SQLiteTransaction transaction, Article article)
        {
            long keyword_id;
            List<long> lstKeywordID = new List<long>();

            // Insert Keywords --> See reference to "Insert Authors"
            if (article.KeywordsCollection.Count > 0)
            {
                foreach (var keyword in article.KeywordsCollection)
                {
                    keyword_id = conn.QueryFirstOrDefault<long>(@"SELECT ID FROM tblKeyword WHERE Keyword LIKE @Keyword;",
                        new { Keyword = keyword }, transaction);
                    if (keyword_id != 0)
                    {
                        lstKeywordID.Add(keyword_id);
                    }
                    else
                    {
                        conn.Execute(@"INSERT INTO tblKeyword (Keyword) VALUES (@Keyword);",
                            new { Keyword = keyword }, transaction);
                        lstKeywordID.Add(conn.LastInsertRowId);
                    }
                }

                foreach (var keywordID in lstKeywordID)
                {
                    conn.Execute(@"INSERT into jntArticleKeyword (Article_ID, Keyword_ID) VALUES (@ArticleID, @KeywordID);",
                        new { ArticleID = article.ID, KeywordID = keywordID }, transaction);
                }
            }
        }
        private static string AddFilter(StringBuilder queryBuilder, User user, string title, List<string> authors, List<string> keywords)
        {
            StringBuilder result = queryBuilder;

            // 1. Add user id for comments and sic
            result.Replace("#UserID", user.ID.ToString());

            // 2. Append title filter
            if (title != null)
                result.Append(" WHERE final.Title LIKE " + ToWildCard(title));


            // 3. Add WHERE clause if title was null but authors or keywords aren't
            if ((authors.Count > 0 || keywords.Count > 0) && title == null)
                result.Append(" WHERE ");

            // 4. Add authors filter
            if (authors.Count > 0)
            {
                // 1. Determine if we need to add "AND"
                if (title != null)
                    result.Append(" AND ");

                // 2. Add filter clause for each author
                foreach (string author in authors)
                {
                    result.Append("final.Authors LIKE " + ToWildCard(author));

                    // If its not the last iteration add "AND"
                    if (author != authors.Last())
                        result.Append(" AND ");
                }
            }

            // 5. Add keywords filter
            if (keywords.Count > 0)
            {
                // 1. Determine if we need to add "AND"
                if (authors.Count > 0 || title != null)
                    result.Append(" AND ");

                // 2. Add filter clause for each author
                foreach (string keyword in keywords)
                {
                    result.Append("final.Keywords LIKE " + ToWildCard(keyword));

                    // If its not the last iteration add "AND"
                    if (keyword != keywords.Last())
                        result.Append(" AND ");
                }
            }

            // 6. Return the result
            return result.ToString();
        }
        private static string ToWildCard(string input)
        {
            return "'%" + input + "%'";
        }
        private static void ConsolidateReferences()
        {
            string sql;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // 1. Authors check
                    sql = @"
DELETE FROM tblAuthor WHERE ID IN
(SELECT ath.ID
FROM tblAuthor AS ath
LEFT JOIN jntArticleAuthor AS aa ON ath.ID = aa.Author_ID
WHERE aa.Author_ID IS NULL);
";
                    conn.Execute(sql, transaction);

                    // 2. Keywords check
                    sql = @"
DELETE FROM tblKeyword WHERE ID IN
(SELECT kwd.ID
FROM tblKeyword AS kwd
LEFT JOIN jntArticleKeyword AS ak ON kwd.ID = ak.Keyword_ID
WHERE ak.Keyword_ID IS NULL);
";
                    conn.Execute(sql, transaction);
                    transaction.Commit();
                }
            }
        }
        #endregion
    }
}
