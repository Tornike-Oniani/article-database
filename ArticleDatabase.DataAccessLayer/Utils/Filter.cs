using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.DataAccessLayer.Utils
{
    public class Filter
    {
        public StringBuilder FilterQuery { get; private set; }
        public bool Modified { get; set; } = false;

        public Filter()
        {
            this.FilterQuery = new StringBuilder(" WHERE ");
        }

        public string GetFilterString()
        {
            return FilterQuery.ToString();
        }
    }
}
