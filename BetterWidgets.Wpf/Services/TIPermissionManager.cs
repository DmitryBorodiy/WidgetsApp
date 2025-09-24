using BetterWidgets.Enums;
using BetterWidgets.Events;
using BetterWidgets.Model;

namespace BetterWidgets.Services
{
    public partial interface IPermissionManager<TWidget>
    {
        event EventHandler<PermissionChangedEventArgs> PermissionChanged;
        event EventHandler<PermissionChangedEventArgs> PermissionRevoked;

        bool HasPermission(Permission permission);

        PermissionState TryCheckPermissionState(Permission allowed);
        Task<PermissionState> TryChangePermissionStateAsync(Permission permission, PermissionState permissionState, CancellationToken token = default);
        Task<PermissionState> RequestAccessAsync(Permission permission, CancellationToken token = default);
        PermissionState TryRevokePermission(Permission permission);
    }
}
