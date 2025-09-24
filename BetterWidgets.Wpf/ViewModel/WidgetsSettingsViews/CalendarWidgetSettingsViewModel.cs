using BetterWidgets.Properties;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using BetterWidgets.Helpers;
using BetterWidgets.Services;
using BetterWidgets.Model;
using BetterWidgets.Behaviours.Validators;
using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Views.Dialogs;
using System.Windows.Media;
using System.Windows;
using BetterWidgets.Extensions;
using BetterWidgets.Controls;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Wpf.Ui.Controls;

namespace BetterWidgets.ViewModel.WidgetsSettingsViews
{
    public partial class CalendarWidgetSettingsViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings<CalendarWidget> _settings;
        private readonly IMSAccountInformation _account;
        private readonly IPermissionManager<CalendarWidget> _permissions;
        private readonly IMSGraphService _graph;
        #endregion

        public CalendarWidgetSettingsViewModel()
        {
            _logger = App.Services?.GetService<ILogger>();
            _account = App.Services?.GetService<IMSAccountInformation>();
            _settings = App.Services?.GetService<Settings<CalendarWidget>>();
            _permissions = App.Services?.GetService<IPermissionManager<CalendarWidget>>();
            _graph = App.Services?.GetService<IMSGraphService>();
        }

        #region Props

        public bool IsMicrosoftSignedIn => Account != null;
        public bool IsNotSignedIn => !_graph?.IsSignedIn ?? true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsMicrosoftSignedIn))]
        [NotifyPropertyChangedFor(nameof(IsNotSignedIn))]
        public Account account;

        [ObservableProperty]
        public ObservableCollection<ErrorMessage> errors = new ObservableCollection<ErrorMessage>();

        public string TimeFormat
        {
            get => GetValue(nameof(TimeFormat), TimeFormatValidator.Default);
            set => SetValue(nameof(TimeFormat), value);
        }

        public string DateFormat
        {
            get => GetValue(nameof(DateFormat), DateFormatValidator.Default);
            set => SetValue(nameof(DateFormat), value);
        }

        public int DayOfWeekSetting
        {
            get => GetValue(nameof(DayOfWeekSetting), 0);
            set => SetValue(nameof(DayOfWeekSetting), value);
        }

        #region AppointmentsColors

        readonly Dictionary<string, string> ColorDefaults = new Dictionary<string, string>()
        {
            { nameof(FreeColor), "#4CAF50" },
            { nameof(TentativeColor), "#9C27B0" },
            { nameof(BusyColor), "#F44336" },
            { nameof(OofColor), "#3F51B5" },
            { nameof(WorkingelsewhereColor), "#FFC107" }
        };

        public Color FreeColor => GetSettingsColor(nameof(FreeColor));
        public Color TentativeColor => GetSettingsColor(nameof(TentativeColor));
        public Color BusyColor => GetSettingsColor(nameof(BusyColor));
        public Color OofColor => GetSettingsColor(nameof(OofColor));
        public Color WorkingelsewhereColor => GetSettingsColor(nameof(WorkingelsewhereColor));

        #endregion

        #endregion

        #region Utils

        private T GetValue<T>(string key, T defaultValue = default)
        {
            if(_settings == null) return default;

            return _settings.GetSetting<T>(key, defaultValue);
        }

        private void SetValue<T>(string key, T value)
        {
            if(_settings == null) return;

            _settings.SetSetting<T>(key, value);
        }

        private Color GetSettingsColor(string key)
        {
            if(_settings == null) return Colors.Transparent;

            string hex = _settings.GetSetting(key, ColorDefaults[key]);

            return (Color)ColorConverter.ConvertFromString(hex);
        }

        private void SetSettingsColor(string key, Color value)
        {
            string hex = value.ToHex();

            _settings?.SetSetting(key, hex);
        }

        private void CheckPermission()
        {
            try
            {
                if(Errors == null) 
                   Errors = new ObservableCollection<ErrorMessage>();

                var permissionState = _permissions.TryCheckPermissionState(new Permission(Scopes.Appointments));

                if(permissionState != PermissionState.Allowed)
                {
                    var message = new ErrorMessage()
                    {
                        Title = Resources.Resources.NoPermission,
                        Message = Resources.Resources.AppointmentsPermissionSubtitle
                    };

                    message.Content = new Button()
                    {
                        Content = Resources.Resources.AllowLabel,
                        Command = RequestPermissionCommand,
                        CommandParameter = message
                    };

                    Errors.Add(message);
                }
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task OnLoadedAsync()
        {
            CheckPermission();

            if(_graph?.IsSignedIn ?? false)
               Account = await _account.GetAccountInformationAsync();
        }

        [RelayCommand]
        private void GoBack() => ShellHelper.GoBack();

        [RelayCommand]
        private async Task SignOutAsync()
        {
            await _graph?.SignOutAsync();

            Account = null;
        }

        [RelayCommand]
        private void SelectColor(string key)
        {
            var oldColor = GetSettingsColor(key);

            ColorPickerDialog dialog = new ColorPickerDialog()
            {
                SelectedColor = oldColor
            };

            dialog.ShowDialog();

            if(oldColor != dialog.SelectedColor)
            {
                SetSettingsColor(key, dialog.SelectedColor);
                OnPropertyChanged(key);
            }
        }

        [RelayCommand]
        private async Task RequestPermissionAsync(ErrorMessage message)
        {
            var permission = await _permissions.RequestAccessAsync(new Permission(Scopes.Appointments));

            if(permission == PermissionState.Allowed)
               Errors?.Remove(message);
        }

        #endregion
    }
}
