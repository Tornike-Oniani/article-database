using System.Text;

namespace Lib.DataAccessLayer.Utils
{
    public class Filter
    {
        public StringBuilder FilterQuery { get; private set; }
        public bool Modified { get; set; } = false;

        public Filter()
        {
            this.FilterQuery = new StringBuilder("");
        }

        public string GetFilterString()
        {
            return FilterQuery.ToString();
        }
    }
}
