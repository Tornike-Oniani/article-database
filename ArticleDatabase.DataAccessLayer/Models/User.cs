using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleDatabase.DataAccessLayer.Models
{
    public class User : BaseModel
    {
        private int? _id;
        private string _username;
        private string _password;
        private int _admin;

        public int? ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public int Admin
        {
            get { return _admin; }
            set { _admin = value; }
        }

        public bool IsAdmin { get { return Admin == 1; } }

    }
}
