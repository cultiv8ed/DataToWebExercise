using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DataToWeb.Library
{
    public class XmlWriter<InputType> : IDataWriter<InputType>
    {
        private IConfig _config { get; set; }

        public XmlWriter(IConfig config)
        {
            _config = config;
        }

        public string OutputAsString(IEnumerable<InputType> dataCollection)
        {
            string result = SerializeData(dataCollection);
            return result;
        }

        public void OutputAsFile(IEnumerable<InputType> dataCollection)
        {
            string result = OutputAsString(dataCollection);
            File.WriteAllLines(_config.GetConfigString("FileName"), new string[] { result });
        }

        private string SerializeData(IEnumerable<InputType> dataCollection)
        {
            string result = string.Empty;
            XmlSerializer xml = new XmlSerializer(typeof(List<InputType>));
            StringWriter stringWriter = new StringWriter();
            xml.Serialize(stringWriter, dataCollection.ToList());
            return stringWriter.ToString();
        }
    }
}
