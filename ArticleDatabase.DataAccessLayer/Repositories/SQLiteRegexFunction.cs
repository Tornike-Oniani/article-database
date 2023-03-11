using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.DataAccessLayer.Repositories
{
    [SQLiteFunction(Name ="REGEXP", Arguments = 2, FuncType = FunctionType.Scalar)]
    internal class SQLiteRegexFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(Convert.ToString(args[1]), Convert.ToString(args[0]));
        }
    }
}
