using BetterWidgets.Widgets;
using BetterWidgets.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.ViewModel.WidgetsSettingsViews
{
    public partial class GpuWidgetSettingsViewModel : ObservableObject
    {
        #region Services
        private readonly Settings<GpuWidget> _settings;
        #endregion

        public GpuWidgetSettingsViewModel()
        {
            _settings = App.Services?.GetRequiredService<Settings<GpuWidget>>();
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

        public int UpdateInterval
        {
            get => _settings.GetSetting(nameof(UpdateInterval), 1000);
            set
            {
                _settings.SetSetting(nameof(UpdateInterval), value);
                OnPropertyChanged(nameof(UpdateInterval));
            }
        }

        #endregion
    }
}
