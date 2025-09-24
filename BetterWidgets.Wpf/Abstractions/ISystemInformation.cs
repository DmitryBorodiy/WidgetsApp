using BetterWidgets.Model;

namespace BetterWidgets.Abstractions
{
    public interface ISystemInformation<T> where T : IWidget
    {
        Task<(CpuInformation cpu, Exception ex)> GetCpuInformationAsync(int index = 0);

        Task<(IEnumerable<GpuInformation> gpus, Exception ex)> GetGpuInformationAsync();
    }
}
