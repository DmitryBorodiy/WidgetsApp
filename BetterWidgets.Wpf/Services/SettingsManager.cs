using System.Reflection;
using BetterWidgets.Abstractions;
using BetterWidgets.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace BetterWidgets.Services
{
    public partial class SettingsManager : ObservableObject, ISettingsManager
    {
        private readonly ILogger _logger;

        public SettingsManager(ILogger<SettingsManager> logger)
        {
            _logger = logger;

            LoadSettings();
        }

        #region Props

        [ObservableProperty]
        public IEnumerable<ISettingCategory> categories;

        [ObservableProperty]
        public IEnumerable<ISettingsPage> widgetSettings;

        #endregion

        #region Methods

        private void LoadSettings()
        {
            if(Categories == null)
               Categories = GetCategories();

            if(WidgetSettings == null)
               WidgetSettings = GetWidgetSettings();
        }

        public ISetting GetById(string id)
        {
            var category = Categories?.FirstOrDefault(c => c.Id == id);

            if(category != null) return category;

            var setting = Categories?
                .SelectMany(p => p.Settings)
                .FirstOrDefault(s => s.Id == id);

            if(setting != null) return setting;

            var widgetSetting = WidgetSettings?.FirstOrDefault(s => s.Id == id);

            if(widgetSetting != null) return widgetSetting;

            return null;
        }

        public IEnumerable<ISetting> Find(string query)
        {
            try
            {
                if(Categories == null) Categories = GetCategories();
                if(WidgetSettings == null) WidgetSettings = GetWidgetSettings();

                var results = new List<ISetting>();

                var settings = Categories?
                    .SelectMany(p => p.Settings)
                    .Where(s => (s.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                (s.Subtitle?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
                var widgetSettings = WidgetSettings?
                    .SelectMany(s => s.Settings)
                    .Where(s => (s.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                (s.Subtitle?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));

                if(settings != null && settings.Any()) results.AddRange(settings);
                if(widgetSettings != null && widgetSettings.Any()) results.AddRange(widgetSettings);

                if(results.Any()) return results;
                else return Enumerable.Empty<ISetting>();
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return Enumerable.Empty<ISetting>();
            }
        }

        public IEnumerable<ISettingCategory> GetCategories()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly?.GetTypes();
            var categories = types?.Where(t => t.IsSubclassOf(typeof(SettingCategory)) &&
                                               !t.IsAbstract && !t.IsInterface);

            foreach(var category in categories)
            {
                ISettingCategory categoryInstance = null;

                try
                {
                    categoryInstance
                        = (ISettingCategory)Activator.CreateInstance(category);
                }
                catch(Exception ex)
                {
                    _logger?.LogError(ex, ex.Message, ex.StackTrace);
                }

                yield return categoryInstance;
            }
        }

        public IEnumerable<ISettingsPage> GetWidgetSettings()
        {
            var assebly = Assembly.GetExecutingAssembly();
            var types = assebly?.GetTypes();
            var widgetSettings = types?.Where(t => t.IsSubclassOf(typeof(WidgetSettingsView)) &&
                                                   !t.IsInterface && !t.IsAbstract);

            foreach(var type in widgetSettings)
            {
                WidgetSettingsView widgetInstance = null;

                try
                {
                    widgetInstance
                        = (WidgetSettingsView)Activator.CreateInstance(type);
                }
                catch(Exception ex)
                {
                    _logger?.LogError(ex, ex.Message, ex.StackTrace);
                }

                yield return widgetInstance;
            }
        }

        #endregion
    }
}
