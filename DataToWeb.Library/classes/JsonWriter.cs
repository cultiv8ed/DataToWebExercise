using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataToWeb.Library
{
    public class JsonWriter<InputType> : IDataWriter<InputType>
    {
        private IConfig _config { get; set; }

        public JsonWriter(IConfig config)
        {
            _config = config;
        }

        public string OutputAsString(IEnumerable<InputType> dataCollection)
        {
            string result = SerializeData(dataCollection);
            bool RemoveQuotes = Convert.ToBoolean(_config.GetConfigObject("RemoveQuotes"));
            
            if (RemoveQuotes)
                result = result.Replace("\"", "");

            return result;
        }

        public void OutputAsFile(IEnumerable<InputType> dataCollection)
        {
            string result = OutputAsString(dataCollection);
            File.WriteAllLines(_config.GetConfigString("FileName"),new string[] { result });
        }

        private string SerializeData(IEnumerable<InputType> dataCollection)
        {
            string result = string.Empty;
            bool RemoveArrayIfSingleRecord = Convert.ToBoolean(_config.GetConfigObject("RemoveArrayIfSingleRecord"));
            Formatting Format = (Formatting)_config.GetConfigObject("JsonFormatting");

            if (RemoveArrayIfSingleRecord && dataCollection.Count() == 1)
                result = JsonConvert.SerializeObject(dataCollection.FirstOrDefault(), Format);
            else
                result = JsonConvert.SerializeObject(dataCollection, Format);
            return result;
        }
    }
}
