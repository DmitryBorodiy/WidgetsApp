using Windows.Storage;
using Microsoft.Extensions.Logging;
using BetterWidgets.Consts;

namespace BetterWidgets.Properties
{
    public abstract class SettingsBase
    {
        #region Services
        private readonly ILogger Logger;
        private readonly ApplicationDataContainer Data;
        #endregion

        public SettingsBase(ILogger<SettingsBase> logger)
        {
            Logger = logger;

            Data = GetDataContainer();
            Root = GetRoot();
        }

        #region Events
        public event EventHandler<string> ValueChanged;
        #endregion

        #region Utils

        private IDictionary<string, object> Root { get; set; }

        private ApplicationDataContainer GetDataContainer()
        {
            try
            {
                return ApplicationData.Current.LocalSettings;
            }
            catch { return null; }
        }

        private IDictionary<string, object> GetRoot()
        {
            if(Data != null)
               return Data.Values;
            else
               return new Dictionary<string, object>();
        }

        public bool ContainsKey(string key) => Root?.ContainsKey(key) ?? false;

        public void OnValueChanged(string key) => ValueChanged?.Invoke(this, key);

        public T GetValue<T>(string key, T defaultValue = default)
        {
            try
            {
                if(string.IsNullOrEmpty(key)) throw new ArgumentNullException(string.Format(Errors.SettingKeyIsEmpty, key));
                if(!ContainsKey(key)) return defaultValue;

                return (T)Root[key];
            }
            catch(Exception ex)
            {
                Logger?.LogError(ex, ex.Message, ex.StackTrace);

                return defaultValue;
            }
        }

        public void SetValue<T>(string key, T value = default, bool throwEvent = true)
        {
            try
            {
                if(string.IsNullOrEmpty(key)) throw new ArgumentNullException(string.Format(Errors.SettingKeyIsEmpty, key));

                Root[key] = value;

                if(throwEvent) ValueChanged?.Invoke(this, key);
            }
            catch(Exception ex)
            {
                Logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        public bool RemoveKey(string key)
        {
            if(ContainsKey(key)) return Root.Remove(key);
            else return false;
        }

        public T GetWidgetValue<T>(Guid widgetId, string key, T defaultValue = default) => GetValue<T>($"{widgetId}:{key}", defaultValue);

        public void SetWidgetValue<T>(Guid widgetId, string key, T value = default)
        {
            SetValue($"{widgetId}:{key}", value, false);
            ValueChanged?.Invoke(this, key);
        }

        #endregion
    }
}
