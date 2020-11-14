using Lib.DataAccessLayer.Info;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Utils
{
    public class LogConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<Log<IInfo>>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray array = JArray.Load(reader);
            List<Log<IInfo>> result = new List<Log<IInfo>>();

            foreach (var item in array.Children())
            {
                string check = item.ToString();

                if (check.Contains("ArticleInfo"))
                {
                    Log<ArticleInfo> temp = item.ToObject<Log<ArticleInfo>>();
                    Log<IInfo> log = new Log<IInfo>(temp.Type, temp.Username, temp.Info, temp.Changed);
                    result.Add(log);
                }
                else if (check.Contains("BookmarkInfo"))
                {
                    Log<BookmarkInfo> temp = item.ToObject<Log<BookmarkInfo>>();
                    Log<IInfo> log = new Log<IInfo>(temp.Type, temp.Username, temp.Info, temp.Changed);
                    result.Add(log);
                }
                else if (check.Contains("ReferenceInfo"))
                {
                    Log<ReferenceInfo> temp = item.ToObject<Log<ReferenceInfo>>();
                    Log<IInfo> log = new Log<IInfo>(temp.Type, temp.Username, temp.Info, temp.Changed);
                    result.Add(log);
                }
                else if (check.Contains("Couple"))
                {
                    Log<Couple> temp = item.ToObject<Log<Couple>>();
                    Log<IInfo> log = new Log<IInfo>(temp.Type, temp.Username, temp.Info, temp.Changed);
                    result.Add(log);
                }
                else if (check.Contains("DeleteInfo"))
                {
                    Log<DeleteInfo> temp = item.ToObject<Log<DeleteInfo>>();
                    Log<IInfo> log = new Log<IInfo>(temp.Type, temp.Username, temp.Info, temp.Changed);
                    result.Add(log);
                }
                else if (check.Contains("Personal"))
                {
                    Log<PersonalInfo> temp = item.ToObject<Log<PersonalInfo>>();
                    Log<IInfo> log = new Log<IInfo>(temp.Type, temp.Username, temp.Info, temp.Changed);
                    result.Add(log);
                }
                else if (check.Contains("Pending"))
                {
                    Log<PendingInfo> temp = item.ToObject<Log<PendingInfo>>();
                    Log<IInfo> log = new Log<IInfo>(temp.Type, temp.Username, temp.Info, temp.Changed);
                    result.Add(log);
                }
            }

            return result;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
