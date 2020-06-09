using System;
using System.Collections.Generic;

namespace DataToWeb.Library
{
    public interface IDataReader<DataType>
    {
        void Read();
        IEnumerable<DataType> GetDataObjects();
    }
}
