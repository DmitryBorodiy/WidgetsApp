using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Enums;
using BetterWidgets.Helpers;
using BetterWidgets.Model;
using BetterWidgets.Model.Weather;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.Views;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.System;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;

namespace BetterWidgets.ViewModel.WidgetsSettingsViews
{
    public partial class WeatherWidgetSettingsViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings<WeatherWidget> _settings;
        private readonly IGeolocationService<WeatherWidget> _geolocationService;
        private readonly IWeatherService<WeatherWidget> _weatherService;
        private readonly IPermissionManager<WeatherWidget> _permissions;
        private readonly Configuration _config;
        private readonly MainWindow _mainWindow;
        #endregion

        #region Consts
        private readonly string _locationNameMessage = nameof(_locationNameMessage);
        private readonly string _geolocationPermissionMessage = nameof(_geolocationPermissionMessage);
        #endregion

        public WeatherWidgetSettingsViewModel()
        {
            _settings = App.Services?.GetService<Settings<WeatherWidget>>();
            _config = App.Services?.GetService<Configuration>();
            _geolocationService = App.Services?.GetService<IGeolocationService<WeatherWidget>>();
            _weatherService = App.Services?.GetService<IWeatherService<WeatherWidget>>();
            _permissions = App.Services?.GetService<IPermissionManager<WeatherWidget>>();
            _mainWindow = App.Current.MainWindow as MainWindow;

            _logger = App.Services?.GetRequiredService<ILogger<WeatherWidgetSettingsViewModel>>();

            ErrorMessages = new ObservableCollection<ErrorMessage>();
        }

        #region Props

        public string Version => _config.WeatherWidgetConfig?.Version;

        public bool IsLiveBackground
        {
            get => _settings?.GetSetting<bool>(nameof(IsLiveBackground), true) ?? true;
            set => _settings?.SetSetting<bool>(nameof(IsLiveBackground), value);
        }

        public int WeatherUnits
        {
            get => _settings?.GetSetting<int>(nameof(WeatherUnits), 0) ?? 0;
            set
            {
                _settings?.SetSetting<int>(nameof(WeatherUnits), value);

                _weatherService.UnitsMode = value == 0 ?
                                            WeatherUnitsMode.Metric : WeatherUnitsMode.Imperial;
            }
        }

        public bool CanEnterLocation => LocationMode == 1;

        public int LocationMode
        {
            get => _settings?.GetSetting<int>(nameof(LocationMode), 0) ?? 0;
            set
            {
                _settings?.SetSetting<int>(nameof(LocationMode), value);

                OnPropertyChanged(nameof(CanEnterLocation));
            }
        }

        public string WeatherLocation
        {
            get => _settings?.GetSetting(nameof(WeatherLocation), string.Empty);
            set
            {
                _settings?.SetSetting(nameof(WeatherLocation), value);

                OnPropertyChanged(nameof(WeatherLocation));
            }
        }

        public double UpdateInterval
        {
            get => _settings?.GetSetting<double>(nameof(UpdateInterval), 30) ?? 30;
            set => SetUpdateInterval(value);
        }

        [ObservableProperty]
        public ObservableCollection<ErrorMessage> errorMessages;

        #endregion

        #region Commands

        public ICommand GoBackCommand => new RelayCommand(ShellHelper.GoBack);

        [RelayCommand]
        private void OnLoaded()
        {
            ValidateSettings();
        }

        [RelayCommand]
        private async Task RequestGeolocationPermissionAsync()
        {
            var permissionState = await _permissions.RequestAccessAsync(new Permission(Scopes.Location, PermissionLevel.HighLevel));

            if(permissionState == PermissionState.Allowed)
            {
                var message = ErrorMessages.FirstOrDefault(m => m.Id == _geolocationPermissionMessage);

                if(message != null)
                   ErrorMessages.Remove(message);
            }
        }

        #endregion

        #region Methods

        private ErrorMessage CreateMessage(string id, string message, string title = null, InfoBarSeverity severity = InfoBarSeverity.Warning, IEnumerable<Button> actions = null)
        {
            var errorMessage = new ErrorMessage()
            {
                Title = title,
                Message = message,
                Severity = severity,
                IsClosable = true,
                IsOpen = true,
                Id = id
            };

            if(actions != null && actions.Any())
            {
                var actionsStack = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };

                foreach(var action in actions)
                    actionsStack.Children.Add(action);

                errorMessage.Content = actionsStack;
            }

            return errorMessage;
        }

        private void ValidateSettings()
        {
            if(string.IsNullOrEmpty(WeatherLocation))
               ErrorMessages?.Add(CreateMessage(
                   _locationNameMessage,
                   Resources.Resources.SetupWeatherLocationMessage, 
                   Resources.Resources.WeatherLocationSetting
               ));

            CheckGeolocationPermission();
        }

        private void CheckGeolocationPermission()
        {
            var permissionStatus = _permissions.TryCheckPermissionState(new Permission(Scopes.Location, PermissionLevel.HighLevel));

            if(permissionStatus != PermissionState.Allowed)
            {
                var allowAction = new Button()
                {
                    Content = Resources.Resources.AllowLabel,
                    Command = RequestGeolocationPermissionCommand
                };

                ErrorMessages?.Add(CreateMessage(
                    _geolocationPermissionMessage,
                    Resources.Resources.GeolocationPermissionDialogSubtitle,
                    Resources.Resources.GeolocationPermissionDialogTitle,
                    actions: [allowAction]));
            }
        }

        private void SetUpdateInterval(double value)
        {
            _settings?.SetSetting<double>(nameof(UpdateInterval), value);

            OnPropertyChanged(nameof(UpdateInterval));

            var manager = WidgetManager.Current;
            var weatherWidget = manager.GetActivatedWidgetByType<WeatherWidget>();

            weatherWidget?.SetUpdateInterval(value);
        }

        #endregion

        #region Tasks

        [RelayCommand]
        public async Task LaunchUriAsync(Uri parameter)
        {
            if(parameter == null) return;

            await Launcher.LaunchUriAsync(parameter);
        }

        [RelayCommand]
        public async Task GetCurrentGeolocationAsync()
        {
            try
            {
                var geolocation = await _geolocationService?.RequestGeopositionAsync();

                if(geolocation.state != PermissionState.Allowed)
                {
                    _mainWindow.NotifyUser
                    (
                        title: Resources.Resources.GeolocationPermissionDialogTitle,
                        message: Resources.Resources.GeolocationPermissionDialogSubtitle,
                        actionText: Resources.Resources.OpenSettingsLabel,
                        actionCommand: LaunchUriCommand
                    );

                    return;
                }

                var coord = geolocation.geoposition.Position;
                var request = WeatherInfoRequest.FromGeocoordinate(coord.Latitude, coord.Longitude);

                var weather = await _weatherService?.GetWeatherForecastAsync(request);

                WeatherLocation = weather?.City.Name;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        #endregion
    }
}
