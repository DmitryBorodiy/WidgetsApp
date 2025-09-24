using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Model;
using BetterWidgets.Widgets;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.Security;
using System.Management;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace BetterWidgets.Services
{
    public sealed class SystemInformation<T> : ISystemInformation<T>, IPermissionable where T : IWidget
    {
        #region Services
        private readonly ILogger _logger;
        private readonly IPermissionManager<T> _permissions;

        private readonly ManagementObjectSearcher _cpuObj;
        private readonly ManagementObjectSearcher _gpuObj;
        #endregion

        public SystemInformation(
            ILogger<SystemInformation<T>> logger,
            IPermissionManager<T> permissions)
        {
            _logger = logger;
            _permissions = permissions;

            _cpuObj = new ManagementObjectSearcher(Queries.WMICpu);
            _gpuObj = new ManagementObjectSearcher(Queries.WMIGpu);
        }

        public async Task<PermissionState> RequestAccessAsync(PermissionLevel level = PermissionLevel.HighLevel)
        {
            var permission = new Permission(Scopes.SystemInformation, level);
            PermissionState state = PermissionState.Undefined;

            await Task.Run(() => state = _permissions.TryCheckPermissionState(permission));

            return state;
        }

        public async Task<(CpuInformation cpu, Exception ex)> GetCpuInformationAsync(int index = 0)
        {
            try
            {
                if(await RequestAccessAsync() != PermissionState.Allowed) throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission + Scopes.SystemInformation);

                var info = new CpuInformation();

                await Task.Run(() => 
                {
                    var infoContainer = _cpuObj.Get().Cast<ManagementObject>().ToArray();

                    if(index < 0 || index >= infoContainer.Length)
                       throw new IndexOutOfRangeException($"CPU index {index} is out of range. Available count: {infoContainer.Length}");

                    var device = infoContainer[index];

                    info.Name = device[WMIProperties.Name].ToString();
                    info.Cores = Convert.ToInt32(device[WMIProperties.NumberOfCores]);
                    info.Load = Convert.ToInt32(device[WMIProperties.LoadPercentage]);
                    info.Threads = Convert.ToInt32(device[WMIProperties.NumberOfLogicalProcessors]);
                    info.MaxClock = Convert.ToInt32(device[WMIProperties.MaxClockSpeed]);
                    info.CurrentClock = Convert.ToInt32(device[WMIProperties.CurrentClockSpeed]);
                });

                return (info, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(IEnumerable<GpuInformation> gpus, Exception ex)> GetGpuInformationAsync()
        {
            try
            {
                if(await RequestAccessAsync() != PermissionState.Allowed) throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission + Scopes.SystemInformation);

                var gpus = await Task.Run(() =>
                {
                    var container = _gpuObj.Get().Cast<ManagementObject>().ToArray();

                    return container.Select(m =>
                    {
                        var info = new GpuInformation();

                        info.Name = m[WMIProperties.Name].ToString();
                        info.TotalMemory = Convert.ToUInt64(m[WMIProperties.AdapterRAM]);

                        return info;
                    });
                });

                return (gpus, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (Enumerable.Empty<GpuInformation>(), ex);
            }
        }
    }
}
