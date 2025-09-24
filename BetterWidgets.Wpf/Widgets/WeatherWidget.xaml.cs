using BetterWidgets.Attributes;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Enums;
using BetterWidgets.ViewModel.Widgets;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BetterWidgets.Widgets
{
    [Guid("b3024980-9783-41f9-89d5-399f2698cc7f")]
    [WidgetPermissions([Scopes.Location])]
    [WidgetTitle(WeatherWidgetTitle, true)]
    [WidgetSubtitle(WeatherWidgetSubtitle, true)]
    [WidgetIcon(IconSource)]
    public partial class WeatherWidget : Widget
    {
        private const string WeatherWidgetTitle = nameof(WeatherWidgetTitle);
        private const string WeatherWidgetSubtitle = nameof(WeatherWidgetSubtitle);
        private const string IconSource = "pack://application:,,,/Assets/WeatherWidget/icon-48.png";

        public WeatherWidget()
        {
            InitializeComponent();

            SizeChanged += WeatherWidget_SizeChanged;
        }

        public void SetUpdateInterval(double minutes)
        {
            if(DataContext is WeatherViewModel vm)
               vm.SetWeatherUpdateInterval(TimeSpan.FromMinutes(minutes));
        }

        private void WeatherWidget_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var size = WidgetSize.GetSize(e.NewSize);

            switch(size)
            {
                case WidgetSizes.Small:

                    VisualStateManager.GoToElementState(AdditionalInfoUI, "InfoCollapsed", true);

                    break;
                case WidgetSizes.Medium:

                    VisualStateManager.GoToElementState(AdditionalInfoUI, "InfoVisible", true);
                    VisualStateManager.GoToElementState(DailyForecastUI, "ForecastCollapsed", true);

                    break;
                case WidgetSizes.Large:

                    VisualStateManager.GoToElementState(DailyForecastUI, "ForecastVisible", true);

                    break;
            }
        }

        private void OnForecastMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            if(scrollViewer == null) return;

            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);            

            e.Handled = true;
        }
    }
}
