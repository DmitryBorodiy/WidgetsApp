using BetterWidgets.Model;
using BetterWidgets.Events;

namespace BetterWidgets.Abstractions
{
    public interface IPerformanceWatcher<T> : IDisposable where T : IWidget
    {
        bool IsWatching { get; }
        int MaxPoints { get; set; }
        int HardwareIndex { get; set; }
        int UpdateTick { get; set; }
        List<UtilizationReport> UtilizationReports { get; }

        event EventHandler<ReportChangedEventArgs> ReportChanged;

        Task<(IEnumerable<UtilizationReport> report, Exception ex)> GetUtilizationReportAsync();

        void Start();
        void Stop();
    }
}
