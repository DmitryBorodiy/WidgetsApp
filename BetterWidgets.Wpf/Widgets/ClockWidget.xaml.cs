using BetterWidgets.Attributes;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using System.Runtime.InteropServices;

namespace BetterWidgets.Widgets
{
    [Guid("c1e7e1b2-94d3-4a64-a275-727abf697e2f")]
    [WidgetPermissions([Scopes.Appointments])]
    [WidgetTitle(ClockWidgetTitle, true)]
    [WidgetSubtitle(ClockWidgetSubtitle, true)]
    [WidgetIcon(IconSource)]
    public partial class ClockWidget : Widget
    {
        private const string ClockWidgetTitle = nameof(ClockWidgetTitle);
        private const string ClockWidgetSubtitle = nameof(ClockWidgetSubtitle);
        private const string IconSource = "pack://application:,,,/Assets/ClockWidget/icon-48.png";

        public ClockWidget() => InitializeComponent();
    }
}
