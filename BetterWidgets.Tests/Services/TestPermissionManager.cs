using BetterWidgets.Abstractions;
using BetterWidgets.Attributes;
using BetterWidgets.Enums;
using BetterWidgets.Events;
using BetterWidgets.Model;
using BetterWidgets.Services;
using System.Reflection;

namespace BetterWidgets.Tests.Services
{
    public class TestPermissionManager<TWidget> : IPermissionManager<TWidget> where TWidget : IWidget
    {
        public event EventHandler<PermissionChangedEventArgs> PermissionChanged;
        public event EventHandler<PermissionChangedEventArgs> PermissionRevoked;

        public bool HasPermission(Permission permission)
        {
            var permissions = typeof(TWidget).GetCustomAttribute<WidgetPermissions>(false);

            return permissions.Permissions?.Any(p => permission.Scope == p) ?? false;
        }

        public Task<PermissionState> RequestAccessAsync(Permission permission, CancellationToken token = default)
        {
            return Task.FromResult(PermissionState.Allowed);
        }

        public Task<PermissionState> TryChangePermissionStateAsync(Permission permission, PermissionState permissionState, CancellationToken token = default)
        {
            return Task.FromResult(PermissionState.Allowed);
        }

        public PermissionState TryCheckPermissionState(Permission allowed)
        {
            return PermissionState.Allowed;
        }

        public PermissionState TryRevokePermission(Permission permission)
        {
            return PermissionState.Denied;
        }
    }
}
