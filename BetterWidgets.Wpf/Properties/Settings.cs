using BetterWidgets.Consts;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Wpf.Ui.Appearance;

namespace BetterWidgets.Properties
{
    public class Settings : SettingsBase
    {
        public Settings(ILogger<Settings> logger) : base(logger) { }

        #region Settings

        public CultureInfo AppLanguage
        {
            get => new CultureInfo(GetValue(nameof(AppLanguage), "en-US"));
            set => SetValue(nameof(AppLanguage), value.Name);
        }

        public double SidebarPaneWidth
        {
            get => GetValue<double>(nameof(SidebarPaneWidth), Values.MenuPaneDefaultWidth);
            set => SetValue(nameof(SidebarPaneWidth), value);
        }

        public TimeSpan LockTime
        {
            get => TimeSpan.FromMinutes(GetValue<double>(nameof(LockTime), 3));
            set => SetValue<double>(nameof(LockTime), value.Minutes);
        }

        public string WeatherUnitsMode
        {
            get => GetValue(nameof(WeatherUnitsMode), Consts.WeatherUnitsMode.Metric);
            set => SetValue(nameof(WeatherUnitsMode), value);
        }

        public double WeatherUpdateInterval
        {
            get => GetValue<double>(nameof(WeatherUpdateInterval), 50);
            set => SetValue(nameof(WeatherUpdateInterval), value);
        }

        public bool IsDarkMode
        {
            get => GetValue<bool>(nameof(IsDarkMode), false);
            set => SetDarkMode(value);
        }

        public double WidgetTransparency
        {
            get => GetValue<double>(nameof(WeatherUpdateInterval), 0.5);
            set => SetValue(nameof(WeatherUpdateInterval), value);
        }

        public bool IsTrayIconEnabled
        {
            get => GetValue(nameof(IsTrayIconEnabled), true);
            set => SetValue(nameof(IsTrayIconEnabled), value);
        }

        public bool AllowSharing
        {
            get => GetValue(nameof(AllowSharing), true);
            set => SetValue(nameof(AllowSharing), value);
        }

        public bool ShowMainWindowOnStartup
        {
            get => GetValue(nameof(ShowMainWindowOnStartup), true);
            set => SetValue(nameof(ShowMainWindowOnStartup), value);
        }

        #endregion

        #region Utils

        private void SetDarkMode(bool value)
        {
            App.RequestedTheme = value ?
                                 ApplicationTheme.Dark :
                                 ApplicationTheme.Light;

            SetValue(nameof(IsDarkMode), value);
        }

        #endregion
    }
}
