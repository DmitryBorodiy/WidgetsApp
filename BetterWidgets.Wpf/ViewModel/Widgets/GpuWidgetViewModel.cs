using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Enums;
using BetterWidgets.Events;
using BetterWidgets.Helpers;
using BetterWidgets.Model;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.ViewModel.Widgets.Components;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace BetterWidgets.ViewModel.Widgets
{
    public partial class GpuWidgetViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings _settingsBase;
        private readonly Settings<GpuWidget> _settings;
        private readonly IPermissionManager _permissions;
        private readonly ISystemInformation<GpuWidget> _systemInfo;
        #endregion

        public GpuWidgetViewModel()
        {
            _settings = App.Services?.GetRequiredService<Settings<GpuWidget>>();
            _logger = App.Services?.GetService<ILogger<GpuWidgetViewModel>>();
            _systemInfo = App.Services?.GetRequiredService<ISystemInformation<GpuWidget>>();
            _permissions = App.Services?.GetRequiredService<IPermissionManager>();
            _settingsBase = App.Services?.GetRequiredService<Settings>();

            _settings.ValueChanged += OnValueChanged;
            _permissions.PermissionChanged += OnPermissionChanged;
            _settingsBase.ValueChanged += OnSettingChanged;
        }

        #region Props

        private Guid Id { get; set; }
        private Widget Widget { get; set; }
        private string SelectedGpuName
        {
            get => _settings.GetSetting<string>(nameof(SelectedGpuName));
            set => _settings.SetSetting(nameof(SelectedGpuName), value);
        }

        [ObservableProperty] public bool isLoading;

        [ObservableProperty] public bool isPrivacyMessageVisible;

        [ObservableProperty] public bool isMainUIVisible = true;

        [ObservableProperty]
        public ObservableCollection<GpuInformationViewModel> gpuDevices;

        [ObservableProperty]
        public GpuInformationViewModel selectedGpuDevice;

        [ObservableProperty] public TimeSpan animationSpeed = TimeSpan.FromSeconds(1);

        [ObservableProperty] public double chartOpacity = 0.8;

        [ObservableProperty] public ImageSource gpuIconSource;

        #region Settings

        public bool EnableChartAnimation => _settings.GetSetting(nameof(EnableChartAnimation), true);
        public double ChartOpacityValue => _settings.GetSetting<double>(nameof(ChartOpacity), 0.8);
        public double UpdateInterval => _settings.GetSetting<double>(nameof(UpdateInterval), 1000);

        #endregion

        #endregion

        #region Utils

        private void SetLayoutState(WidgetSizes size)
        {
            try
            {
                if(Widget?.Content is Panel rootLayout)
                {
                    var mainUI = rootLayout.Children[0] as Grid;

                    if(mainUI.Resources.Contains(size.ToString()) &&
                       mainUI.Resources[size.ToString()] is Style rootLayoutStyle)
                       mainUI.Style = rootLayoutStyle;

                    foreach(var element in mainUI.Children)
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
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        private ImageSource GetGpuIcon()
        {
            string fileName = "gpu-ic-{0}.png";
            string currentTheme = _settingsBase.IsDarkMode ? "dark" : "light";
            fileName = string.Format(fileName, currentTheme);

            return IconHelper.GetWidgetAssetIcon<GpuWidget>(fileName);
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void LaunchSettings()
        {
            if (Id == Guid.Empty) return;

            ShellHelper.LaunchSettingsById(Id);
        }

        [RelayCommand]
        private async Task OnLoadedAsync(Widget widget)
        {
            Widget = widget;
            IsLoading = true;
            Id = widget?.Id ?? Guid.Empty;
            GpuIconSource = GetGpuIcon();
            ChartOpacity = ChartOpacityValue;
            SizeChangedCommand.Execute(widget?.Size);
            AnimationSpeed = TimeSpan.FromSeconds(EnableChartAnimation ? 1 : 0);

            var access = _permissions.TryCheckPermissionState(Id, new Permission(Scopes.SystemInformation));

            if(access == PermissionState.Allowed)
            {
                var gpuInfo = await _systemInfo.GetGpuInformationAsync();

                if(gpuInfo.ex != null)
                {
                    Widget?.ShowNotify(
                        gpuInfo.ex.Message,
                        Resources.Resources.Failed,
                        true, InfoBarSeverity.Error);

                    return;
                }

                if(gpuInfo.gpus?.Any() ?? false)
                {
                    int hardwareIndex = 0;

                    var gpuViewModels = gpuInfo.gpus.Select(g =>
                    {
                        var gpuViewModel = new GpuInformationViewModel(g, hardwareIndex);

                        hardwareIndex++;

                        return gpuViewModel;
                    });

                    GpuDevices = new ObservableCollection<GpuInformationViewModel>(gpuViewModels);

                    SelectedGpuDevice = _settings.ContainsKey(nameof(SelectedGpuName)) ?
                        GpuDevices.FirstOrDefault(d => d.Name == SelectedGpuName) ?? GpuDevices.FirstOrDefault() :
                        GpuDevices.FirstOrDefault();
                }
            }
            else
            {
                IsMainUIVisible = false;
                IsPrivacyMessageVisible = true;
            }

            IsLoading = false;
        }

        [RelayCommand]
        private void OnUnpinned()
        {
            if (GpuDevices?.Any() ?? false)
            {
                foreach (var device in GpuDevices)
                    device.StopWatchingCommand.Execute(default);
            }
        }

        [RelayCommand]
        private void OnSizeChanged(Size size)
        {
            if(Widget == null) return;

            var widgetSize = WidgetSize.GetSize(size);

            SetLayoutState(widgetSize);
        }

        [RelayCommand]
        private void OnStateChanged(WidgetState state)
        {
            if (state != WidgetState.Activated)
                UnpinnedCommand.Execute(default);
            else
                LoadedCommand.Execute(Widget);
        }

        [RelayCommand]
        private async Task RequestAccessAsync()
        {
            await _permissions.RequestAccessAsync(Id, new Permission(Scopes.SystemInformation));
        }

        #endregion

        #region Handlers

        partial void OnSelectedGpuDeviceChanged(GpuInformationViewModel oldValue, GpuInformationViewModel newValue)
        {
            if (newValue == null) return;
            if (newValue == oldValue) return;

            if (oldValue != null) oldValue.StopWatchingCommand.Execute(default);

            SelectedGpuName = newValue.Name;
            newValue.StartWatchingCommand.Execute(default);
        }

        private void OnValueChanged(object sender, string e)
        {
            if(string.IsNullOrEmpty(e)) return;

            if(e == nameof(EnableChartAnimation))
            {
                AnimationSpeed = TimeSpan.FromSeconds(EnableChartAnimation ? 1 : 0);
                LoadedCommand.Execute(Widget);
            }

            if(e == nameof(ChartOpacity))
               ChartOpacity = ChartOpacityValue;
        }

        private void OnPermissionChanged(object sender, PermissionChangedEventArgs e)
        {
            if(e.Permission == null) return;

            if(e.Permission.Scope == Scopes.SystemInformation)
            {
                if(e.Permission.State == PermissionState.Allowed)
                {
                    IsMainUIVisible = true;
                    IsPrivacyMessageVisible = false;

                    LoadedCommand.Execute(Widget);
                }
                else
                {
                    IsMainUIVisible = false;
                    IsPrivacyMessageVisible = true;

                    UnpinnedCommand.Execute(default);
                }
            }
        }

        private void OnSettingChanged(object sender, string e)
        {
            if(e == nameof(_settingsBase.IsDarkMode))
               GpuIconSource = GetGpuIcon();
        }

        #endregion
    }
}
