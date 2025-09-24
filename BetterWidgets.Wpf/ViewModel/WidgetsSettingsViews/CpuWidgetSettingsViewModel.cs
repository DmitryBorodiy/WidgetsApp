using System.Windows.Input;
using BetterWidgets.Helpers;
using BetterWidgets.Widgets;
using BetterWidgets.Properties;
using BetterWidgets.Abstractions;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.ViewModel.WidgetsSettingsViews
{
    public partial class CpuWidgetSettingsViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly ICpuWatcher _cpuWatcher;
        private readonly Settings<CpuWidget> _settings;
        #endregion

        public CpuWidgetSettingsViewModel()
        {
            _logger = App.Services?.GetService<ILogger<CpuWidget>>();
            _cpuWatcher = App.Services?.GetRequiredService<ICpuWatcher>();
            _settings = App.Services?.GetRequiredService<Settings<CpuWidget>>();
        }

        #region Props

        public bool EnableChartAnimation
        {
            get => _settings.GetSetting(nameof(EnableChartAnimation), true);
            set
            {
                _settings.SetSetting(nameof(EnableChartAnimation), value);
                OnPropertyChanged(nameof(EnableChartAnimation));
            }
        }

        public double ChartOpacity
        {
            get => _settings.GetSetting(nameof(ChartOpacity), 0.6);
            set
            {
                _settings.SetSetting(nameof(ChartOpacity), value);
                OnPropertyChanged(nameof(ChartOpacity));
            }
        }

        public double UpdateInterval
        {
            get => _cpuWatcher.UpdateTick;
            set
            {
                _cpuWatcher.UpdateTick = value;
                OnPropertyChanged(nameof(UpdateInterval));
            }
        }

        #endregion

        #region Commands

        public ICommand BackCommand => new RelayCommand(ShellHelper.GoBack);

        #endregion
    }
}
