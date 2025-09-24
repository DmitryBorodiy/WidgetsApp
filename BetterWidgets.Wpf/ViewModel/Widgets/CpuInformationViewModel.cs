using SkiaSharp;
using LiveChartsCore;
using SkiaSharp.Views.WPF;
using BetterWidgets.Controls;
using BetterWidgets.Helpers;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;
using BetterWidgets.Properties;
using BetterWidgets.Model;
using BetterWidgets.Abstractions;
using System.Windows;
using BetterWidgets.Enums;
using System.Windows.Controls;
using BetterWidgets.Consts;
using BetterWidgets.Services;
using BetterWidgets.Events;
using System.Threading.Tasks;

namespace BetterWidgets.ViewModel.Widgets
{
    public partial class CpuInformationViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly ICpuWatcher _cpuWatcher;
        private readonly ISystemInformation<CpuWidget> _system;
        private readonly IPermissionManager<CpuWidget> _permission;
        private readonly Settings<CpuWidget> _settings;
        #endregion

        public CpuInformationViewModel()
        {
            _logger = App.Services?.GetService<ILogger<CpuWidget>>();
            _cpuWatcher = App.Services?.GetRequiredService<ICpuWatcher>();
            _system = App.Services?.GetRequiredService<ISystemInformation<CpuWidget>>();
            _settings = App.Services?.GetRequiredService<Settings<CpuWidget>>();
            _permission = App.Services?.GetRequiredService<IPermissionManager<CpuWidget>>();

            if(_cpuWatcher != null)
               _cpuWatcher.UtilizationChanged += OnCpuUtilizationChanged;

            if(_permission != null)
               _permission.PermissionRevoked += OnPermissionRevoked;

            if(_settings != null)
               _settings.ValueChanged += OnSettingsValueChanged;
        }

        #region Props

        private Widget Widget { get; set; }
        private WidgetSizes Size { get; set; } = WidgetSizes.Default;

        [ObservableProperty]
        public bool isLoading;

