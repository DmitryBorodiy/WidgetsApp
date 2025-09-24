using Microsoft.Extensions.Logging;

namespace BetterWidgets.Properties
{
    public class Settings<T> : SettingsBase
    {
        private readonly string _prefix = string.Empty;

        public Settings(ILogger<Settings<T>> logger) : base(logger)
        {
            _prefix = typeof(T).Name;
        }

        public bool HasSetting(string key) => ContainsKey($"{_prefix}:{key}");

        public TSetting GetSetting<TSetting>(string key, TSetting defaultValue = default)
        {
            return GetValue($"{_prefix}:{key}", defaultValue);
        }

        public void SetSetting<TSetting>(string key, TSetting value)
        {
            SetValue($"{_prefix}:{key}", value, false);
            OnValueChanged(key);
        }
    }
}
