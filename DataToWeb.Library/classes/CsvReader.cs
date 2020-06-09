using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataToWeb.Library
{
    public class CsvInputReader<DataType> : IDataReader<DataType>
    {
        private IConfig _config { get; set; }
        private List<DataType> ReadData { get; set; }

        public CsvInputReader(IConfig config)
        {
            _config = config;
        }
        public void Read()
        {
            using (TextReader fileReader = File.OpenText(_config.GetConfigString("FileName")))
            {
                var csv = new CsvReader(fileReader, System.Globalization.CultureInfo.CurrentCulture);
                csv.Configuration.HasHeaderRecord = Convert.ToBoolean(_config.GetConfigString("HasHeaderRecord"));
                csv.Read();
                ReadData = csv.GetRecords<DataType>().ToList();
            }
        }
        public IEnumerable<DataType> GetDataObjects()
        {
            return ReadData;
        }
    }
}
