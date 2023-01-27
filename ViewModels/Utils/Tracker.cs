using Lib.DataAccessLayer.Info;
using Lib.DataAccessLayer.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace MainLib.ViewModels.Utils
{
    public class Log<T>
    {
        private string _type;

        // [Create, Update, Delete, Coupling, Personal, Pending, Abstract]
        public string Type
        {
            get { return _type; }
            set
            {
                if (value.ToString() == "Create"
                    || value.ToString() == "Update"
                    || value.ToString() == "Delete"
                    || value.ToString() == "Coupling"
                    || value.ToString() == "Personal"
                    || value.ToString() == "Pending"
                    || value.ToString() == "Abstract")
                    _type = value;
                else
                    throw new ArgumentException("Invalid argument. It has to be Create, Update, Delete, Coupling, Personal, Pending or Abstract");
            }
        }
        public string Username { get; set; }
        public string Changed { get; set; }
        public T Info { get; set; }

        public Log(string type, string username, T info, string changed = null)
        {
            this.Type = type;
            this.Username = username;
            this.Info = info;
            this.Changed = changed;
        }
    }

    public class Tracker
    {
        private User _user;
        private string syncPath;
        private string logPath;

        public Tracker(User user)
        {
            this._user = user;
            syncPath = Path.Combine(Environment.CurrentDirectory, "Sync");
            logPath = Path.Combine(syncPath, "log.json");
        }

        public string GetFilesPath()
        {
            return Path.Combine(syncPath, "Files");
        }

        // Create necessary files and folders for tracking
        public void init()
        {
            // 1. Create sync directory if it doesn't exist
            if (!Directory.Exists(syncPath))
            {
                Directory.CreateDirectory(syncPath);
                Directory.CreateDirectory(Path.Combine(syncPath, "Files"));
                File.Create(logPath);
            }
        }

        // Track creation of Article, Bookmark or Reference (TODO: User)
        public void TrackCreate<T>(T instance)
        {
            Track<T>("Create", instance);
        }

        public void TrackUpdate<T>(T instance, string id)
        {
            Track<T>("Update", instance, id);
        }

        public void TrackCoupling<T>(T instance)
        {
            Track<T>("Coupling", instance);
        }

        public void TrackDelete(string objectType, string name)
        {
            DeleteInfo info = new DeleteInfo(objectType, name);
            Track<DeleteInfo>("Delete", info);
        }

        public void TrackPersonal(PersonalInfo info)
        {
            Track<PersonalInfo>("Personal", info);
        }

        public void TrackPending(PendingInfo info)
        {
            Track<PendingInfo>("Pending", info);
        }

        private void Track<T>(string action, T instance, string id = null)
        {
            // If user is not admin, don't track
            if (!_user.IsAdmin)
                return;

            Log<T> log = new Log<T>(action, _user.Username, instance, id);
            string info = JsonConvert.SerializeObject(log);
            using (StreamWriter sw = new StreamWriter(logPath, true))
            {
                sw.WriteLine(info + ",");
            }
        }
    }
}