        [ObservableProperty]
        public bool isPermissionUIVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(
            nameof(CpuName), 
            nameof(CpuClock), 
            nameof(CpuCores), 
            nameof(CpuThreads), 
            nameof(HasClockInfo))]
        public CpuInformation cpuInformation;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CpuUtilizationPercent))]
        public int cpuUtilization;

        public string CpuUtilizationPercent => $"{CpuUtilization}%";

        public string CpuName => CpuInformation?.Name;
        public string CpuClock => $"{CpuInformation?.CurrentClock} MHz";
        public string CpuCores => CpuInformation?.Cores.ToString();
        public string CpuThreads => CpuInformation?.Threads.ToString();

        public bool HasClockInfo => CpuInformation?.HasCurrentClock ?? false;

        #region Chart

        [ObservableProperty]
        public ObservableCollection<ISeries> cpuUsageData;

        [ObservableProperty]
        public ObservableCollection<int> cpuUsageValues;

        #endregion

        #region Settings

        private int SelectedCpuIndex => _settings.GetSetting(nameof(SelectedCpuIndex), 0);
        private double UpdateTickInterval => _settings.GetSetting(nameof(UpdateTickInterval), 1000);
        public TimeSpan EnableChartAnimation => _settings.GetSetting(nameof(EnableChartAnimation), true) ?
                                                TimeSpan.FromSeconds(1) : TimeSpan.Zero;
        public double ChartOpacity => _settings.GetSetting(nameof(ChartOpacity), 0.6);

        #endregion

        #endregion

        #region Utils

        private void SetLayoutState(WidgetSizes size)
        {
            if(Widget?.Content is Panel rootLayout)
            {
                if(rootLayout.Resources.Contains(size.ToString()) &&
                   rootLayout.Resources[size.ToString()] is Style rootLayoutStyle)
                   rootLayout.Style = rootLayoutStyle;

                foreach(var element in rootLayout.Children)
                {
                    if(element is FrameworkElement control)
                    {
                        if(control.Resources.Contains(size.ToString()) &&
                           control.Resources[size.ToString()] is Style style)
                           control.Style = style;
                    }
                }
            }
        }

        private async Task<bool> LoadCpuInformationAsync()
        {
            try
            {
                var cpuInfo = await _system.GetCpuInformationAsync(SelectedCpuIndex);

                if(cpuInfo.ex != null) throw cpuInfo.ex;

                if(cpuInfo.cpu != null)
                   CpuInformation = cpuInfo.cpu;

                return CpuInformation != null;
            }
            catch(UnauthorizedAccessException)
            {
                IsPermissionUIVisible = true;

                return false;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                Widget?.ShowNotify(
                    Resources.Resources.NoData,
                    Resources.Resources.NoCpuData,
                    false, InfoBarSeverity.Error);

                return false;
            }
        }

        #region Chart

        private SolidColorPaint GetCurrentSkColor()
        {
            var systemColor = AccentColorHelper.AccentColor;
            var paint = new SolidColorPaint(systemColor.ToSKColor());

            return paint;
        }

        private LinearGradientPaint GetCurrentGradient()
        {
            var accentColor = AccentColorHelper.AccentColor.ToSKColor();
            var transparent = new SKColor(0, 0, 0, 0);

            var startPoint = new SKPoint(0, 0);
            var endPoint = new SKPoint(0, 1);

            return new LinearGradientPaint([accentColor, transparent], startPoint, endPoint);
        }

        private LineSeries<T> BuildSeries<T>(IReadOnlyCollection<T> values) => new LineSeries<T>()
        {
            GeometrySize = 0.3,
            LineSmoothness = 0.1,
            Values = values,
            Stroke = GetCurrentSkColor(),
            GeometryFill = null,
            GeometryStroke = null,
            Fill = GetCurrentGradient()
        };

        private async Task DrawChartAsync()
        {
            CpuUsageData = await GetCpuUsageSeriesAsync();
        }

        private async Task<ObservableCollection<ISeries>> GetCpuUsageSeriesAsync()
        {
            if(CpuUsageValues == null)
            {
                var values = await _cpuWatcher.GetUtilizationDataAsync();
                CpuUsageValues = new ObservableCollection<int>(values);
            }

            var sampleData = BuildSeries(CpuUsageValues);
            var series = new ObservableCollection<ISeries>([sampleData]);

            return series;
        }

        #endregion

        #endregion

        #region Commands

        [RelayCommand]
        private async Task OnAppearedAsync(Widget widget)
        {
            Widget = widget;
            IsLoading = true;

            if(widget != null)
               SizeChangedCommand.Execute(widget.Size);

            bool success = await LoadCpuInformationAsync();

            if(success)
            {
                Widget?.HideNotify();

                _cpuWatcher?.Start();
                await DrawChartAsync();
            }
            else
            {
                _cpuWatcher?.Stop();
            }

            IsLoading = false;
        }

        [RelayCommand]
        private void OnSizeChanged(Size size)
        {
            if(Widget == null) return;

            Size = WidgetSize.GetSize(size);

            SetLayoutState(Size);
        }

        [RelayCommand]
        private void OnUnpinned()
        {
            _cpuWatcher?.Stop();
        }

        [RelayCommand]
        private async Task RequestPermissionAsync(Widget widget)
        {
            try
            {
                var access = await _permission.RequestAccessAsync(new Permission(Scopes.SystemInformation));

                if(access == PermissionState.Allowed)
                {
                    IsPermissionUIVisible = false;
                    AppearedCommand.Execute(widget);
                }
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        [RelayCommand]
        private void LaunchSettings(Widget widget)
        {
            if(Widget == null) return;

            ShellHelper.LaunchSettingsById(Widget.Id);
        }

        #endregion

        #region Handlers

        private void OnCpuUtilizationChanged(object sender, List<int> e)
        {
            if(e == null || e.Count == 0) return;

            App.Current.Dispatcher.Invoke(() =>
            {
                if(CpuUsageValues == null)
                {
                    CpuUsageValues = new ObservableCollection<int>(e);
                    return;
                }

                if(CpuUsageValues.Count >= 30 && Size != WidgetSizes.Small)
                   CpuUsageValues.RemoveAt(0);

                CpuUtilization = e.Last();

                if(Size != WidgetSizes.Small)
                   CpuUsageValues.Add(CpuUtilization);
            });
        }

        private void OnPermissionRevoked(object sender, PermissionChangedEventArgs e)
        {
            if(e.Permission == null) return;
            if(e.Permission.Scope == Scopes.SystemInformation)
            {
                var access = _permission.TryCheckPermissionState(e.Permission);

                if(access != PermissionState.Allowed)
                {
                    IsPermissionUIVisible = true;
                    _cpuWatcher?.Stop();
                }
            }
        }

        private async void OnSettingsValueChanged(object sender, string e)
        {
            if(nameof(EnableChartAnimation) == e)
            {
                OnPropertyChanged(nameof(EnableChartAnimation));
                await DrawChartAsync();
            }

            if(nameof(ChartOpacity) == e)
               OnPropertyChanged(nameof(ChartOpacity));
        }

        #endregion
    }
}
