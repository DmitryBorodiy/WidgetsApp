using Wpf.Ui.Controls;
using BetterWidgets.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using BetterWidgets.Model;
using BetterWidgets.Enums;

namespace BetterWidgets.ViewModel.Components
{
    public partial class PermissionViewItem : ObservableObject
    {
        #region Services
        private readonly Guid _widgetGuid;
        private readonly Permission _permission;
        private readonly IPermissionManager _permissionManager;
        #endregion

        public PermissionViewItem(Guid widgetGuid, Permission permission)
        {
            _widgetGuid = widgetGuid;
            _permission = permission;
            _permissionManager = PermissionManager.GetForCurrentThread();

            Icon = permission.GetIcon();
            Title = Resources.Resources.ResourceManager.GetString(permission.Scope);
            Subtitle = Resources.Resources.ResourceManager.GetString($"{permission.Scope}PermissionSubtitle");

            _isEnabled = GetEnabledState(widgetGuid, permission);
        }

        #region Props

        [ObservableProperty]
        public string title = string.Empty;

        [ObservableProperty]
        public string subtitle = string.Empty;

        [ObservableProperty]
        public SymbolRegular icon;

        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetEnabledState(value, _widgetGuid, _permission);
        }

        #endregion

        private bool GetEnabledState(Guid widgetId, Permission permission)
            => _permissionManager?.TryCheckPermissionState(widgetId, permission) == PermissionState.Allowed;

        private async void SetEnabledState(bool value, Guid widgetId, Permission permission)
        {
            PermissionState state = value ? PermissionState.Allowed : PermissionState.Denied;

            var result = await _permissionManager?.TryChangePermissionStateAsync(widgetId, permission, state);
            
            _isEnabled = result == PermissionState.Allowed;
            OnPropertyChanged(nameof(IsEnabled));
        }
    }
}
