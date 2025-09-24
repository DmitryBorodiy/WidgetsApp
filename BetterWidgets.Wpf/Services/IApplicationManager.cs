using Windows.ApplicationModel;

namespace BetterWidgets.Services
{
    public interface IApplicationManager
    {
        Task<StartupTask> GetCurrentStartupTaskAsync();
        Task<StartupTaskState> RequestStateAsync(bool isEnabled);
        Task<StartupTaskState> GetApplicationStartupStateAsync();
    }
}
