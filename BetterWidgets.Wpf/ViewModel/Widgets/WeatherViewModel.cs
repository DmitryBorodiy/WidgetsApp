using BetterWidgets.Controls;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using BetterWidgets.Model.Weather;
using System.Windows.Media;
using BetterWidgets.Consts;
using BetterWidgets.Enums;
using System.Collections.ObjectModel;
using BetterWidgets.ViewModel.Widgets.Components;
using System.Windows.Threading;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using BetterWidgets.Helpers;
using Wpf.Ui.Controls;
using BetterWidgets.Views.Dialogs;
using BetterWidgets.ViewModel.Dialogs;

namespace BetterWidgets.ViewModel.Widgets
{
    public partial class WeatherViewModel : ObservableObject
    {
        #region Consts
        private const string weatherData = "weather.json";
        #endregion

        #region Services
        private ILogger _logger;
        private IGeolocationService<WeatherWidget> _geolocation;
        private IWeatherService<WeatherWidget> _weather;
        private Settings<WeatherWidget> _settings;
        private ISettingsManager _settingsManager;
        private DispatcherTimer _updateWeather;
        private readonly DataService<WeatherWidget> _data;
        private readonly IThemeService _themeService;
        private readonly IShareService _shareService;
        #endregion

        public WeatherViewModel()
        {
            App.ThemeChanged += App_ThemeChanged;

            _logger = App.Services?.GetRequiredService<ILogger<WeatherViewModel>>();
            _geolocation = App.Services?.GetService<IGeolocationService<WeatherWidget>>();
            _weather = App.Services?.GetService<IWeatherService<WeatherWidget>>();
            _settings = App.Services?.GetService<Settings<WeatherWidget>>();
            _settingsManager = App.Services?.GetService<ISettingsManager>();
            _themeService = App.Services?.GetService<IThemeService>();
            _data = App.Services?.GetService<DataService<WeatherWidget>>();
            _shareService = App.Services?.GetService<IShareService>();

            if(_settings != null)
               _settings.ValueChanged += Settings_ValueChanged;

            _updateWeather = new DispatcherTimer();
            _updateWeather.Interval = TimeSpan.FromMinutes(UpdateInterval);
            _updateWeather.Tick += UpdateWeather_Tick;
        }

        #region Props

        private Guid WidgetId { get; set; }

