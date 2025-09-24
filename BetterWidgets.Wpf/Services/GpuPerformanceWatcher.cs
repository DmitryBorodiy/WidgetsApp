using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Events;
using BetterWidgets.Model;
using BetterWidgets.Services.Hardware;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Logging;

namespace BetterWidgets.Services
{
    public class GpuPerformanceWatcher<T> : IPerformanceWatcher<T>, IPermissionable where T : IWidget
    {
        #region Services
        private Timer _timer;
        private readonly IDataService _dataService;
        private readonly IWidgetManager _widgetManager;
        private readonly IPermissionManager _permissions;
        private readonly IPermissionManager<T> _widgetPermissions;
        private readonly ILogger<GpuPerformanceWatcher<T>> _logger;

        private readonly Computer _computer;
        private readonly GpuUpdateVisitor _gpuUpdateVisitor;
        #endregion

        public GpuPerformanceWatcher(ILogger<GpuPerformanceWatcher<T>> logger, IDataService dataService, GpuUpdateVisitor gpuUpdateVisitor, IPermissionManager permissions, IPermissionManager<T> widgetPermissions)
        {
            _logger = logger;
            _permissions = permissions;
            _dataService = dataService;
            _gpuUpdateVisitor = gpuUpdateVisitor;
            _widgetManager = WidgetManager.Current;
            _widgetPermissions = widgetPermissions;

            _computer = new Computer() { IsGpuEnabled = true };
        }

        private bool _isRunning;
        private bool _hasAccess = true;
        private int _updateTick = 1000;

        private readonly object _sync = new();

        #region Props

        public bool IsWatching { get; set; }
        public int MaxPoints { get; set; } = 30;
        public int HardwareIndex { get; set; }
        public int UpdateTick
        {
            get => _updateTick;
            set
            {
                if(value < 500) return;
                if(_updateTick == value) return;

                _updateTick = value;
                _timer?.Change(0, value);
            }
        }

        public List<UtilizationReport> UtilizationReports { get; } = new();

        #endregion

        #region Events

        public event EventHandler<ReportChangedEventArgs> ReportChanged;

        #endregion

        #region Utils

        private async Task<UtilizationReport> GetReportAsync() => await Task.Run(() =>
        {
            try
            {
                _computer.Open();
                _computer.Accept(_gpuUpdateVisitor);

                var report = _gpuUpdateVisitor.Reports.Count > HardwareIndex ?
                             _gpuUpdateVisitor.Reports[HardwareIndex] : _gpuUpdateVisitor.Reports.FirstOrDefault();

                return report;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        });

        #endregion

        #region Tasks

        private async Task SaveReportAsync()
            => await _dataService.SetToFileAsync(nameof(GpuPerformanceWatcher<T>), UtilizationReports);

        public async Task<(IEnumerable<UtilizationReport> report, Exception ex)> GetUtilizationReportAsync()
        {
            try
            {
                if (await RequestAccessAsync() != PermissionState.Allowed) throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission + Scopes.SystemInformation);

                var report = new List<UtilizationReport>();
                var reportData = await _dataService.GetFromFileAsync<List<UtilizationReport>>(nameof(GpuPerformanceWatcher<T>));

                if (reportData.ex != null) return (Enumerable.Empty<UtilizationReport>(), reportData.ex);

                if (reportData.data != null && reportData.data.Any())
                    report.AddRange(reportData.data);

                var currentReport = await GetReportAsync();

                if (currentReport != null) report.Add(currentReport);

                return (report, null);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (Enumerable.Empty<UtilizationReport>(), ex);
            }
        }

        public Task<PermissionState> RequestAccessAsync(PermissionLevel level = PermissionLevel.HighLevel)
        {
            var access = _widgetPermissions.TryCheckPermissionState(new Permission(Scopes.SystemInformation));
            _hasAccess = access == PermissionState.Allowed;

            return Task.FromResult(access);
        }

        #endregion

        #region Methods

        public void Start()
        {
            lock (_sync)
            {
                if (IsWatching) return;

                if (_timer == null)
                    _timer = new Timer(OnTimerTick, UtilizationReports, 0, UpdateTick);
                else
                    _timer.Change(0, UpdateTick);

                IsWatching = true;
            }
        }

        public void Stop()
        {
            lock (_sync)
            {
                if (!IsWatching) return;

                _timer?.Change(Timeout.Infinite, Timeout.Infinite);

                IsWatching = false;
            }
        }

        public void Dispose()
        {
            Stop();

            _timer.Dispose();
            _computer.Close();
        }

        #endregion

        #region Handlers

        private async void OnTimerTick(object state)
        {
            if(_isRunning || !_hasAccess) return;

            _isRunning = true;

            try
            {
                if (state is List<UtilizationReport> reports)
                {
                    if (reports.Count > MaxPoints) reports.RemoveAt(0);

                    var oldReport = reports.LastOrDefault();
                    var newReport = await GetReportAsync();

                    if (newReport != null) reports.Add(newReport);

                    await SaveReportAsync();

                    ReportChanged?.Invoke(this, new ReportChangedEventArgs(oldReport, newReport));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
            finally { _isRunning = false; }
        }

        #endregion
    }
}
