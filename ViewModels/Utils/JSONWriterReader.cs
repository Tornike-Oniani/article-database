using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Utils
{
    public class JSONWriterReader<T>
    {
        private string filePath;

        public JSONWriterReader(string filePath)
        {
            this.filePath = filePath;
        }

        public void WriteData(T data)
        {
            string stringifiedData = JsonConvert.SerializeObject(data);
            using (StreamWriter sw = new StreamWriter(filePath, false))
            {
                sw.Write(stringifiedData);
            }
        }
        public T ReadData()
        {
            if (!File.Exists(filePath)) 
            {
                throw new FileNotFoundException("File for search history was not found", filePath);
            }
            string data = File.ReadAllText(filePath);
            if (String.IsNullOrEmpty(data))
            {
                throw new InvalidDataException("No searches were found");
            }
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
