using BetterWidgets.Attributes;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Tests.Attributes;

namespace BetterWidgets.Tests.Widgets
{
    [TestGuid]
    [WidgetPermissions([Scopes.SystemInformation])]
    public sealed class CpuWidget : Widget
    {
    }
}
