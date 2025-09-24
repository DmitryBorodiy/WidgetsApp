using BetterWidgets.Enums;
using BetterWidgets.Events;
using BetterWidgets.Model;
using BetterWidgets.Services;

namespace BetterWidgets.Tests.Services
{
    public class PermissionManager : IPermissionManager
    {
        public event EventHandler<PermissionChangedEventArgs> PermissionChanged;

        public bool HasPermission(Guid widgetId, Permission permission)
        {
            return true;
        }

        public Task<PermissionState> RequestAccessAsync(Guid widgetId, Permission permission, CancellationToken token = default)
        {
            return Task.FromResult(PermissionState.Allowed);
        }

        public Task<PermissionState> TryChangePermissionStateAsync(Guid widgetId, Permission permission, PermissionState permissionState, CancellationToken token = default)
        {
            return Task.FromResult(PermissionState.Allowed);
        }

        public PermissionState TryCheckPermissionState(Guid widgetId, Permission permission)
        {
            return PermissionState.Allowed;
        }

        public PermissionState TryRevokePermission(Guid widgetId, Permission permission)
        {
            return PermissionState.Denied;
        }
    }
}