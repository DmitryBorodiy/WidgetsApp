using BetterWidgets.Attributes;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Tests.Attributes;

namespace BetterWidgets.Tests.Widgets
{
    [TestGuid]
    [WidgetPermissions([Scopes.Appointments])]
    public class CalendarWidget : Widget
    {
    }
}
