namespace Lib.DataAccessLayer.Models
{
    public class Abstract
    {
        public int Id { get; set; }
        public string ArticleTitle { get; set; }
        public string Body { get; set; }

        // Constructor
        public Abstract()
        {

        }
        public Abstract(int id, int articleTitle, int body)
        {
            this.Id = id;
            this.ArticleTitle = ArticleTitle;
            this.Body = Body;
        }
    }
}
