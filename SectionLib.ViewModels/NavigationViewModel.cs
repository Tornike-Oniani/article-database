using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;
using SectionLib.ViewModels.Commands;
using SectionLib.ViewModels.Main;
using SectionLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SectionLib.ViewModels
{
    public class NavigationViewModel : BaseViewModel
    {
        private BaseViewModel _selectedViewModel;
        private string _selectedSection;

        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set { _selectedViewModel = value; OnPropertyChanged("SelectedViewModel"); }
        }
        public User User { get; set; }
        public ObservableCollection<string> Sections { get; set; }
        public string SelectedSection
        {
            get { return _selectedSection; }
            set
            {
                _selectedSection = value;

                // If no section is selected disable navigation
                if (value == null)
                {
                    _canNavigate = false;
                    OnPropertyChanged("CanNavigate");
                }
                else
                {
                    _canNavigate = true;
                    OnPropertyChanged("CanNavigate");
                }

                // 1. Update connection string
                ConfigurationManager.ConnectionStrings["Default"].ConnectionString = "Data Source=Sections\\" + value + "\\NikasDB.sqlite3";
                ConfigurationManager.AppSettings["Attach"] = "ATTACH DATABASE \'" + Environment.CurrentDirectory + "\\" + "Sections\\" + SelectedSection + "\\" + "User.sqlite3\'" + "AS user;";

                // 2. If section was changed and the user is in data search, clear the search so that user will not see previous section's data
                //if (SelectedViewModel is DataViewViewModel)
                //    ((DataViewViewModel)SelectedViewModel).Articles.Clear();

                // 3. Update static selected section for copy paths
                Program.SelectedSection = value;

                OnPropertyChanged("SelectedSection");
            }
        }

        private bool _canNavigate;

        public bool CanNavigate
        {
            get { return _canNavigate; }
            set { _canNavigate = value; }
        }

        public ICommand UpdateViewCommand { get; set; }
        public RelayCommand GetSectionsCommand { get; set; }

        public NavigationViewModel(IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            this.User = new UserRepo().LoginFirst();
            UpdateViewCommand = new UpdateViewCommand(this, User, dialogService, windowService, browserService);
            UpdateViewCommand.Execute(ViewType.Home);
            this.Title = User.Username;
            Sections = new ObservableCollection<string>();

            // Initialize commands
            GetSectionsCommand = new RelayCommand(GetSections);

            // Change configuration connection string to writable
            var fi = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            fi.SetValue(ConfigurationManager.ConnectionStrings["Default"], false);

            // Populate sections
            GetSectionsCommand.Execute(null);
        }

        public void GetSections(object input = null)
        {
            // 1. Clear sections
            Sections.Clear();

            // 2. Fetch all sections from database
            foreach (string section in new SectionRepo().GetSections())
                Sections.Add(section);

            // 3. Set last section as selected
            if (Sections.Count > 0)
                SelectedSection = Sections.Last();
        }
    }
}
