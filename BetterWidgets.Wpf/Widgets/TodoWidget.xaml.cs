using BetterWidgets.Attributes;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using System.Runtime.InteropServices;

namespace BetterWidgets.Widgets
{
    [Guid("b788407e-89e2-4318-ab54-92b9a8662683")]
    [WidgetPermissions([Scopes.Tasks])]
    [WidgetTitle(TodoWidgetTitle, true)]
    [WidgetSubtitle(TodoWidgetSubtitle, true)]
    [WidgetIcon(IconSource)]
    public partial class TodoWidget : Widget
    {
        private const string TodoWidgetTitle = nameof(TodoWidgetTitle);
        private const string TodoWidgetSubtitle = nameof(TodoWidgetSubtitle);
        private const string IconSource = "pack://application:,,,/Assets/Todo/icon-48.png";

        public TodoWidget() => InitializeComponent();
    }
}
