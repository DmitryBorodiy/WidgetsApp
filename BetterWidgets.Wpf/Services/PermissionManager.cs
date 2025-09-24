using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Events;
using BetterWidgets.Model;
using BetterWidgets.Properties;
using BetterWidgets.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Windows.Security.Credentials.UI;
using Wpf.Ui.Controls;

namespace BetterWidgets.Services
{
    public partial class PermissionManager : IPermissionManager
    {
        #region Services
        private readonly ILogger _logger;
        private readonly IWindowsHelloService _windowsHello;
        private readonly Settings _settings;
        #endregion

        public PermissionManager(
            ILogger<PermissionManager> logger,
            Settings settings)
        {
            _logger = logger;
            _settings = settings;
            _windowsHello = App.Services?.GetService<IWindowsHelloService>();
        }

        public static IPermissionManager GetForCurrentThread() => App.Services?.GetService<IPermissionManager>();

        public event EventHandler<PermissionChangedEventArgs> PermissionChanged;

        public PermissionState TryCheckPermissionState(Guid widgetId, Permission permission)
        {
            try
            {
                if(permission == null) throw new ArgumentNullException(Errors.PermissionWasNull);
                if(!HasPermission(widgetId, permission)) return PermissionState.Denied;

                var state = (PermissionState)_settings?.GetValue<int>(permission.GetKey(widgetId), (int)PermissionState.Undefined);

                return state;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return PermissionState.Undefined;
            }
        }

        public bool HasPermission(Guid widgetId, Permission permission)
        {
            var widget = WidgetManager.Current.GetWidgetById(widgetId);

            return widget?.Permissions?.Any(p => p.GetKey(widgetId) == permission.GetKey(widgetId)) ?? false;
        }

        public async Task<PermissionState> RequestAccessAsync(Guid widgetId, Permission permission, CancellationToken token = default)
        {
            try
            {
                if(permission == null) throw new ArgumentNullException(Errors.PermissionWasNull);
                if(!HasPermission(widgetId, permission)) throw new UnauthorizedAccessException(string.Format(Errors.WidgetHasNotDefinedPermission, permission.Scope));

                var dialog = new RequestAccessDialog(permission);
                var result = await dialog.CreateDialog().ShowDialogAsync(true, token);

                return await RequestConcentAsync(widgetId, permission, result);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return PermissionState.Denied;
            }
        }

        public async Task<PermissionState> TryChangePermissionStateAsync(Guid widgetId, Permission permission, PermissionState permissionState, CancellationToken token = default)
        {
            try
            {
                if(permission == null) throw new ArgumentNullException(Errors.PermissionWasNull);
                if(!HasPermission(widgetId, permission)) throw new UnauthorizedAccessException(Errors.WidgetHasNotDefinedPermission);

                var state = TryCheckPermissionState(widgetId, permission);

                if(permissionState == PermissionState.Allowed && state != PermissionState.Allowed)
                   state = await RequestAccessAsync(widgetId, permission, token);
                else if(permissionState == PermissionState.Denied)
                   state = TryRevokePermission(widgetId, permission);

                return state;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return PermissionState.Denied;
            }
        }

        public PermissionState TryRevokePermission(Guid widgetId, Permission permission)
        {
            if(permission == null) throw new ArgumentNullException(Errors.PermissionWasNull);
            if(!HasPermission(widgetId, permission)) throw new UnauthorizedAccessException(Errors.WidgetHasNotDefinedPermission);

            permission.State = PermissionState.Denied;
            _settings?.SetValue(permission.GetKey(widgetId), (int)permission.State);

            PermissionChanged?.Invoke(this, new PermissionChangedEventArgs(permission));

            return permission.State;
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
