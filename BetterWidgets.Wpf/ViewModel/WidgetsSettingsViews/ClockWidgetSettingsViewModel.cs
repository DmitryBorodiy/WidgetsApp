using BetterWidgets.Behaviours.Validators;
using BetterWidgets.Enums;
using BetterWidgets.Helpers;
using BetterWidgets.Model;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.Views;
using BetterWidgets.Views.Dialogs;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Controls;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace BetterWidgets.ViewModel.WidgetsSettingsViews
{
    public partial class ClockWidgetSettingsViewModel : ObservableObject
    {
        #region Services
        private readonly DispatcherTimer _timer;
        private readonly Settings<ClockWidget> _settings;
        private readonly ITimeService<ClockWidget> _timeService;
        private readonly DataService<ClockWidget> _data;
        private readonly MainWindow _mainWindow;
        #endregion

        public ClockWidgetSettingsViewModel()
        {
            _data = App.Services?.GetService<DataService<ClockWidget>>();
            _settings = App.Services?.GetService<Settings<ClockWidget>>();
            _timeService = App.Services?.GetService<ITimeService<ClockWidget>>();
            _mainWindow = App.Services?.GetService<MainWindow>();

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1),
                IsEnabled = true
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();

            if(_timeService != null)
               _timeService.TimeZonesChanged += OnTimeZonesChanged;

            TimeZonesCollection = new List<TimeZoneModel>();
        }

        #region Props

        public string TimeFormat
        {
            get => GetSetting(nameof(TimeFormat), TimeFormatValidator.Default);
            set
            {
                _settings?.SetSetting(nameof(TimeFormat), value);

                OnPropertyChanged(nameof(TimeFormat));
                OnPropertyChanged(nameof(TimeLabel));
            }
        }

        public string DateFormat
        {
            get => GetSetting(nameof(DateFormat), DateFormatValidator.Default);
            set
            {
                _settings?.SetSetting(nameof(DateFormat), value);

                OnPropertyChanged(nameof(DateFormat));
                OnPropertyChanged(nameof(DateLabel));
            }
        }

        public bool IsDateEnabled
        {
            get => GetSetting<bool>(nameof(IsDateEnabled), true);
            set => SetSetting<bool>(nameof(IsDateEnabled), value);
        }

        public bool IsBackgroundDisabled
        {
            get => GetSetting<bool>(nameof(IsBackgroundDisabled), false);
            set => SetSetting<bool>(nameof(IsBackgroundDisabled), value);
        }

        public bool IsAppointmentsEnabled
        {
            get => GetSetting<bool>(nameof(IsAppointmentsEnabled), true);
            set => SetSetting<bool>(nameof(IsAppointmentsEnabled), value);
        }

        public bool IsShadowEnabled
        {
            get => GetSetting<bool>(nameof(IsShadowEnabled), false);
            set => SetSetting<bool>(nameof(IsShadowEnabled), value);
        }

        public bool IsClockFaceEnabled
        {
            get => GetSetting<bool>(nameof(IsClockFaceEnabled), true);
            set => SetSetting<bool>(nameof(IsClockFaceEnabled), value);
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TimeLabel))]
        [NotifyPropertyChangedFor(nameof(DateLabel))]
        public DateTime dateTime;

        [ObservableProperty]
        public List<FontFamily> _fontsItemsSource;

        [ObservableProperty]
        public int _selectedFontIndex;

        public bool EnableCustomFont
        {
            get => _settings.GetSetting(nameof(EnableCustomFont), false);
            set
            {
                _settings.SetSetting(nameof(EnableCustomFont), value);
                OnPropertyChanged(nameof(EnableCustomFont));
            }
        }

        public string DefaultFontFamily
        {
            get => _settings.GetSetting(nameof(DefaultFontFamily), "Segoe UI");
            set
            {
                _settings.SetSetting(nameof(DefaultFontFamily), value);
                OnPropertyChanged(nameof(SelectedFont));
            }
        }

        public FontFamily SelectedFont => EnableCustomFont ?
                                          new FontFamily(DefaultFontFamily) : SystemFonts.MessageFontFamily;

        public string TimeLabel => DateTime.ToString(TimeFormat);
        public string DateLabel => DateTime.ToString(DateFormat);

        private ClockWidgetMode _widgetMode = ClockWidgetMode.DigitalClock;
        public ClockWidgetMode WidgetMode
        {
            get => (ClockWidgetMode)_settings?.GetSetting<int>(nameof(WidgetMode), 0);
            set
            {
                _settings?.SetSetting<int>(nameof(WidgetMode), (int)value);
                SetProperty(ref _widgetMode, value);
            }
        }

        private TimeZoneModel selectedTimeZone;
        public TimeZoneModel SelectedTimeZone
        {
            get => selectedTimeZone;
            set => SetSelectedTimeZone(value);
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsTimeZonesEmpty))]
        public List<TimeZoneModel> timeZonesCollection;
        public bool IsTimeZonesEmpty => TimeZonesCollection?.Any() ?? true;

        #endregion

        #region Utils

        private T GetSetting<T>(string name, T defaultValue = default)
        {
            if(_settings == null) return defaultValue;

            return _settings.GetSetting<T>(name, defaultValue);
        }

        private void SetSetting<T>(string name, T value)
        {
            _settings?.SetSetting<T>(name, value);
            OnPropertyChanged(name);
        }

        private void SetSelectedTimeZone(TimeZoneModel value)
        {
            if(value == null) return;
            if(selectedTimeZone == value) return;

            selectedTimeZone = value;
            DateTime = TimeZoneInfo.ConvertTime(DateTime.Now, value.TimeZone);

            SetSetting(nameof(SelectedTimeZone), value.TimeZone.DisplayName);
        }

        private TimeZoneModel GetSettingsTimeZone()
        {
            string name = GetSetting(nameof(SelectedTimeZone), TimeZoneInfo.Local.DisplayName);
            var timezone = _timeService?.GetTimeZoneByName(name);

            return timezone?.Id != TimeZoneInfo.Local.Id ?
                   TimeZonesCollection?.FirstOrDefault(t => t.TimeZone.DisplayName == name) : null;
        }

        private int GetSelectedFontIndex()
            => FontsItemsSource.IndexOf(FontsItemsSource.FirstOrDefault(f => f.Source == DefaultFontFamily));

        #endregion

        #region Commands

        public ICommand BackCommand => new RelayCommand(ShellHelper.GoBack);

        [RelayCommand]
        private async Task OnLoadedAsync()
        {
            FontsItemsSource = Fonts.SystemFontFamilies.ToList();
            TimeZonesCollection = (List<TimeZoneModel>) 
                                   await _timeService?.GetAllUsingTimezonesAsync();

            SelectedFontIndex = GetSelectedFontIndex();
            SelectedTimeZone = GetSettingsTimeZone();
        }

        [RelayCommand]
        private async Task AddTimeZone()
        {
            TimeZonePicker picker = new TimeZonePicker();
            var result = await picker.ShowDialogAsync();

            if(result == MessageBoxResult.Primary)
            {
                await _timeService.AddUsingTimezoneAsync(picker.TimeZone.TimeZone);

                SelectedTimeZone = picker.TimeZone;
            }
        }

        [RelayCommand]
        private async Task RemoveTimeZoneAsync(object parameter)
        {
            if(parameter is TimeZoneModel timeZone)
            {
                await _timeService.RemoveUsingTimezoneAsync(timeZone.TimeZone);
            }
        }

        #endregion

        #region EventHandlers

        private void OnTimerTick(object sender, EventArgs e)
        {
            DateTime = TimeZoneInfo.ConvertTime
            (
                DateTime.Now, 
                SelectedTimeZone?.TimeZone ?? TimeZoneInfo.Local
            );
        }

        private void OnTimeZonesChanged(object sender, IEnumerable<TimeZoneModel> e)
        {
            TimeZonesCollection = null;
            TimeZonesCollection = (List<TimeZoneModel>)e;
        }

        partial void OnSelectedFontIndexChanged(int value)
        {
            if(value >= 0)
            {
                DefaultFontFamily = FontsItemsSource[value].Source;
                _settings.SetSetting(nameof(SelectedFontIndex), value);
            }
        }

        #endregion
    }
}
