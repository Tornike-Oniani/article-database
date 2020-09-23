using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;
using SectionLib.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectionLib.ViewModels.Main
{
    public class HomeViewModel : BaseViewModel
    {
        private NavigationViewModel _mainViewModel;
        private IDialogService _dialogService;
        private IWindowService _windowService;
        private IBrowserService _browserService;

        public RelayCommand AddSectionCommand { get; set; }
        public RelayCommand DeleteSectionCommand { get; set; }
        public RelayCommand ValidateCommand { get; set; }
        public RelayCommand ExportCommand { get; set; }

        public HomeViewModel(NavigationViewModel mainViewModel, IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            this._mainViewModel = mainViewModel;
            this._dialogService = dialogService;
            this._windowService = windowService;
            this._browserService = browserService;

            // 1. Initialize commands
            ValidateCommand = new RelayCommand(Validate, CanValidate);
            AddSectionCommand = new RelayCommand(AddSection);
            DeleteSectionCommand = new RelayCommand(DeleteSection, CanDeleteSection);
            ExportCommand = new RelayCommand(Export, CanExport);
        }


        public void AddSection(object input = null)
        {
            // 1. Add section to database
            new SectionRepo().AddSection();

            // 2. Repopulate the Sections list and set the selected section to last (The last will be the section we just created)
            _mainViewModel.GetSections();

            // 3. Check if 'Sections' folder exists
            string sections_path = Path.Combine(Environment.CurrentDirectory, "Sections");
            string section_folder_path = Path.Combine(sections_path, _mainViewModel.SelectedSection);
            if (!Directory.Exists(sections_path))
                Directory.CreateDirectory(sections_path);

            // 4. Create folders for the new section
            Directory.CreateDirectory(section_folder_path);
            Directory.CreateDirectory(Path.Combine(section_folder_path, "Files"));
            Directory.CreateDirectory(Path.Combine(section_folder_path, "Logs"));

            // 5. Copy blank database files into new section directory
            File.Copy(Path.Combine(Environment.CurrentDirectory, "NikasDB.sqlite3"), Path.Combine(section_folder_path, "NikasDB.sqlite3"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "User.sqlite3"), Path.Combine(section_folder_path, "User.sqlite3"));
        }
        public void DeleteSection(object input = null)
        {
            // 1. Ask user if he wants to delete the selected section
            if (_dialogService.OpenDialog(new DialogYesNoViewModel(
                "Do you want to delete " + _mainViewModel.SelectedSection + " and all its files?",
                "Delete section",
                DialogType.Warning
                )))
            {
                // 2. Delte the section from database
                new SectionRepo().DeleteSection(_mainViewModel.SelectedSection);

                // 3. Remove folders and file of the selected section
                string section_folder_path = Path.Combine(Environment.CurrentDirectory, "Sections", _mainViewModel.SelectedSection);
                Directory.Delete(section_folder_path, true);

                // 4. Repopulate the Sections list and set the selected section to last (The last will be the section we just created)
                _mainViewModel.GetSections();
            }
            else
            {
                // If user clicked no do nothing
                return;
            }
        }
        public void Validate(object input = null)
        {
            // Path to Logs folder (root directory + Logs)
            string current_directory = Program.GetSectionLogsPath();

            // All pdf files in Files folder
            //string[] existingFiles = Directory.GetFiles(User.GetFilesFolder(), " *.pdf").Select(System.IO.Path.GetFileName).ToArray();
            string[] existingFiles = Directory.GetFiles(Program.GetSectionFilesPath(), "*.pdf").Select(System.IO.Path.GetFileName).ToArray();
            // All file names in tblArticle database table
            string[] databaseFiles = new GlobalRepo().GetFileNames();

            // Mismatch between physical pdf files and database names
            if (databaseFiles != null)
            {
                string[] mismatch = databaseFiles.Except(existingFiles).ToArray();
                // If any mismatch was found create log file and write mismatched files in there
                if (mismatch.Length > 0)
                {
                    // Current date will be set as file name
                    DateTime current_date = DateTime.Today;
                    // Full file path including its name
                    string file_path = current_directory + current_date.ToLongDateString() + ".txt";

                    // Open or Create file
                    using (StreamWriter sw = File.AppendText(file_path))
                    {
                        // Start writing log startin with current hour as timestamp
                        sw.WriteLine(current_date.ToLongTimeString());
                        sw.WriteLine("Following files are missing:");
                        // Write each file name from mismatch folder into text file
                        foreach (string file in mismatch)
                            sw.WriteLine(file);
                        sw.WriteLine("");
                    }

                    _dialogService.OpenDialog(new DialogOkViewModel("Some files are missing see Logs for more info...", "Validation", DialogType.Error));
                }
                else
                {
                    _dialogService.OpenDialog(new DialogOkViewModel("No missing files were found!", "Validation", DialogType.Success));
                }
            }
            else
            {
                _dialogService.OpenDialog(new DialogOkViewModel("There are no records in database yet", "Validation", DialogType.Information));
            }
        }
        public void Export(object input = null)
        {
            if (Program.SelectedSection == null)
            {
                _dialogService.OpenDialog(new DialogViewModelBase("Please select a section", "Export", DialogType.Error));
                return;
            }

            string folder_name = _mainViewModel.User.Username + " - " + Program.SelectedSection;

            string destination = null;

            // 1. Using winforms dialog box select a folder
            destination = _browserService.OpenFolderDialog();

            // 2. If nothing was selected return
            if (destination == null)
                return;

            destination += "\\";

            // 3. If directory already exists ask the user if he wants to overwrite the files
            if (Directory.Exists(destination + folder_name))
            {
                // If clicked yes
                if (_dialogService.OpenDialog(new DialogYesNoViewModel(
                    "Export folder already exists, do you wish to overwrite?",
                    "Export",
                    DialogType.Warning
                    )))
                {
                    // Delete the folder
                    DirectoryInfo delete_folder = new DirectoryInfo(destination + folder_name);
                    delete_folder.Delete(true);
                }
                else
                {
                    // If clicked no terminate the function
                    _dialogService.OpenDialog(new DialogOkViewModel("Export terminated. Please select a different path...", "Export", DialogType.Error));
                    return;
                }
            }

            // 4. Create folders in destination and copy database files
            Directory.CreateDirectory(destination + folder_name);
            Directory.CreateDirectory(destination + folder_name + @"\Files");
            File.Copy(Program.GetSectionPath() + @"\NikasDB.sqlite3", destination + folder_name + "\\" + "NikasDB.sqlite3");
            File.Copy(Program.GetSectionPath() + @"\user.sqlite3", destination + folder_name + "\\" + "user.sqlite3");

            // 9. Create progress bar and copy physical .pdf files
            _windowService.OpenWindow(
                new ExportViewModel(Program.GetSectionFilesPath(), destination, folder_name, _dialogService),
                WindowType.Generic,
                true,
                true,
                true
                );
        }

        public bool CanDeleteSection(object input = null)
        {
            if (_mainViewModel.SelectedSection != null)
                return true;

            return false;
        }
        public bool CanValidate(object input = null)
        {
            if (_mainViewModel.SelectedSection != null)
                return true;

            return false;
        }
        public bool CanExport(object input = null)
        {
            if (Program.SelectedSection != null)
                return true;

            return false;
        }
    }
}
