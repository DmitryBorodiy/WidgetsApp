using BetterWidgets.Attributes;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.ViewModel.Widgets;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace BetterWidgets.Widgets
{
    [Guid("ea9b5064-93ff-40b1-8374-27ad28db6dec")]
    [WidgetPermissions([Scopes.Appointments])]
    [WidgetTitle(CalendarWidgetTitle, true)]
    [WidgetSubtitle(CalendarWidgetSubtitle, true)]
    [WidgetIcon(IconSource)]
    public partial class CalendarWidget : Widget
    {
        private const string CalendarWidgetTitle = nameof(CalendarWidgetTitle);
        private const string CalendarWidgetSubtitle = nameof(CalendarWidgetSubtitle);
        private const string IconSource = "pack://application:,,,/Assets/CalendarWidget/icon-48.png";

        public CalendarWidget() => InitializeComponent();

        private CalendarWidgetViewModel VM => DataContext as CalendarWidgetViewModel;

        private void OnAppointmentsMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VM.ViewAppointmentCommand.Execute(default);
        }
    }
}
