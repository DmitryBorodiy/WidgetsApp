using BetterWidgets.Helpers;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Windows.ApplicationModel;

namespace BetterWidgets.ViewModel.SettingsViews
{
    public partial class GeneralSettingsViewModel : ObservableObject
    {
        #region Services
        private readonly Settings _settings;
        private readonly Configuration _config;
        private readonly IApplicationManager _appManager;
        private readonly TrayIconService _trayIcon;
        #endregion

        public GeneralSettingsViewModel()
        {
            _settings = App.Services?.GetService<Settings>();
            _config = App.Services?.GetService<Configuration>();
            _appManager = App.Services?.GetService<IApplicationManager>();
            _trayIcon = App.Services?.GetService<TrayIconService>();

            Languages = _config.Languages;
        }

        #region Props

        private bool _isAutorunEnabled;
        public bool IsAutorunEnabled
        {
            get => _isAutorunEnabled;
            set => SetAutorunEnabled(value);
        }

        public bool IsTrayIconEnabled
        {
            get => _settings.IsTrayIconEnabled;
            set
            {
                _settings.IsTrayIconEnabled = value;

                if(value)
                {
                    if(!_trayIcon.IsRegistered) 
                       _trayIcon?.Register();
                }
                else _trayIcon?.Unregister();

                OnPropertyChanged(nameof(IsTrayIconEnabled));
            }
        }

        [ObservableProperty]
        public IEnumerable<CultureInfo> languages;

        public int SelectedLanguageIndex
        {
            get => GetSelectedLanguageIndex();
            set
            {
                _settings.AppLanguage = Languages.ToArray()[value];

                CultureInfo.CurrentCulture = _settings.AppLanguage;
                CultureInfo.CurrentUICulture = _settings.AppLanguage;

                OnPropertyChanged(nameof(SelectedLanguageIndex));
            }
        }

        public bool ShowMainWindowOnStartup
        {
            get => _settings.ShowMainWindowOnStartup;
            set => _settings.ShowMainWindowOnStartup = value;
        }

        #endregion

        #region Commands

        public ICommand BackCommand => new RelayCommand(ShellHelper.GoBack);

        #endregion

        #region Utils

        private async Task GetAutorunStateAsync()
        {
            var state = await _appManager?.GetApplicationStartupStateAsync();

            _isAutorunEnabled = state == StartupTaskState.Enabled ||
                                state == StartupTaskState.EnabledByPolicy;

            OnPropertyChanged(nameof(IsAutorunEnabled));
        }

        private async void SetAutorunEnabled(bool value)
        {
            var state = await _appManager?.RequestStateAsync(value);

            _isAutorunEnabled = state == StartupTaskState.Enabled ||
                                state == StartupTaskState.EnabledByPolicy;

            OnPropertyChanged(nameof(IsAutorunEnabled));
        }

        private int GetSelectedLanguageIndex()
        {
            var language = Languages.FirstOrDefault(l => l.Name == _settings.AppLanguage.Name);

            if(language == null) return 0;

            int index = Languages.ToList().IndexOf(language);

            return index >= 0 ? index : 0;
        }

        [RelayCommand]
        private void Restart()
        {
            var exePath = Process.GetCurrentProcess().MainModule.FileName;

            Process.Start(exePath);

            Application.Current.Shutdown();
        }

        #endregion

        #region CommandHandlers

        [RelayCommand]
        private async Task OnLoaded(object parameter)
        {
            await GetAutorunStateAsync();
        }

        #endregion
    }
}
