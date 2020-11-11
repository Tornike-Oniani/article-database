using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectionBrowser.DataAccess.Repositories.Base
{
    public abstract class BaseRepo
    {
        protected string LoadConnectionString(string id = "Default")
        {
            if (id == "Default")
                return Connection.ConnectionString;
            else if (id == "Main")
                return Connection.MainString;
            else
                throw new ArgumentException("Invalid argument");
        }
        protected string AttachUser()
        {
            return Connection.UserString;
        }
    }
}
