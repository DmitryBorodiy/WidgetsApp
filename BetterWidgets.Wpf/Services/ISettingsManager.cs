using BetterWidgets.Abstractions;

namespace BetterWidgets.Services
{
    public interface ISettingsManager
    {
        IEnumerable<ISettingsPage> WidgetSettings { get; }
        IEnumerable<ISettingCategory> Categories { get; }

        ISetting GetById(string id);
        IEnumerable<ISetting> Find(string query);

        IEnumerable<ISettingCategory> GetCategories();
        IEnumerable<ISettingsPage> GetWidgetSettings();
    }
}
