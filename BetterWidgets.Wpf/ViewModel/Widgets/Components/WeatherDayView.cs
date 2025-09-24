using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BetterWidgets.Consts;
using BetterWidgets.Model.Weather;
using BetterWidgets.Properties;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;

namespace BetterWidgets.ViewModel.Widgets.Components
{
    public partial class WeatherDayView : ObservableObject
    {
        private const string _weatherIconPath = "pack://application:,,,/Assets/WeatherWidget/UnitAssets/{0}.png";

        #region Services
        private readonly Settings<WeatherWidget> _settings;
        #endregion

        public WeatherDayView() { }
        public WeatherDayView(WeatherDay weatherDay, WeatherUnits units = WeatherUnits.Celsius)
        {
            WeatherDay = weatherDay;
            WeatherUnits = units;

            _settings = App.Services?.GetService<Settings<WeatherWidget>>();

            if(_settings != null)
                _settings.ValueChanged += OnSettingsValueChanged;

            App.ThemeChanged += OnThemeChanged;
        }

        #region Props

        private WeatherUnits WeatherUnits { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Date))]
        [NotifyPropertyChangedFor(nameof(Time))]
        [NotifyPropertyChangedFor(nameof(Temperature))]
        [NotifyPropertyChangedFor(nameof(MaxTemperature))]
        [NotifyPropertyChangedFor(nameof(MinTemperature))]
        [NotifyPropertyChangedFor(nameof(Humidity))]
        [NotifyPropertyChangedFor(nameof(WindSpeed))]
        [NotifyPropertyChangedFor(nameof(IconSource))]
        public WeatherDay weatherDay;

        public string Title => GetTitle();
        public string Temperature => GetTemp(WeatherDay?.Main.Temperature);
        public string MaxTemperature => GetTemp(WeatherDay?.Main.MaxTemperature);
        public string MinTemperature => GetTemp(WeatherDay?.Main.MinTemperature);
        public string Humidity => GetHumidity(WeatherDay?.Main.Humidity);
        public string WindSpeed => GetWindSpeed(WeatherDay?.Wind.Speed);

        public string Date => GetDate();
        public string Time => GetTime();
        public bool IsLiveBackground => _settings?.GetSetting(nameof(IsLiveBackground), true) ?? true;

        public ImageSource IconSource => GetWeatherIcon();
        public SolidColorBrush Foreground => GetForeground();

        #endregion

        #region Utils

        private string GetTitle()
        {
            if(WeatherDay == null) return Resources.Resources.WeatherWidgetTitle;
            if(WeatherDay.Condition == null ||
               WeatherDay.Condition.Count == 0) return Resources.Resources.WeatherWidgetTitle;

            return char.ToUpper(WeatherDay.Condition[0].Description[0]) + WeatherDay.Condition[0].Description.Substring(1);
        }

        private ImageSource GetWeatherIcon()
        {
            if(WeatherDay == null) return null;

            var uri = new Uri(string.Format(_weatherIconPath, WeatherDay.Condition[0]?.Icon));
            var icon = new BitmapImage(uri);

            return icon;
        }

        private string GetTemp(double? temp)
            => temp.HasValue ?
               Math.Round(temp.Value).ToString() : string.Empty;

        private string GetDate() => WeatherDay?.DateTime.ToString("dd ddd");

        private string GetTime() => WeatherDay.DateTime.ToString("h tt", CultureInfo.CurrentCulture);

        private string GetHumidity(int? humidity) => $"{humidity} %";

        private string GetWindSpeed(double? speed)
        {
            if(!speed.HasValue) return string.Empty;

            string spd = Math.Round(speed.Value).ToString();
            string units = WeatherUnits == WeatherUnits.Celsius ?
                           Resources.Resources.MeterSec : Resources.Resources.MilesHour;

            return $"{spd} {units}";
        }

        private SolidColorBrush GetForeground()
            => (SolidColorBrush)App.Current.Resources[IsLiveBackground ? "SystemAltHighBrushLight" : "TextFillColorPrimaryBrush"];

        #endregion

        #region EventHandlers

        private void OnSettingsValueChanged(object sender, string e)
        {
            if(nameof(IsLiveBackground) == e)
               OnPropertyChanged(nameof(Foreground));
        }

        private void OnThemeChanged(object sender, ApplicationTheme e)
        {
            OnPropertyChanged(nameof(Foreground));
        }

        #endregion
    }
}
