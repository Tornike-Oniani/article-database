using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Utils;
using System.Collections.Generic;
using System.Windows.Input;

namespace MainLib.ViewModels.Main
{
    public class SQLViewModel : BaseViewModel
    {
        // Private attributes
        private string _sqlQuery;
        private List<object> _items;
        private Shared services;

        // Public properties
        public string SqlQuery
        {
            get { return _sqlQuery; }
            set { _sqlQuery = value; OnPropertyChanged("SqlQuery"); }
        }
        public List<object> Items
        {
            get { return _items; }
            set { _items = value; OnPropertyChanged("Items"); }
        }

        // Commands
        public ICommand ExecuteSQLCommand { get; set; }
        public ICommand QuerySQLCommand { get; set; }

        // Constructor
        public SQLViewModel()
        {
            // init
            this.services = Shared.GetInstance();
            // Set commands
            ExecuteSQLCommand = new RelayCommand(ExecuteSQL);
            QuerySQLCommand = new RelayCommand(QuerySQL);
        }

        // Command actions
        public void ExecuteSQL(object input = null)
        {
            try
            {
                services.IsWorking(true, "Executing sql query...");
                new GenericRepo().ExecuteSQL(SqlQuery);
                services.IsWorking(false);
            }
            catch
            {

            }
            finally
            {
                services.IsWorking(false);
            }
        }
        public void QuerySQL(object input = null)
        {
            try
            {
                services.IsWorking(true, "Executing sql query...");
                Items = new GenericRepo().QuerySQL(SqlQuery);
                services.IsWorking(false);
            }
            catch
            {
                Items = new List<object>();
            }
            finally
            {
                services.IsWorking(false);
            }
        }
    }
}
