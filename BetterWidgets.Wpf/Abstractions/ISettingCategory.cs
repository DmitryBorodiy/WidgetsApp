using System.Collections.ObjectModel;

namespace BetterWidgets.Abstractions
{
    public interface ISettingCategory : ISetting
    {
        ObservableCollection<ISetting> Settings { get; set; }

        void FindSetting(string id);
    }
}
