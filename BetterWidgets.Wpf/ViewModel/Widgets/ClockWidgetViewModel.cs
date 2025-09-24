using BetterWidgets.Behaviours.Validators;
using BetterWidgets.Controls;
using BetterWidgets.Enums;
using BetterWidgets.Helpers;
using BetterWidgets.Model;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.ViewModel.Components;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Windows.Storage;
using Windows.System;
using Wpf.Ui.Controls;

namespace BetterWidgets.ViewModel.Widgets
{
    public partial class ClockWidgetViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger<ClockWidget> _logger;
        private readonly Settings<ClockWidget> _settings;
        private readonly ITimeService<ClockWidget> _timeService;
        private readonly DataService<ClockWidget> _data;
        private readonly ICalendarService<ClockWidget> _msCalendar;
        private readonly IMSGraphService _graph;
        private readonly IShareService _share;
        private readonly Settings _mainSettings;
        #endregion

        public ClockWidgetViewModel()
        {
            _mainSettings = App.Services?.GetService<Settings>();
            _settings = App.Services?.GetService<Settings<ClockWidget>>();
            _logger = App.Services?.GetRequiredService<ILogger<ClockWidget>>();
            _timeService = App.Services?.GetService<ITimeService<ClockWidget>>();
            _data = App.Services?.GetService<DataService<ClockWidget>>();
            _msCalendar = App.Services?.GetService<ICalendarService<ClockWidget>>();
            _graph = App.Services?.GetService<IMSGraphService>();
            _share = App.Services?.GetService<IShareService>();

            if(_settings != null)
               _settings.ValueChanged += OnSettingsValueChanged;

            if(_timeService != null)
               _timeService.TimeZonesChanged += OnTimeZonesChanged;

            if(_graph != null)
               _graph.SignedIn += OnGraphSignedIn;
        }

        #region Fields
        private DispatcherTimer _timer;
        #endregion

        #region Props

