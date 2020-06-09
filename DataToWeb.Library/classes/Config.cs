using System;
using System.Collections.Generic;

namespace DataToWeb.Library
{
    public class Config : IConfig
    {
        private Dictionary<string, object> _configItems { get; set; } = new Dictionary<string, object>();

        public string GetConfigString(string key)
        {
            object val = GetConfigObject(key);
            return (string)val;
        }
        public object GetConfigObject(string key)
        {
            object val = string.Empty;
            bool success = _configItems.TryGetValue(key, out val);
            if (!success)
                throw new ApplicationException($"Unable to read value for config key: {key}");
            return val;
        }
        public void SetConfigItem(string key, object value)
        {
            bool exists = _configItems.ContainsKey(key);
            if (exists)
                _configItems.Remove(key);

            _configItems.Add(key, value);
        }
    }
}
