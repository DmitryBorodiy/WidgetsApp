using BetterWidgets.Abstractions;
using BetterWidgets.Enums;
using BetterWidgets.Model;
using BetterWidgets.Properties;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Media;

namespace BetterWidgets.ViewModel.Components
{
    public partial class AppointmentViewModel : ObservableObject
    {
        #region Services
        private readonly Settings<CalendarWidget> _settings;
        #endregion

        public AppointmentViewModel()
        {
            _settings = App.Services?.GetService<Settings<CalendarWidget>>();
        }

        public AppointmentViewModel(ICalendarAppointment appointment, string timeFormat = null, string dateFormat = null)
        {
            _settings = App.Services?.GetService<Settings<CalendarWidget>>();

            Id = appointment.Id;
            Appointment = appointment as CalendarAppointment;
            TitleLabel = appointment.Subject;
            WebLink = appointment.WebLink;
            Location = appointment.Location;
            IsAllDay = appointment.IsAllDay ?? false;

            StartDate = appointment.Start ?? DateTime.Now;
            EndDate = appointment.End ?? DateTime.Now;

            StartTimeLabel = IsAllDay ? Resources.Resources.AllDay :
                             !string.IsNullOrEmpty(timeFormat) ? 
                             StartDate.Day == EndDate.Day ? 
                             StartDate.ToString(timeFormat) : StartDate.ToString($"{timeFormat}, {dateFormat}")
                             : StartDate.ToString();

            EndTimeLabel = IsAllDay ? string.Empty :
                           !string.IsNullOrEmpty(timeFormat) ?
                           StartDate.Day == EndDate.Day ?
                           appointment.End?.ToString(timeFormat) : EndDate.ToString($"{timeFormat}, {dateFormat}")
                           : EndDate.ToString();

            StatusColor = GetStatusBrush(appointment.CalendarStatus);
        }

        #region Fields

        public Color? FreeColor => GetSettingsColor(nameof(FreeColor));
        public Color? TentativeColor => GetSettingsColor(nameof(TentativeColor));
        public Color? BusyColor => GetSettingsColor(nameof(BusyColor));
        public Color? OofColor => GetSettingsColor(nameof(OofColor));
        public Color? WorkingelsewhereColor => GetSettingsColor(nameof(WorkingelsewhereColor));

        #endregion

        #region Props

        public string Id { get; set; }
        public CalendarAppointment Appointment { get; set; }

        [ObservableProperty]
        public string titleLabel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsTimeVisible))]
        public string startTimeLabel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsTimeVisible))]
        public string endTimeLabel;

        [ObservableProperty]
        public string location;

        [ObservableProperty]
        public SolidColorBrush statusColor;

        [ObservableProperty]
        public string webLink;

        [ObservableProperty]
        public bool isAllDay;

        [ObservableProperty]
        public DateTime startDate = DateTime.Now;

        [ObservableProperty]
        public DateTime endDate = DateTime.Now;

        [ObservableProperty]
        public string body;

        public bool IsTimeVisible => !string.IsNullOrEmpty(StartTimeLabel) ||
                                     !string.IsNullOrEmpty(EndTimeLabel);

        #endregion

        #region Utils

        private Color? GetSettingsColor(string key)
        {
            if(_settings == null) return null;
            if(!_settings.HasSetting(key)) return null;

            string hex = _settings.GetSetting<string>(key);

            return (Color)ColorConverter.ConvertFromString(hex);
        }

        private SolidColorBrush GetStatusBrush(AppointmentStatus status)
        {
            Color color;

            switch(status)
            {
                case AppointmentStatus.Free:
                    color = FreeColor.HasValue ? FreeColor.Value :
                            (Color)Application.Current.Resources["PaletteGreenColor"];
                    break;
                case AppointmentStatus.Busy:
                    color = BusyColor.HasValue ? BusyColor.Value : 
                            (Color)Application.Current.Resources["PaletteRedColor"];
                    break;
                case AppointmentStatus.Tentative:
                    color = TentativeColor.HasValue ? TentativeColor.Value : 
                            (Color)Application.Current.Resources["PalettePurpleColor"];
                    break;
                case AppointmentStatus.WorkingElsewhere:
                    color = WorkingelsewhereColor.HasValue ? WorkingelsewhereColor.Value : 
                            (Color)Application.Current.Resources["PaletteAmberColor"];
                    break;
                case AppointmentStatus.Oof:
                    color = OofColor.HasValue ? OofColor.Value : 
                            (Color)Application.Current.Resources["PaletteIndigoColor"];
                    break;
            }

            return new SolidColorBrush(color);
        }

        #endregion
    }
}