        [ObservableProperty]
        public Widget widget;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RootLayoutVisibility), nameof(CanShare))]
        public bool isLoading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RootLayoutVisibility))]
        public bool isSetupNeeded;

        private WeatherInfo Weather { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Background), nameof(CanShare))]
        public WeatherDayView weatherDay;

        [ObservableProperty]
        public ObservableCollection<WeatherDayView> days;

        public Visibility RootLayoutVisibility => IsLoading || IsSetupNeeded ? 
                                                  Visibility.Collapsed : Visibility.Visible;

        public Style Background => GetBackground();
        public SolidColorBrush Foreground => GetForeground();

        public bool IsLiveBackground => _settings?.GetSetting<bool>(nameof(IsLiveBackground), true) ?? true;
        public string WeatherLocation => _settings?.GetSetting(nameof(WeatherLocation), string.Empty);
        public WeatherLocationMode LocationMode => (WeatherLocationMode)_settings?.GetSetting<int>(nameof(LocationMode), 0);
        public WeatherUnits WeatherUnits => (WeatherUnits)_settings?.GetSetting<int>(nameof(WeatherUnits), 0);
        public double UpdateInterval => _settings?.GetSetting<double>(nameof(UpdateInterval), 50) ?? 50;

        public bool CanShare => WeatherDay != null && !IsLoading;

        #endregion

        #region Utils

        private void SetWeatherUnits()
        {
            _weather.UnitsMode = WeatherUnits == WeatherUnits.Celsius ?
                                 WeatherUnitsMode.Metric : WeatherUnitsMode.Imperial;
        }

        private ObservableCollection<WeatherDayView> GetDaysCollection(WeatherInfo weather)
        {
            if(weather == null) return null;

            var days = weather?.Days?.Select(d => new WeatherDayView(d));

            return new ObservableCollection<WeatherDayView>(days);
        }

        public void SetWeatherUpdateInterval(TimeSpan interval)
        {
            if(interval.TotalMinutes < 15 ||
               interval.TotalHours > 5) return;

            if(_updateWeather != null)
               _updateWeather.Interval = interval;
        }

        #endregion

        #region Tasks

        [RelayCommand]
        private async Task OnLoadAsync(Widget widgetParameter)
        {
            if(widgetParameter != null)
            {
                Widget = widgetParameter;
                WidgetId = widgetParameter.Id;
            }
            
            if(Widget != null) Widget.NetworkStateChanged += OnNetworkStatusChanged;

            SetWeatherUnits();

            Weather = await GetWeatherAsync();
            Days = GetDaysCollection(Weather);
            WeatherDay = Days?.FirstOrDefault();

            if(!Widget?.IsPreview ?? true) _updateWeather?.Start();
        }

        [RelayCommand]
        private void OnPinned(Widget widget)
        {
            LoadCommand.Execute(widget);

            _updateWeather?.Start();
        }

        [RelayCommand]
        private void OnUnpin() => _updateWeather?.Stop();

        [RelayCommand]
        private void OnExecutionStateChanged(WidgetState state)
        {
            if(state == WidgetState.Suspended) _updateWeather?.Stop();
            else if(state == WidgetState.Activated) _updateWeather?.Start();
        }

        [RelayCommand]
        private void LaunchSettings()
        {
            ShellHelper.LaunchSettingsById(WidgetId);
        }

        [RelayCommand]
        private void RequestFindDialog()
        {
            if(Widget == null) return;
            if(Widget.IsPreview)
            {
                ShellHelper.LaunchSettingsById(Widget.Id);
                return;
            }

            var view = new SearchDialog();
            var viewModel = new SearchDialogViewModel()
            {
                Widget = Widget,
                SearchText = Weather?.City.Name
            };

            view.DataContext = viewModel;

            Widget.ShowContentDialog(new()
            {
                Title = Resources.Resources.FindPlace,
                PrimaryButtonContent = Resources.Resources.Search,
                SecondaryButtonContent = Resources.Resources.CancelLabel,
                PrimaryButtonAppearance = ControlAppearance.Primary,
                PrimaryButtonCommand = SearchCommand,
                PrimaryButtonParameter = viewModel,
                Content = view
            });
        }

        [RelayCommand]
        private async Task SearchAsync(SearchDialogViewModel viewModel)
        {
            if(viewModel == null) return;
            if(string.IsNullOrEmpty(viewModel.SearchText)) return;

            IsLoading = true;

            Weather = await GetWeatherByQueryAsync(viewModel.SearchText);

            if(Weather != null)
            {
                Days = GetDaysCollection(Weather);
                WeatherDay = Days?.FirstOrDefault();
                IsSetupNeeded = false;
                Widget?.HideNotify();

                _settings?.SetSetting(nameof(WeatherLocation), viewModel.SearchText);
            }
            else Widget?.ShowNotify
            (
                Resources.Resources.CannotGetWeatherTitle,
                Resources.Resources.CannotGetWeatherSubtitle
            );

            IsLoading = false;
        }

        [RelayCommand]
        private void Share(Widget widget)
        {
            if(!CanShare) return;
            if(widget == null) widget = Widget;

            var widgetView = _shareService.CreateWidgetCard(widget);

            if(widgetView.ex != null)
               widget.ShowNotify(
                   string.Format(Resources.Resources.CantShareSubtitle, widgetView.ex.Message),
                   Resources.Resources.CantShare,
                   true
               );

            if(widgetView.image != null)
               _shareService.RequestShare(widgetView.image, widget, Weather?.City.Name, WeatherDay?.Title);
        }

        private Style GetBackground()
        {
            var condition = WeatherDay?.WeatherDay?.Condition.FirstOrDefault();

            if(condition == null) return null;

            string key = string.Empty;
            bool isNight = condition.Icon.EndsWith("n");
            WeatherId weatherId = (WeatherId)condition.Id;

            key += (int)weatherId <= 803 && (int)weatherId > 781 ? 
                   nameof(WeatherId.Clear) : nameof(WeatherId.Cloudy);

            key += isNight ? nameof(WeatherPod.Night) : nameof(WeatherPod.Day);

            key += KnownResources.WeatherBackgound;

            return (Style)Application.Current.Resources[key];
        }

        private SolidColorBrush GetForeground()
        {
            SolidColorBrush brush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];

            if(IsLiveBackground)
               brush = new SolidColorBrush(Colors.White);
            
            return brush;
        }

        private async Task<WeatherInfo> GetWeatherAsync()
        {
            IsLoading = true;

            if(!NetworkHelper.IsConnected) return await GetWeatherFromLocalAsync();

            var weatherInfo = LocationMode == WeatherLocationMode.DetectLocation ?
                              await GetWeatherByGeolocationAsync() : await GetWeatherByQueryAsync();

            if(weatherInfo != null)
               await _data.SetToFileAsync<WeatherInfo>(weatherData, weatherInfo);

            IsSetupNeeded = weatherInfo == null;
            IsLoading = false;

            return weatherInfo;
        }

        private async Task<WeatherInfo> GetWeatherByQueryAsync(string query = null)
        {
            if(string.IsNullOrEmpty(WeatherLocation) &&
               string.IsNullOrEmpty(query))
            {
                IsSetupNeeded = true;

                return null;
            }

            var request = query == null ?
                          WeatherInfoRequest.FromQuery(WeatherLocation) :
                          WeatherInfoRequest.FromQuery(query);

            return await _weather.GetWeatherForecastAsync(request, !Widget?.IsPreview ?? false);
        }

        private async Task<WeatherInfo> GetWeatherByGeolocationAsync()
        {
            var geoposition = await _geolocation.RequestGeopositionAsync();

            return geoposition.state != PermissionState.Allowed ?
                   await GetWeatherByQueryAsync() :
                   await _weather?.GetWeatherForecastAsync
                   (
                       WeatherInfoRequest.FromGeocoordinate(geoposition.geoposition.Position.Latitude, 
                                                            geoposition.geoposition.Position.Longitude),
                       !Widget?.IsPreview ?? false
                   );
        }

        private async Task<WeatherInfo> GetWeatherFromLocalAsync()
        {
            var data = await _data?.GetFromFileAsync<WeatherInfo>(weatherData);
            IsLoading = false;

            return data.data;
        }

        #endregion

        #region EventHandlers

        private void App_ThemeChanged(object sender, ApplicationTheme e)
        {
            OnPropertyChanged(nameof(Foreground));
        }

        private void Settings_ValueChanged(object sender, string e)
        {
            if(nameof(IsLiveBackground) == e)
            {
                OnPropertyChanged(nameof(IsLiveBackground));
                OnPropertyChanged(nameof(Foreground));
            }

            if(nameof(WeatherUnits) == e) SetWeatherUnits();
        }

        private void UpdateWeather_Tick(object sender, EventArgs e)
        {
            if(LoadCommand.CanExecute(Widget) && !LoadCommand.IsRunning)
               LoadCommand.Execute(Widget);
        }

        private async void OnNetworkStatusChanged(object sender, bool isConnected)
        {
            if(isConnected)
            {
                Widget?.HideNotify();

                Weather = isConnected ? 
                          await GetWeatherAsync() :
                          await GetWeatherFromLocalAsync();

                if(Weather != null)
                {
                    Days = GetDaysCollection(Weather);
                    WeatherDay = Days.FirstOrDefault(d => d.WeatherDay.DateTime <= DateTime.Now);
                }
                else
                {
                    if(isConnected)
                       Widget?.ShowNotify
                       (
                           title: Resources.Resources.CannotGetWeatherTitle,
                           message: Resources.Resources.CannotGetWeatherSubtitle,
                           severity: InfoBarSeverity.Error
                       );
                    else
                       Widget?.ShowNotify
                       (
                           title: Resources.Resources.NoNetworkTitle,
                           message: Resources.Resources.NoNetworkSubtitle,
                           severity: InfoBarSeverity.Error
                       );
                }
            }
        }

        #endregion
    }
}
