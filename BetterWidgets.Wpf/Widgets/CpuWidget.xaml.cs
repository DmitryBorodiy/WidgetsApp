using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Attributes;
using System.Runtime.InteropServices;

namespace BetterWidgets.Widgets
{
    [Guid("f6a83396-7d7c-4c7c-bb35-3d8dff4bb216")]
    [WidgetPermissions([Scopes.SystemInformation])]
    [WidgetTitle(nameof(CpuWidget), true)]
    [WidgetSubtitle(CpuWidgetSubtitle, true)]
    [WidgetIcon(IconSource)]
    public partial class CpuWidget : Widget
    {
        #region Consts
        private const string CpuWidgetSubtitle = nameof(CpuWidgetSubtitle);
        private const string IconSource = "pack://application:,,,/Assets/CpuWidget/icon-48.png";
        #endregion

        public CpuWidget() => InitializeComponent();
    }
}