        [ObservableProperty]
        public Widget widget;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Time))]
        [NotifyPropertyChangedFor(nameof(Date))]
        public DateTime dateTime;

        public string Time => GetDateTime(TimeFormat);
        public string Date => GetDateTime(DateFormat);

        public FontFamily DefaultFont => EnableCustomFont ?
                                         new FontFamily(DefaultFontFamily) : SystemFonts.MessageFontFamily;

        [ObservableProperty]
        public DropShadowEffect shadow;

        [ObservableProperty]
        public string timeFormat = TimeFormatValidator.Default;

        [ObservableProperty]
        public string dateFormat = DateFormatValidator.Default;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasTimeZones))]
        [NotifyPropertyChangedFor(nameof(HasNextTimeZone))]
        [NotifyPropertyChangedFor(nameof(HasPreviousTimeZone))]
        public ObservableCollection<TimeZoneModel> timeZonesCollection;

        private TimeZoneModel selectedTimeZone;
        public TimeZoneModel SelectedTimeZone
        {
            get => selectedTimeZone;
            set => SetSelectedTimeZone(value);
        }

        [ObservableProperty]
        public string timeZoneTitle;

        [ObservableProperty]
        public bool isLocalTimezone;

        [ObservableProperty]
        public bool hasNextTimeZone;

        [ObservableProperty]
        public bool hasPreviousTimeZone;

        [ObservableProperty]
        public bool isClockFaceEnabled = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowAppointment), nameof(CanShare))]
        public AppointmentViewModel appointment;

        public bool EnableCustomFont => _settings.GetSetting(nameof(EnableCustomFont), false);
        public string DefaultFontFamily => _settings.GetSetting(nameof(DefaultFontFamily), "Segoe UI");

        public bool ShowAppointment => Appointment != null && IsAppointmentsEnabled;

        public bool HasTimeZones => TimeZonesCollection?.Count > 0;
        public bool IsAnalogClockVisible => WidgetMode == ClockWidgetMode.AnalogClock;
        public bool IsDigitalClockVisible => WidgetMode == ClockWidgetMode.DigitalClock;

        public bool IsBackgroundDisabled => _settings?.GetSetting<bool>(nameof(IsBackgroundDisabled), false) ?? false;
        public bool IsAppointmentsEnabled => _settings?.GetSetting<bool>(nameof(IsAppointmentsEnabled), false) ?? false;
        public bool IsShadowEnabled => _settings?.GetSetting<bool>(nameof(IsShadowEnabled), false) ?? false;
        public bool IsDateEnabled => _settings?.GetSetting<bool>(nameof(IsDateEnabled), true) ?? true;
        public ClockWidgetMode WidgetMode => (ClockWidgetMode)_settings?.GetSetting<int>(nameof(WidgetMode), 0);
        public string CustomClockFaceFileName
            => _settings?.GetSetting<string>(nameof(CustomClockFaceFileName), string.Empty);

        public bool CanShare => Appointment != null;

        #endregion

        #region Utils

        private DispatcherTimer CreateTimer() => new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(1),
            IsEnabled = true
        };

        private void LoadFormats()
        {
            DateFormat = _settings?.GetSetting<string>(nameof(DateFormat), DateFormatValidator.Default);
            TimeFormat = _settings?.GetSetting<string>(nameof(TimeFormat), TimeFormatValidator.Default);
        }

        private string GetDateTime(string format)
            => DateTime.ToString(format, CultureInfo.CurrentCulture);

        private void SetBackgroundDisabled(bool value)
        {
            Widget.IsBlurEnabled = !value;
            Widget.BackdropOpacity = value ? 0 : _mainSettings.WidgetTransparency;
            Widget.CornerMode = value ?
                                WidgetCornerMode.DoNotRound : WidgetCornerMode.Round;

            OnPropertyChanged(nameof(IsBackgroundDisabled));
        }

        private DropShadowEffect _shadow;
        private DropShadowEffect GetShadow(bool enable)
        {
            if(_shadow == null) _shadow = new DropShadowEffect()
            {
                BlurRadius = 5,
                Direction = 260,
                Opacity = 0.3,
                ShadowDepth = 1,
                Color = Colors.Black
            };

            return enable ? _shadow : null;
        }

        private TimeZoneModel GetSettingsTimeZone()
        {
            string name = _settings?.GetSetting(nameof(SelectedTimeZone), TimeZoneInfo.Local.DisplayName);
            var timezone = _timeService?.GetTimeZoneByName(name);

            return TimeZonesCollection?.FirstOrDefault(t => t.TimeZone.DisplayName == name) ?? new TimeZoneModel(TimeZoneInfo.Local);
        }

        private string GetTimeZoneTitle(TimeZoneModel timeZone)
        {
            if(timeZone == null) return string.Empty;
            if(timeZone.TimeZone.DisplayName == TimeZoneInfo.Local.DisplayName) return string.Empty;

            return Regex.Replace(timeZone.TimeZone.DisplayName, @"\([^)]*\)", string.Empty);
        }

        private void SetSelectedTimeZone(TimeZoneModel value, bool isSave = true)
        {
            if(value == null) return;
            if(selectedTimeZone == value) return;

            selectedTimeZone = value;

            TimeZoneTitle = GetTimeZoneTitle(value);
            HasNextTimeZone = TimeZonesCollection?.IndexOf(value) < TimeZonesCollection?.Count - 1;
            HasPreviousTimeZone = TimeZonesCollection?.IndexOf(value) >= 0;
            IsLocalTimezone = (SelectedTimeZone?.TimeZone ?? TimeZoneInfo.Local) == TimeZoneInfo.Local;
            DateTime = TimeZoneInfo.ConvertTime(DateTime.Now, value.TimeZone);

            if(isSave) _settings?.SetSetting(nameof(SelectedTimeZone), value.TimeZone.DisplayName);
        }

        #endregion

        #region Tasks

        [RelayCommand]
        private async Task OnAppearedAsync(Widget widget)
        {
            Widget = widget;
            
            _timer = CreateTimer();
            _timer.Tick += OnTimerTick;

            LoadFormats();
            SetBackgroundDisabled(IsBackgroundDisabled);
            Shadow = GetShadow(IsShadowEnabled);
            IsClockFaceEnabled = _settings?.GetSetting<bool>(nameof(IsClockFaceEnabled), true) ?? true;
            TimeZonesCollection = new ObservableCollection<TimeZoneModel>(await _timeService.GetAllUsingTimezonesAsync());
            SelectedTimeZone = GetSettingsTimeZone();
            Appointment = await GetMainAppointmentAsync();
        }

        [RelayCommand]
        private void OnPinned()
        {
            _timer?.Start();
        }

        [RelayCommand]
        private void OnUnpin()
        {
            _timer?.Stop();
        }

        [RelayCommand]
        private void OnExecutionStateChanged(WidgetState state)
        {
            if(state == WidgetState.Activated) _timer?.Start();
            else _timer?.Stop();
        }

        [RelayCommand]
        private void OnMouseEnter()
        {
            if(IsBackgroundDisabled)
               SetBackgroundDisabled(!IsBackgroundDisabled);
        }

        [RelayCommand]
        private void OnMouseLeave()
        {
            if(IsBackgroundDisabled)
               SetBackgroundDisabled(IsBackgroundDisabled);
        }

        [RelayCommand]
        private async Task LaunchUriAsync(string uri)
        {
            if(Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute))
               await Launcher.LaunchUriAsync(new Uri(uri));
        }

        [RelayCommand]
        private void Close() => Widget?.UnpinDesktop();

        [RelayCommand]
        private void NextTimeZone()
        {
            int? index = TimeZonesCollection?.IndexOf(SelectedTimeZone) + 1;

            if(!index.HasValue) return;
            if(index == TimeZonesCollection.Count) return;

            SelectedTimeZone = TimeZonesCollection[index.Value];
        }

        [RelayCommand]
        private void PreviousTimeZone()
        {
            int? index = TimeZonesCollection?.IndexOf(SelectedTimeZone) - 1;

            if(!index.HasValue) return;
            if(index == -1)
            {
                SelectedTimeZone = new TimeZoneModel(TimeZoneInfo.Local);

                return;
            }

            SelectedTimeZone = TimeZonesCollection[index.Value];
        }

        [RelayCommand]
        private void LaunchSettings() => ShellHelper.LaunchSettingsById(Widget.Id);

        [RelayCommand]
        private async Task LaunchAppointmentAsync(AppointmentViewModel appointment)
        {
            if(appointment == null) return;
            if(string.IsNullOrEmpty(appointment.WebLink)) return;

            await Launcher.LaunchUriAsync(new Uri(appointment.WebLink));
        }

        [RelayCommand]
        private async Task ShareAsync()
        {
            if(!CanShare) return;

            var serializationResult = _msCalendar.SerializeIcs(new CalendarAppointment()
            {
                Subject = Appointment.TitleLabel,
                Start = Appointment.StartDate,
                End = Appointment.EndDate,
                BodyPreview = Appointment.Body,
                IsAllDay = Appointment.IsAllDay,
                Location = Appointment.Location,
                WebLink = Appointment.WebLink,
                Id = Appointment.Id
            });

            if(serializationResult.ex != null ||
               serializationResult.serialized == null)
            {
                Widget?.ShowNotify(
                    title: Resources.Resources.CantShare,
                    message: Resources.Resources.CantShareSubtitle + serializationResult.ex?.Message,
                    isClosable: true,
                    severity: InfoBarSeverity.Warning);
            }

            var local = ApplicationData.Current.LocalFolder;
            var calendarFile = await local.CreateFileAsync
            (
                $"{Appointment.TitleLabel}.ics",
                CreationCollisionOption.ReplaceExisting
            );

            await FileIO.WriteTextAsync(calendarFile, serializationResult.serialized);

            _share.RequestShare(calendarFile, Widget, Appointment.TitleLabel, Appointment.Body);
        }

        private AppointmentViewModel CreatePreviewAppointment() => new AppointmentViewModel()
        {
            TitleLabel = Resources.Resources.AppointmentSampleTitle,
            StartTimeLabel = Resources.Resources.AppointmentSampleStartTime,
            EndTimeLabel = Resources.Resources.AppointmentSampleEndTime,
            StatusColor = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"])
        };

        private async Task<AppointmentViewModel> GetMainAppointmentAsync()
        {
            if(Widget == null) return null;
            if(Widget.IsPreview) return CreatePreviewAppointment();
            if(!_graph.IsSignedIn || _graph.Client == null) return null;

            var appointments = await _msCalendar.GetAppointmentsByDateAsync(DateTime.Now, DateTime.Now.AddDays(3));

            var appointment = appointments.appointments.FirstOrDefault();

            return appointment == null ? null :
                   new AppointmentViewModel(appointment, TimeFormat, DateFormat);
        }

        #endregion

        #region EventHandlers

        private void OnTimerTick(object sender, EventArgs e)
        {
            DateTime = TimeZoneInfo.ConvertTime(DateTime.Now, SelectedTimeZone?.TimeZone ?? TimeZoneInfo.Local);
        }

        private void OnTimeZonesChanged(object sender, IEnumerable<TimeZoneModel> e)
        {
            TimeZonesCollection = new ObservableCollection<TimeZoneModel>(e);
            SelectedTimeZone = GetSettingsTimeZone();
        }

        private async void OnGraphSignedIn(object sender, EventArgs e)
        {
            Appointment = await GetMainAppointmentAsync();
        }

        private void OnSettingsValueChanged(object sender, string e)
        {
            if(nameof(IsDateEnabled) == e)
               OnPropertyChanged(nameof(IsDateEnabled));

            if(nameof(IsAppointmentsEnabled) == e)
               OnPropertyChanged(nameof(ShowAppointment));

            if(nameof(IsBackgroundDisabled) == e && Widget != null)
               SetBackgroundDisabled(IsBackgroundDisabled);

            if(nameof(IsShadowEnabled) == e)
               Shadow = GetShadow(IsShadowEnabled);

            if(nameof(DateFormat) == e)
               DateFormat = _settings?.GetSetting<string>(nameof(DateFormat), DateFormatValidator.Default);

            if(nameof(TimeFormat) == e)
               TimeFormat = _settings?.GetSetting<string>(nameof(TimeFormat), TimeFormatValidator.Default);

            if(nameof(IsClockFaceEnabled) == e)
               IsClockFaceEnabled = _settings?.GetSetting<bool>(nameof(IsClockFaceEnabled), true) ?? true;

            if(nameof(SelectedTimeZone) == e)
               SetSelectedTimeZone(GetSettingsTimeZone(), false);

            if(nameof(WidgetMode) == e)
            {
                OnPropertyChanged(nameof(IsAnalogClockVisible));
                OnPropertyChanged(nameof(IsDigitalClockVisible));
            }

            if(nameof(DefaultFontFamily) == e || nameof(EnableCustomFont) == e)
            {
                OnPropertyChanged(nameof(DefaultFont));
            }
        }

        #endregion
    }
}
