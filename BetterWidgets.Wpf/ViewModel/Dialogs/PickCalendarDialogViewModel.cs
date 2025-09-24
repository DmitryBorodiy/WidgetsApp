using BetterWidgets.Model;
using BetterWidgets.Widgets;
using BetterWidgets.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using BetterWidgets.Properties;
using BetterWidgets.Helpers;
using BetterWidgets.ViewModel.Widgets;

namespace BetterWidgets.ViewModel.Dialogs
{
    public partial class PickCalendarDialogViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings<CalendarWidget> _settings;
        private readonly ICalendarService<CalendarWidget> _calendar;
        #endregion

        public PickCalendarDialogViewModel()
        {
            _settings = App.Services?.GetService<Settings<CalendarWidget>>();
            _calendar = App.Services?.GetService<ICalendarService<CalendarWidget>>();
            _logger = App.Services?.GetRequiredService<ILogger<PickCalendarDialogViewModel>>();
        }

        #region Fields
        private readonly string calendarsCache = nameof(calendarsCache);
        #endregion

        #region Props

        public string CalendarId
        {
            get => _settings.GetSetting<string>(nameof(CalendarId));
            set => _settings.SetSetting(nameof(CalendarId), value);
        }

        private Calendar selectedCalendar;
        public Calendar SelectedCalendar
        {
            get => selectedCalendar;
            set
            {
                CalendarId = value.Id;
                SetProperty(ref selectedCalendar, value, nameof(SelectedCalendar));
            }
        }

        [ObservableProperty]
        public ObservableCollection<Calendar> calendars;

        #endregion

        #region Commands

        [RelayCommand]
        private async Task UpdateAsync(bool fetchData)
        {
            Calendars = await GetCalendarsAsync(fetchData);

            SetProperty(ref selectedCalendar,
                        string.IsNullOrEmpty(CalendarId) ?
                        Calendars.FirstOrDefault() :
                        Calendars.FirstOrDefault(c => c.Id == CalendarId),
                        nameof(SelectedCalendar));
        }

        [RelayCommand]
        private void SelectCalendar(ObservableObject vm)
        {
            if(vm is CalendarWidgetViewModel calendarVM)
               calendarVM.SelectedCalendar = SelectedCalendar;
        }

        #endregion

        #region Utils

        private async Task<ObservableCollection<Calendar>> GetCalendarsAsync(bool fetchData)
        {
            try
            {
                var data = await GetAsync(fetchData);

                if(data.ex != null) throw data.ex;

                return new ObservableCollection<Calendar>(data.calendars);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        private async Task<(IEnumerable<Calendar> calendars, Exception ex)> GetAsync(bool fetchData) => await _calendar.GetCachedAsync
        (
            calendarsCache,
            _calendar.GetAllCalendarsAsync,
            fetchData && NetworkHelper.IsConnected
        );

        #endregion
    }
}
