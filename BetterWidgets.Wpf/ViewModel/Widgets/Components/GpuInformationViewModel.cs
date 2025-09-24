using System.Windows.Media;
using BetterWidgets.Properties;
using BetterWidgets.Abstractions;
using BetterWidgets.Events;
using BetterWidgets.Extensions;
using BetterWidgets.Model;
using BetterWidgets.Services;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BetterWidgets.ViewModel.Widgets.Components
{
    public partial class GpuInformationViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings<GpuWidget> _settings;
        private readonly IPerformanceWatcher<GpuWidget> _gpuWatcher;
        #endregion

        public GpuInformationViewModel(GpuInformation gpuInformation, int index)
        {
            _settings = App.Services?.GetRequiredService<Settings<GpuWidget>>();
            _logger = App.Services?.GetService<ILogger<GpuInformationViewModel>>();
            _gpuWatcher = App.Services.GetRequiredKeyedService<IPerformanceWatcher<GpuWidget>>(nameof(GpuPerformanceWatcher<IWidget>));
        
            Name = gpuInformation.Name;
            _gpuWatcher.HardwareIndex = index;
            _gpuWatcher.UpdateTick = UpdateInterval;

            _settings.ValueChanged += OnSettingChanged;
            _gpuWatcher.ReportChanged += OnReportChanged;
        }

        private readonly Color _chartColor = Color.FromArgb(255, 135, 103, 181);

        #region Props

        [ObservableProperty]
        public LineSeriesViewModel series;

        [ObservableProperty] public string name;

        [ObservableProperty] public int loadPercentage;

        [ObservableProperty] public string percentageDisplay = "0%";

        [ObservableProperty] public string totalMemoryDisplay = "0 B";

        [ObservableProperty] public string usedMemoryDisplay = "0 B";

        private int UpdateInterval => _settings.GetSetting(nameof(UpdateInterval), 1000);

        #endregion

        #region Commands

        [RelayCommand]
        private async Task StartWatchingAsync()
        {
            try
            {
                var report = await _gpuWatcher.GetUtilizationReportAsync();

                if(report.ex != null)
                {
                    _gpuWatcher.Stop();
                    _logger?.LogError(report.ex, report.ex.Message, report.ex.StackTrace);

                    return;
                }

                if(report.report?.Any() ?? false)
                {
                    _gpuWatcher.Start();

                    Series = new LineSeriesViewModel(report.report.Select(r =>
                    {
                        double load = r.Load;
                        return load;
                    }), _chartColor);

                    var lastReport = report.report.Last();
                    int load = (int)Math.Round(lastReport.Load);

                    LoadPercentage = load;
                    PercentageDisplay = $"{load}%";
                    UsedMemoryDisplay = lastReport.MemoryUsage.ToReadable("MB");
                    TotalMemoryDisplay = lastReport.MemoryTotal.ToReadable("MB");
                }
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        [RelayCommand]
        private void StopWatching()
        {
            if(!_gpuWatcher.IsWatching) return;

            _gpuWatcher.Stop();
        }

        #endregion

        #region Handlers

        private void OnSettingChanged(object sender, string e)
        {
            if(e == nameof(UpdateInterval))
               _gpuWatcher.UpdateTick = UpdateInterval;
        }

        private void OnReportChanged(object sender, ReportChangedEventArgs e)
        {
            if(e.NewValue == null) return;
            if(e.OldValue == e.NewValue) return;

            int load = (int)Math.Round(e.NewValue.Load);

            LoadPercentage = load;
            PercentageDisplay = $"{load}%";
            UsedMemoryDisplay = e.NewValue.MemoryUsage.ToReadable("MB");
            TotalMemoryDisplay = e.NewValue.MemoryTotal.ToReadable("MB");

            Series?.AddNext(e.NewValue.Load);
        }

        #endregion
    }
}
