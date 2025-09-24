using Windows.ApplicationModel;
using Microsoft.Extensions.Logging;

namespace BetterWidgets.Services
{
    public class ApplicationManager : IApplicationManager
    {
        private readonly ILogger _logger;

        public ApplicationManager(ILogger<ApplicationManager> logger)
        {
            _logger = logger;
        }

        public async Task<StartupTask> GetCurrentStartupTaskAsync()
        {
            var tasks = await StartupTask.GetForCurrentPackageAsync();
            
            return tasks?.FirstOrDefault();
        }

        public async Task<StartupTaskState> RequestStateAsync(bool enable)
        {
            try
            {
                var task = await GetCurrentStartupTaskAsync();

                bool isEnabled = task?.State == StartupTaskState.Enabled ||
                                 task?.State == StartupTaskState.EnabledByPolicy;

                if(enable && !isEnabled) await task?.RequestEnableAsync();
                else if(isEnabled && !enable) task?.Disable();

                return task?.State ?? StartupTaskState.Disabled;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return StartupTaskState.Disabled;
            }
        }

        public async Task<StartupTaskState> GetApplicationStartupStateAsync()
        {
            try
            {
                var task = await GetCurrentStartupTaskAsync();

                return task?.State ?? StartupTaskState.Disabled;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return StartupTaskState.Disabled;
            }
        }
    }
}
