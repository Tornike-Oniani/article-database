using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Utils
{
    public class Sync
    {
        public string Name { get; set; }
        public int Number { get; set; }
    }

    public class SyncInfo
    {
        public int LastSyncExportNumber { get; set; }
        public string LastSyncedName { get; set; }
        public int LastSyncedNumber { get; set; }
        public List<Sync> Syncs { get; set; }
    }

    public class SyncInformationManager
    {
        private string syncInfoPath;

        public SyncInformationManager()
        {
            syncInfoPath = Path.Combine(Environment.CurrentDirectory, "syncInformation.json");
        }

        public SyncInfo Read()
        {
            string info = File.ReadAllText(syncInfoPath);
            return JsonConvert.DeserializeObject<SyncInfo>(info);
        }
        public void Write(SyncInfo info)
        {
            string infoToString = JsonConvert.SerializeObject(info);
            File.WriteAllText(syncInfoPath, infoToString);
        }
    }
}
