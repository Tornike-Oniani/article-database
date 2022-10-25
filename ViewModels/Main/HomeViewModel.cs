using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;
using MainLib.ViewModels.Popups;
using MainLib.ViewModels.Utils;
using Newtonsoft.Json;
using NotificationService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Main
{
    public class HomeViewModel : BaseViewModel
    {
        // Private members
        private User _user;
        private string _exportedSync;
        private string _syncNameAndNumber;
        private Shared services;

        // Public properties
        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }
        public string ExportedSync
        {
            get { return _exportedSync; }
            set { _exportedSync = value; OnPropertyChanged("ExportedSync"); }
        }
        public string SyncNameAndNumber
        {
            get { return _syncNameAndNumber; }
            set { _syncNameAndNumber = value; OnPropertyChanged("SyncNameAndNumber"); }
        }

        // Commands
        public RelayCommand ValidateCommand { get; set; }
        public RelayCommand ImportCommand { get; set; }
        public RelayCommand SyncCommand { get; set; }
        public RelayCommand ExportSyncCommand { get; set; }

        // Constructor
        public HomeViewModel()
        {
            this.services = Shared.GetInstance();
            this.User = services.User;

            ValidateCommand = new RelayCommand(Validate);
            ImportCommand = new RelayCommand(Import);
            SyncCommand = new RelayCommand(Sync);
            ExportSyncCommand = new RelayCommand(ExportSync);

            UpdateSyncInformationDisplay();
        }

        // Command actions
        public async void Validate(object input = null)
        {
            try
            {
                // Path to Logs folder (root directory + Logs)
                string current_directory = System.IO.Path.Combine(Environment.CurrentDirectory, "Logs\\");

                // All pdf files in Files folder
                string[] existingFiles = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Files"), "*.pdf").Select(Path.GetFileName).ToArray();
                // All file names in tblArticle database table
                string[] databaseFiles = new GlobalRepo().GetFileNames();

                // Mismatch between physical pdf files and database names
                if (databaseFiles != null)
                {
                    string[] mismatch = databaseFiles.Except(existingFiles).ToArray();
                    // If any mismatch was found create log file and write mismatched files in there
                    if (mismatch.Length > 0)
                    {
                        services.IsWorking(true);

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

                        services.IsWorking(false);

                        //services.DialogService.OpenDialog(new DialogOkViewModel("Some files are missing see Logs for more info...", "Result", DialogType.Warning));
                        services.ShowNotification("Some files are missing see Logs for more info...", "Database validation", NotificationType.Error, "HomeNotificationArea", TimeSpan.MaxValue);

                    }
                    else
                    {
                        //services.DialogService.OpenDialog(new DialogOkViewModel("No missing files were found!", "Result", DialogType.Success));
                        services.ShowNotification("No missing files were found!", "Database validation", NotificationType.Success, "HomeNotificationArea", TimeSpan.MaxValue);
                    }
                }
                else
                {
                    //services.DialogService.OpenDialog(new DialogOkViewModel("There are no records in database yet", "Result", DialogType.Information));
                    services.ShowNotification("There are no records in database yet", "Database validation", NotificationType.Success, "HomeNotificationArea", TimeSpan.MaxValue);
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("App", "Validate", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
            }
        }
        public void Import(object input = null)
        {
            try
            {
                string destination;

                if (!SelectAndCheckFolder(out destination))
                    return;

                string root_directory = Environment.CurrentDirectory;

                // If an old temp query file exists delete it
                QueryManager queryManager = new QueryManager(destination);
                queryManager.DeleteTempQuery();

                // Generate section name and add it to json file
                SectionManager sectionManager = new SectionManager();
                sectionManager.GenerateSectionName();
                sectionManager.AddSectionToJsonFile();

                // Query for pending articles
                queryManager.SetPendingQuery(sectionManager.sectionName);

                // Create temp query file
                queryManager.CreateTempQuery();

                // Import recrods to database, return list of files to copy and duplicates to log
                FileManager fileManager = new FileManager();
                fileManager.files = new GlobalRepo().ImportSection(
                    queryManager.GetFullQuery(),
                    queryManager.pendingQuery,
                    queryManager.filesQuery,
                    queryManager.duplicatesQuery,
                    out fileManager.duplicates);

                // Log duplicate files
                fileManager.LogDuplicateFiles();

                // Create progress bar and copy physical .pdf files
                services.WindowService.OpenWindow(
                    new ImportViewModel(fileManager.filesToCopy, destination),
                    WindowType.Generic,
                    true,
                    true,
                    true
                    );
            }
            catch (Exception e)
            {
                new BugTracker().Track("App", "Import", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
            }
        }
        public async void Sync(object input = null)
        {
            try
            {
                string destination = services.BrowserService.OpenFolderDialog(services.LastSyncFolderPath);

                if (destination == null)
                    return;

                services.SaveSyncPath(destination);

                if (
                    !Directory.Exists(Path.Combine(destination, "Files")) ||
                    !File.Exists(Path.Combine(destination, "log.json")) ||
                    !File.Exists(Path.Combine(destination, "sync.json"))
                    )
                {
                    services.DialogService.OpenDialog(new DialogOkViewModel("Please select a valid folder", "Sync", DialogType.Error));
                    return;
                }

                services.IsWorking(true);

                SyncInformationManager syncManager = new SyncInformationManager();
                SyncInfo syncInfo = null;
                string stringSync = File.ReadAllText(Path.Combine(destination, "sync.json"));
                Sync sync = JsonConvert.DeserializeObject<Sync>(stringSync);

                await Task.Run(() =>
                {
                    syncInfo = syncManager.Read();
                });

                if (syncInfo.Syncs.FindIndex((el) => el.Name == sync.Name && el.Number == sync.Number) >= 0)
                {
                    services.DialogService.OpenDialog(new DialogOkViewModel("This synchronisation was already imported", "Synchronisation", DialogType.Error));
                    return;
                }

                LogReader reader = new LogReader(Path.Combine(destination, "Files"));

                await Task.Run(() =>
                {
                    reader.GetLogs(destination);
                    reader.Sync();
                });

                // Update sync information
                syncInfo.LastSyncedName = sync.Name;
                syncInfo.LastSyncedNumber = sync.Number;
                syncInfo.Syncs.Add(sync);

                await Task.Run(() =>
                {
                    syncManager.Write(syncInfo);
                    // Write in history that this was synced in the database
                    using (StreamWriter sw = File.AppendText(Path.Combine(destination, "history.txt")))
                    {
                        sw.WriteLine(Properties.Settings.Default.SyncName);
                    }
                });

                services.IsWorking(false);

                // Show messagebox regarding errors
                if (reader.NoErrors)
                {
                    //services.DialogService.OpenDialog(new DialogOkViewModel("Synchronisation successful", "Synchronisation", DialogType.Success));
                    services.ShowNotification("Synchronisation successful", "Synchronisation", NotificationType.Success, "HomeNotificationArea", TimeSpan.MaxValue);
                }
                else
                {
                    //services.DialogService.OpenDialog(new DialogOkViewModel("Some steps couldn't be reproduced, see logs for more information", "Synchronisation", DialogType.Warning));
                    services.ShowNotification("Some steps couldn't be reproduced, see logs for more information.", "Synchronisation", NotificationType.Error, "HomeNotificationArea", TimeSpan.MaxValue);
                }
            }
            catch (Exception e)
            {
                new BugTracker().Track("App", "Sync", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                UpdateSyncInformationDisplay();
                services.IsWorking(false);
            }
        }
        public async void ExportSync(object input = null)
        {
            try
            {
                string destination = services.BrowserService.OpenFolderDialog(services.LastSyncFolderPath);

                if (destination == null)
                    return;

                services.SaveSyncPath(destination);

                // If sync is already exported in destination
                if (Directory.Exists(Path.Combine(destination, "Sync")))
                {
                    //services.DialogService.OpenDialog(
                    //    new DialogOkViewModel(
                    //        "This folder already contains sync information. Please choose different path",
                    //        "Export Sync",
                    //        DialogType.Error)
                    //    );
                    services.ShowNotification("This folder already contains sync information. Please choose different path.", "Export Sync", NotificationType.Error, "HomeNotificationArea", TimeSpan.MaxValue);
                    return;
                }



                // Root sync folder path
                string syncPath = Path.Combine(Environment.CurrentDirectory, "Sync");

                // There is no sync folder in root
                if (!Directory.Exists(syncPath))
                {
                    //services.DialogService.OpenDialog(
                    //    new DialogOkViewModel(
                    //        "No information to export, please restart application.",
                    //        "Export Sync",
                    //        DialogType.Error)
                    //    );
                    services.ShowNotification("No information to export, please restart application.", "Export Sync", NotificationType.Error, "HomeNotificationArea", TimeSpan.MaxValue);
                    return;
                }

                services.IsWorking(true);

                await Task.Run(() =>
                {
                    SyncInformationManager syncManager = new SyncInformationManager();
                    SyncInfo syncInfo = syncManager.Read();
                    syncInfo.LastSyncExportNumber += 1;
                    string syncName = Properties.Settings.Default.SyncName;

                    // Destination sync folder path
                    destination = Path.Combine(destination, $"Sync {syncName} [{syncInfo.LastSyncExportNumber}]");

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

                    // Create export sync info file
                    string infoFilePath = Path.Combine(syncPath, "sync.json");
                    Sync sync = new Sync() { Name = syncName, Number = syncInfo.LastSyncExportNumber };
                    File.WriteAllText(infoFilePath, JsonConvert.SerializeObject(sync));

                    // 1. Create/get backup folder path
                    string backupPath = Path.Combine(Environment.CurrentDirectory, "Backup");

                    if (!Directory.Exists(backupPath))
                        Directory.CreateDirectory(backupPath);

                    // 2. Generate backup name (using current date)
                    string backupName = $"Sync {syncName} [{syncInfo.LastSyncExportNumber}] - " + DateTime.Today.ToLongDateString();
                    backupPath = Path.Combine(backupPath, backupName);

                    // 3. Backup sync
                    string current_time = DateTime.Now.ToLongTimeString();
                    string modified_time = "";
                    foreach (string item in current_time.Split(':'))
                    {
                        modified_time += item + "-";
                    }
                    modified_time = modified_time.Remove(modified_time.Length - 1, 1);
                    backupPath += " (" + modified_time + ")";

                    RelocateFiles(syncPath, backupPath);

                    // 4. Move sync to destination                  
                    RelocateFiles(syncPath, destination, true);

                    // 5. Recreate sync
                    new Tracker(User).init();

                    // Update sync information
                    syncManager.Write(syncInfo);
                });

                UpdateSyncInformationDisplay();

                services.IsWorking(false);

                //services.DialogService.OpenDialog(new DialogOkViewModel("Sync information exported successfully.", "Sync export", DialogType.Success));
                services.ShowNotification("Sync information exported successfully.", "Export Sync", NotificationType.Success, "HomeNotificationArea", new TimeSpan(0, 0, 10));
            }
            catch (Exception e)
            {
                new BugTracker().Track("App", "Export Sync", e.Message, e.StackTrace);
                services.DialogService.OpenDialog(new DialogOkViewModel("Something went wrong.", "Error", DialogType.Error));
            }
            finally
            {
                services.IsWorking(false);
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
        private bool SelectAndCheckFolder(out string result)
        {
            string destination = null;

            // 1. Using winforms dialog box select a folder
            destination = services.BrowserService.OpenFolderDialog(null);

            result = destination;

            // 2. If nothing was selected return
            if (destination == null)
                return false;

            // 3. If the wrong folder was selected return with an error messagebox
            if (IsFolderCorrupt(destination))
            {
                services.DialogService.OpenDialog(new DialogOkViewModel("Please select a correct folder.", "Error", DialogType.Error));
                return false;
            }

            return true;
        }
        private bool IsFolderCorrupt(string destination)
        {
            return (!File.Exists(destination + "\\" + "NikasDB.sqlite3")) ||
                   (!Directory.Exists(destination + @"\Files")) ||
                   (!File.Exists(destination + "\\" + "user.sqlite3"));
        }

        private void UpdateSyncInformationDisplay()
        {
            SyncInfo syncInfo = new SyncInformationManager().Read();
            ExportedSync = syncInfo.LastSyncExportNumber.ToString();
            SyncNameAndNumber = $"{(String.IsNullOrEmpty(syncInfo.LastSyncedName) ? "none" : syncInfo.LastSyncedName)} [{syncInfo.LastSyncedNumber}]";
        }

        private class QueryManager
        {
            private string rootDirectory;
            private string baseQuery;
            private string tempQueryPath;
            private string attachQuery;

            public string filesQuery { get; set; }
            public string duplicatesQuery { get; set; }
            public string pendingQuery { get; set; }

            public QueryManager(string destination)
            {
                this.rootDirectory = Environment.CurrentDirectory;
                this.baseQuery = File.ReadAllText(Path.Combine(rootDirectory, "Query.txt"));
                this.tempQueryPath = Path.Combine(rootDirectory, "temp_query.txt");
                this.attachQuery = $@"
ATTACH DATABASE '{rootDirectory}\user.sqlite3' AS user;
ATTACH DATABASE '{destination}\NikasDB.sqlite3' AS db2main;
ATTACH DATABASE '{destination}\user.sqlite3' AS db2user;
";
                this.filesQuery = "SELECT tf.File AS OldFile, tnf.File AS NewFile FROM temp_Files AS tf, temp_NewFiles AS tnf WHERE tf.Title = tnf.Title;";
                this.duplicatesQuery = "SELECT Title, File FROM temp_tblDuplicates;";
            }

            public void DeleteTempQuery()
            {
                if (File.Exists(tempQueryPath))
                    File.Delete(tempQueryPath);
            }
            public void CreateTempQuery()
            {
                // Open or Create file
                using (StreamWriter sw = File.AppendText(tempQueryPath))
                {
                    sw.WriteLine(attachQuery);
                    sw.WriteLine(baseQuery);
                }
            }
            public void SetPendingQuery(string sectionName)
            {
                this.pendingQuery = $"INSERT INTO tblPending (Article_ID, Section) SELECT ID, \"{sectionName}\" AS Section FROM tblArticle WHERE Title IN (SELECT Title FROM temp_tblArticle);";
            }
            public string GetFullQuery()
            {
                return File.ReadAllText(Path.Combine(rootDirectory, "temp_query.txt"));
            }
        }
        private class SectionManager
        {
            private string info;
            private string rootDirectory;
            List<string> sections;

            public string sectionName { get; set; }

            public SectionManager()
            {
                this.rootDirectory = Environment.CurrentDirectory;
                this.info = File.ReadAllText(Path.Combine(rootDirectory, "sections.json"));
                this.sections = JsonConvert.DeserializeObject<List<string>>(info);
            }

            public void GenerateSectionName()
            {
                if (sections == null || sections.Count == 0)
                    this.sectionName = "Section 1";
                else
                {
                    int number = int.Parse(sections.Last().Split(' ')[1]) + 1;
                    this.sectionName = "Section " + number.ToString();
                }
            }
            public void AddSectionToJsonFile()
            {
                sections.Add(sectionName);
                info = JsonConvert.SerializeObject(sections);
                File.WriteAllText(Path.Combine(rootDirectory, "sections.json"), info);
            }
        }
        private class FileManager
        {
            private DirectoryInfo logs;
            private string logFileName;

            public List<ExistCheck> duplicates;
            public List<CompFile> files;
            public Dictionary<string, string> filesToCopy;

            public FileManager()
            {
                this.filesToCopy = new Dictionary<string, string>();
                this.logs = new DirectoryInfo("." + @"\Logs\");
            }

            public void LogDuplicateFiles()
            {
                GenerateFilesToCopy();
                CreateLogFileName();

                // Current time
                DateTime current_time = DateTime.Now;

                // Open or Create file
                using (StreamWriter sw = File.AppendText(logFileName))
                {
                    // Start writing log starting with current hour as timestamp
                    sw.WriteLine(current_time.ToLongTimeString());
                    sw.WriteLine("Following titles are duplicate:");
                    // Write each file name from mismatch folder into text file
                    foreach (ExistCheck duplicate in duplicates)
                        sw.WriteLine(duplicate.Title + " ||| " + duplicate.File);
                    sw.WriteLine("");
                }
            }

            private void GenerateFilesToCopy()
            {
                // Create dictionary of files to copy
                foreach (CompFile file in files)
                {
                    this.filesToCopy.Add(file.OldFile + ".pdf", file.NewFile + ".pdf");
                }
            }
            private void CreateLogFileName()
            {
                // Current date will be set as file name
                DateTime current_date = DateTime.Today;

                // Full file path including its name
                this.logFileName = logs.FullName + "Merge --- " + current_date.ToLongDateString() + ".txt";
            }
        }
    }
}
