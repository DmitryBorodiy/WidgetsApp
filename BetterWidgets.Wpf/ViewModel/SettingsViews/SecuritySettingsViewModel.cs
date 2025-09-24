using BetterWidgets.Extensions;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace BetterWidgets.ViewModel.SettingsViews
{
    public partial class SecuritySettingsViewModel : ObservableObject
    {
        #region Services
        private readonly Settings _settings;
        private readonly IWindowsHelloService _windowsHello;
        private readonly IWidgetManager _widgetManager;
        #endregion

        public SecuritySettingsViewModel()
        {
            _settings = App.Services?.GetService<Settings>();
            _windowsHello = App.Services?.GetService<IWindowsHelloService>();
            _widgetManager = WidgetManager.Current;
        }

        #region Props

        public bool IsWindowsHelloEnabled
        {
            get => _settings?.GetValue<bool>(nameof(IsWindowsHelloEnabled), false) ?? false;
            set
            {
                _settings?.SetValue<bool>(nameof(IsWindowsHelloEnabled), value);
                OnPropertyChanged(nameof(IsWindowsHelloEnabled));
            }
        }

        public double LockInterval
        {
            get => _windowsHello?.LockTime.TotalMinutes ?? 5;
            set
            {
                if (_windowsHello != null)
                    _windowsHello.LockTime = TimeSpan.FromMinutes(value);

                OnPropertyChanged(nameof(LockInterval));
            }
        }

        public bool AllowSharing
        {
            get => _settings.AllowSharing;
            set
            {
                _settings.AllowSharing = value;
                OnPropertyChanged(nameof(AllowSharing));
            }
        }

        [ObservableProperty]
        public ObservableCollection<PermissionsViewModel> permissions;

        private PermissionsViewModel _widgetPermission;
        public PermissionsViewModel WidgetPermission
        {
            get => _widgetPermission;
            set => SetWidgetPermission(value);
        }

        #endregion

        #region Tasks

        [RelayCommand]
        private void OnLoad(object parameter)
        {
            Permissions = _widgetManager?.Widgets.Values.Select(w => new PermissionsViewModel(w))
                                                        .ToObservableCollection();
        }

        private void SetWidgetPermission(PermissionsViewModel widget)
        {
            if(widget == null) return;

            _widgetPermission = widget;
            OnPropertyChanged(nameof(WidgetPermission));

            PermissionsDialog dialog = new PermissionsDialog(widget.Widget);
            dialog.ShowDialog();
        }

        #endregion
    }
}
