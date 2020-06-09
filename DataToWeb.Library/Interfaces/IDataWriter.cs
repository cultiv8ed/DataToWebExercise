using System;
using System.Collections.Generic;

namespace DataToWeb.Library
{
    public interface IDataWriter<InputType>
    {
         string OutputAsString(IEnumerable<InputType> dataCollection);
         void OutputAsFile(IEnumerable<InputType> dataCollection);
    }
}
