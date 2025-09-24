using BetterWidgets.Attributes;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using System.Runtime.InteropServices;

namespace BetterWidgets.Tests.Widgets
{
    [Guid("b788407e-89e2-4318-ab54-92b9a8662683")]
    [WidgetPermissions([Scopes.Tasks])]
    public sealed partial class ToDoWidget : Widget
    {
    }
}
