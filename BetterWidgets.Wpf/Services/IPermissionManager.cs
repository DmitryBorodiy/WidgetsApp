using BetterWidgets.Enums;
using BetterWidgets.Events;
using BetterWidgets.Model;

namespace BetterWidgets.Services
{
    public partial interface IPermissionManager
    {
        event EventHandler<PermissionChangedEventArgs> PermissionChanged;

        bool HasPermission(Guid widgetId, Permission permission);
        
        PermissionState TryCheckPermissionState(Guid widgetId, Permission permission);
        Task<PermissionState> TryChangePermissionStateAsync(Guid widgetId, Permission permission, PermissionState permissionState, CancellationToken token = default);
        Task<PermissionState> RequestAccessAsync(Guid widgetId, Permission permission, CancellationToken token = default);
        PermissionState TryRevokePermission(Guid widgetId, Permission permission);
    }
}
