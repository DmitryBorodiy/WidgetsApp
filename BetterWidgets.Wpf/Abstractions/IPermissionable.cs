using BetterWidgets.Enums;

namespace BetterWidgets.Abstractions
{
    public interface IPermissionable
    {
        Task<PermissionState> RequestAccessAsync(PermissionLevel level = PermissionLevel.HighLevel);
    }
}
