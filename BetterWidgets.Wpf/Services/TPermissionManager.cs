using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Model;
using BetterWidgets.Properties;
using BetterWidgets.Views.Dialogs;
using Microsoft.Extensions.Logging;
using Windows.Security.Credentials.UI;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;
using BetterWidgets.Controls;
using BetterWidgets.Events;

namespace BetterWidgets.Services
{
    public partial class PermissionManager<TWidget> : IPermissionManager<TWidget> where TWidget : Widget
    {
        #region Services
        private readonly ILogger _logger;
        private readonly IWidgetManager _widgetManager;
        private readonly IWindowsHelloService _windowsHello;
        private readonly Settings _settings;
        #endregion

        public PermissionManager(
            ILogger<PermissionManager<TWidget>> logger,
            Settings settings)
        {
            _logger = logger;
            _settings = settings;
            _widgetManager = WidgetManager.Current;
            _windowsHello = App.Services?.GetService<IWindowsHelloService>();

            _widgetType = typeof(TWidget);
            _widget = GetWidget(_widgetType);
        }

        #region Fields

        private readonly Type _widgetType;
        private WidgetMetadata _widget;

        #endregion

        #region Events
        public event EventHandler<PermissionChangedEventArgs> PermissionChanged;
        public event EventHandler<PermissionChangedEventArgs> PermissionRevoked;
        #endregion

        public PermissionState TryCheckPermissionState(Permission permission)
        {
            try
            {
                if(permission == null) throw new ArgumentNullException(Errors.PermissionWasNull);

                _widget = GetWidget(_widgetType);

                if(!HasPermission(permission)) return PermissionState.Denied;

                var state = (PermissionState)_settings?.GetValue(permission.GetKey(_widget.Id), (int)PermissionState.Undefined);

                permission.State = state;

                return state;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return PermissionState.Undefined;
            }
        }

        public bool HasPermission(Permission permission)
        {
            return _widget?.Permissions?.Any(p => p.GetKey(_widget.Id) == permission.GetKey(_widget.Id)) ?? false;
        }

        public async Task<PermissionState> RequestAccessAsync(Permission permission, CancellationToken token = default)
        {
            try
            {
                if(permission == null) throw new ArgumentNullException(Errors.PermissionWasNull);
                if(_widget == null) _widget = GetWidget(_widgetType);

                if(!HasPermission(permission)) return PermissionState.Denied;

                var dialog = new RequestAccessDialog(permission);
                var result = await dialog.CreateDialog().ShowDialogAsync(true, token);

                return await RequestConcentAsync(_widget.Id, permission, result);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
                
                return PermissionState.Denied;
            }
        }

        public async Task<PermissionState> TryChangePermissionStateAsync(Permission permission, PermissionState permissionState, CancellationToken token = default)
        {
            try
            {
                if(permission == null) throw new ArgumentNullException(Errors.PermissionWasNull);

                _widget = GetWidget(_widgetType);

                if(!HasPermission(permission)) return PermissionState.Denied;

                var state = TryCheckPermissionState(permission);

                if(permissionState == PermissionState.Allowed && state != PermissionState.Allowed)
                   state = await RequestAccessAsync(permission, token);
                else if(permissionState == PermissionState.Denied)
                   state = TryRevokePermission(permission);

                return state;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return PermissionState.Denied;
            }
        }

        public PermissionState TryRevokePermission(Permission permission)
        {
            if(permission == null) throw new ArgumentNullException(Errors.PermissionWasNull);
            if(_widget == null) throw new InvalidOperationException(Errors.WidgetTypeWasNotRegistered);

            if(!HasPermission(permission)) return PermissionState.Denied;

            permission.State = PermissionState.Denied;
            _settings?.SetValue(permission.GetKey(_widget.Id), (int)permission.State);

            PermissionRevoked?.Invoke(this, new PermissionChangedEventArgs(permission));

            return permission.State;
        }

        private WidgetMetadata GetWidget(Type widgetType)
        {
            if(widgetType == null) throw new ArgumentNullException(Errors.TypeWasNull);

            return _widgetManager?.GetWidgetByType(widgetType);
        }

        private PermissionState SetPermission(Guid widgetId, Permission permission, MessageBoxResult result)
        {
            var state = result == MessageBoxResult.Primary ?
                        PermissionState.Allowed : PermissionState.Denied;

            permission.State = state;
            _settings.SetValue(permission.GetKey(widgetId), (int)state);

            PermissionChanged?.Invoke(this, new PermissionChangedEventArgs(permission));

            return state;
        }

        private async Task<PermissionState> RequestConcentAsync(Guid widgetId, Permission permission, MessageBoxResult result, CancellationToken token = default)
        {
            if(await _windowsHello.CheckAvailabilityAsync())
            {
                var concentResult = await _windowsHello.RequestConcentAsync
                (
                    Resources.Resources.RequestAccessDialogTitle,
                    token
                );

                if(concentResult == UserConsentVerificationResult.Verified)
                   return SetPermission(widgetId, permission, result);

                return PermissionState.Denied;
            }
            else return SetPermission(widgetId, permission, result);
        }
    }
}
