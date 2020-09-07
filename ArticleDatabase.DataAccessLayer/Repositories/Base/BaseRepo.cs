using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleDatabase.DataAccessLayer.Repositories.Base
{
    public abstract class BaseRepo
    {
        protected string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
        protected string AttachUser()
        {
            return ConfigurationManager.AppSettings["Attach"];
        }
    }
}
