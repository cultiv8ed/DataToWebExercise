using System;

namespace DataToWeb.Library
{
    public interface IConfig
    {
        string GetConfigString(string key);
        object GetConfigObject(string key);
        void SetConfigItem(string key, object value);
    }
}
