using BetterWidgets.Abstractions;
using BetterWidgets.Properties;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.ViewModel.Dialogs
{
    public partial class CalendarPickerViewModel : ObservableObject
    {
        #region Services
        private readonly Settings<CalendarWidget> _settings;
        #endregion

        public CalendarPickerViewModel()
        {
            _settings = App.Services?.GetService<Settings<CalendarWidget>>();
        
            if(_settings != null)
               _settings.ValueChanged += OnSettingsValueChanged;
        }

        #region Props

        public IWidget Widget { get; set; }
        public int DayOfWeekSetting => _settings?.GetSetting(nameof(DayOfWeekSetting), 0) ?? 0;

        public DayOfWeek DayOfWeek => (DayOfWeek)DayOfWeekSetting;

        [ObservableProperty]
        public DateTime selectedDateTime = DateTime.Now;

        #endregion

        #region Commands

        [RelayCommand]
        private void SelectToday()
        {
            SelectedDateTime = DateTime.Now;

            Widget?.HideContentDialog(true);
        }

        #endregion

        #region EventHandlers

        private void OnSettingsValueChanged(object sender, string e)
        {
            if(nameof(DayOfWeekSetting) == e) OnPropertyChanged(nameof(DayOfWeek));
        }

        #endregion
    }
}
