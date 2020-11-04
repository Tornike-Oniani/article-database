using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Popups;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;
using MainLib.ViewModels.Utils;
using System.CodeDom;

namespace MainLib.ViewModels.Main
{
    public class HomeViewModel : BaseViewModel
    {
        // Private members
        private User _user;
        private Action<bool> _workStatus;
        private IDialogService _dialogService;
        private IBrowserService _browserService;
        private IWindowService _windowService;

        // Public properties
        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }

        // Commands
        public RelayCommand ValidateCommand { get; set; }
        public RelayCommand ImportCommand { get; set; }
        public RelayCommand SyncCommand { get; set; }
        public RelayCommand ExportSyncCommand { get; set; }

        // Constructor
        public HomeViewModel(
            User user, 
            Action<bool> workStatus, 
            IDialogService dialogService, 
            IWindowService windowService, 
            IBrowserService browserService)
        {
            this.User = user;
            this._workStatus = workStatus;
            this._dialogService = dialogService;
            this._windowService= windowService;
            this._browserService = browserService;

            ValidateCommand = new RelayCommand(Validate);
            ImportCommand = new RelayCommand(Import);
            SyncCommand = new RelayCommand(Sync);
            ExportSyncCommand = new RelayCommand(ExportSync);
        }

        // Command actions
        public async void Validate(object input = null)
        {
            try
            {
                // Path to Logs folder (root directory + Logs)
                string current_directory = System.IO.Path.Combine(Environment.CurrentDirectory, "Logs\\");

                // All pdf files in Files folder
                string[] existingFiles = Directory.GetFiles(System.IO.Path.Combine(Environment.CurrentDirectory, "Files"), "*.pdf").Select(System.IO.Path.GetFileName).ToArray();
                // All file names in tblArticle database table
                string[] databaseFiles = (new GlobalRepo()).GetFileNames();

                // Mismatch between physical pdf files and database names
                if (databaseFiles != null)
                {
                    string[] mismatch = databaseFiles.Except(existingFiles).ToArray();
                    // If any mismatch was found create log file and write mismatched files in there
                    if (mismatch.Length > 0)
                    {
                        _workStatus(true);

                        await Task.Run(() =>
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
                        });

                        _workStatus(false);

                        _dialogService.OpenDialog(new DialogOkViewModel("Some files are missing see Logs for more info...", "Result", DialogType.Warning));
                    }
                    else
                    {
                        _dialogService.OpenDialog(new DialogOkViewModel("No missing files were found!", "Result", DialogType.Success));
                    }
                }
                else
                {
                    _dialogService.OpenDialog(new DialogOkViewModel("There are no records in database yet", "Result", DialogType.Information));
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("App", "Validate", e.Message);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                _workStatus(false);
            }
        }
        public void Import(object input = null)
        {
            try
            {
                string destination = null;

                // 1. Using winforms dialog box select a folder
                destination = _browserService.OpenFolderDialog();

                // 2. If nothing was selected return
                if (destination == null)
                    return;

                // 3. If the wrong folder was selected return with an error messagebox
                if ((!File.Exists(destination + "\\" + "NikasDB.sqlite3")) || (!Directory.Exists(destination + @"\Files")) || (!File.Exists(destination + "\\" + "user.sqlite3")))
                {
                    _dialogService.OpenDialog(new DialogOkViewModel("Please select a correct folder.", "Error", DialogType.Error));
                    return;
                }

                string root_directory = Environment.CurrentDirectory;

                // 4. If an old temp query file exists delete it
                if (File.Exists(Environment.CurrentDirectory + "\\" + "temp_query.txt"))
                    File.Delete(Environment.CurrentDirectory + "\\" + "temp_query.txt");

                // 5. Set up queries for database import
                string base_query = System.IO.File.ReadAllText(root_directory + "\\" + "Query.txt");
                string file_path = root_directory + "\\" + "temp_query.txt";
                string attach_string =
                    "ATTACH DATABASE \'" + root_directory + "\\" + "user.sqlite3\'" + "AS user;\n" +
                    "ATTACH DATABASE \'" + destination + "\\" + "NikasDB.sqlite3\'" + "AS db2main;\n" +
                    "ATTACH DATABASE \'" + destination + "\\" + "user.sqlite3\'" + "AS db2user;\n";
                string files_query = @"SELECT tf.File AS OldFile, tnf.File AS NewFile FROM temp_Files AS tf, temp_NewFiles AS tnf WHERE tf.Title = tnf.Title;";
                string duplicates_query = @"SELECT Title, File FROM temp_tblDuplicates;";

                // Open or Create file
                using (StreamWriter sw = File.AppendText(file_path))
                {
                    sw.WriteLine(attach_string);
                    sw.WriteLine(base_query);
                }

                // Path to temp file where full query is written (Including attaches)
                string full_query = System.IO.File.ReadAllText(root_directory + "\\" + "temp_query.txt");

                // 6. Import recrods to database, return list of files to copy and duplicates to log
                List<ExistCheck> duplicates;
                List<CompFile> files = (new GlobalRepo()).ImportSection(full_query, files_query, duplicates_query, out duplicates);

                // 7. Create dictionary of files to copy
                Dictionary<string, string> files_to_copy = new Dictionary<string, string>();
                foreach (CompFile file in files)
                {
                    files_to_copy.Add(file.OldFile + ".pdf", file.NewFile + ".pdf");
                }

                // 8. Log duplicate files
                DirectoryInfo current_directory_logs = new DirectoryInfo("." + @"\Logs\");

                // Current date will be set as file name
                DateTime current_date = DateTime.Today;
                // Current time
                DateTime current_time = DateTime.Now;
                // Full file path including its name
                string merge_log_path = current_directory_logs.FullName + "Merge --- " + current_date.ToLongDateString() + ".txt";

                // Open or Create file
                using (StreamWriter sw = File.AppendText(merge_log_path))
                {
                    // Start writing log starting with current hour as timestamp
                    sw.WriteLine(current_time.ToLongTimeString());
                    sw.WriteLine("Following titles are duplicate:");
                    // Write each file name from mismatch folder into text file
                    foreach (ExistCheck duplicate in duplicates)
                        sw.WriteLine(duplicate.Title + " ||| " + duplicate.File);
                    sw.WriteLine("");
                }

                // 9. Create progress bar and copy physical .pdf files
                _windowService.OpenWindow(
                    new ImportViewModel(files_to_copy, destination, _dialogService),
                    WindowType.Generic,
                    true,
                    true,
                    true
                    );
            }
            catch(Exception e)
            {
                new BugTracker().Track("App", "Import", e.Message);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                _workStatus(false);
            }
        }
        public async void Sync(object input = null)
        {
            try
            {
                string destination = _browserService.OpenFolderDialog();

                if (destination == null)
                    return;

                if (
                    !Directory.Exists(Path.Combine(destination, "Files")) ||
                    !File.Exists(Path.Combine(destination, "log.json"))
                    )
                {
                    _dialogService.OpenDialog(new DialogOkViewModel("Please select a valid folder", "Sync", DialogType.Error));
                    return;
                }

                _workStatus(true);

                LogReader reader = new LogReader(Path.Combine(destination, "Files"));

                await Task.Run(() =>
                {
                    reader.GetLogs(destination);
                    reader.Sync();
                });

                _workStatus(false);


                // Show messagebox regarding errors
                if (reader.NoErrors)
                    _dialogService.OpenDialog(new DialogOkViewModel("Synchronisation successful", "Synchronisation", DialogType.Success));
                else
                    _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong, see logs for more information", "Synchronisation", DialogType.Error));
            }
            catch(Exception e)
            {
                new BugTracker().Track("App", "Sync", e.Message);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                _workStatus(false);
            }
        }
        public async void ExportSync(object input = null)
        {
            try
            {
                string destination = _browserService.OpenFolderDialog();

                if (destination == null)
                    return;

                if (Directory.Exists(Path.Combine(destination, "Sync")))
                {
                    _dialogService.OpenDialog(
                        new DialogOkViewModel(
                            "This folder already contains sync information. Please choose different path",
                            "Export Sync",
                            DialogType.Error)
                        );
                    return;
                }

                destination = Path.Combine(destination, "Sync");

                string syncPath = Path.Combine(Environment.CurrentDirectory, "Sync");

                if (!Directory.Exists(syncPath))
                {
                    _dialogService.OpenDialog(
                        new DialogOkViewModel(
                            "No information to export, please restart application.",
                            "Export Sync",
                            DialogType.Error)
                        );
                    return;
                }

                _workStatus(true);

                await Task.Run(() =>
                {
                    // 0. Modify file to turn json into array
                    string fileContent = File.ReadAllText(Path.Combine(syncPath, "log.json"));
                    if (!string.IsNullOrEmpty(fileContent))
                    {
                        int index = fileContent.LastIndexOf(',');
                        fileContent = "[" + fileContent.Remove(index, 1) + "]";
                        using (StreamWriter sw = new StreamWriter(Path.Combine(syncPath, "log.json")))
                        {
                            sw.WriteLine(fileContent);
                        }
                    }

                    // 1. Create/get backup folder path
                    string backupPath = Path.Combine(Environment.CurrentDirectory, "Backup");

                    if (!Directory.Exists(backupPath))
                        Directory.CreateDirectory(backupPath);

                    // 2. Generate backup name (using current date)
                    string backupName = "Sync - " + DateTime.Today.ToLongDateString();
                    backupPath = Path.Combine(backupPath, backupName);

                    // 3. Backup sync
                    if (Directory.Exists(backupPath))
                    {
                        string current_time = DateTime.Now.ToLongTimeString();
                        string modified_time = "";
                        foreach (string item in current_time.Split(':'))
                        {
                            modified_time += item + "-";
                        }
                        modified_time = modified_time.Remove(modified_time.Length - 1, 1);
                        backupPath += " -- " + modified_time;
                    }

                    RelocateFiles(syncPath, backupPath);

                    // 4. Move sync to destination                  
                    RelocateFiles(syncPath, destination, true);

                    // 5. Recreate sync
                    new Tracker(User).init();
                });

                _workStatus(false);
            }
            catch (Exception e)
            {
                new BugTracker().Track("App", "Export Sync", e.Message);
                _dialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                _workStatus(false);
            }
        }

        private void RelocateFiles(string root, string destination, bool Move = false)
        {
            // Create subdirectory structure in destination    
            foreach (string dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(Path.Combine(destination, dir.Substring(root.Length + 1)));
                // Example:
                //     > C:\sources (and not C:\E:\sources)
            }

            if (Move)
            {
                foreach (string file_name in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                {
                    File.Move(file_name, Path.Combine(destination, file_name.Substring(root.Length + 1)));
                }

                Directory.Delete(root, true);
            }
            else
            {
                foreach (string file_name in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                {
                    File.Copy(file_name, Path.Combine(destination, file_name.Substring(root.Length + 1)));
                }
            }
        }
    }
}
