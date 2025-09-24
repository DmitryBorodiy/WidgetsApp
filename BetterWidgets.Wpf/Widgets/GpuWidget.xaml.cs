using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Attributes;
using System.Runtime.InteropServices;

namespace BetterWidgets.Widgets
{
    [WidgetIcon(IconSource)]
    [WidgetTitle(nameof(GpuWidget), true)]
    [WidgetSubtitle(GpuWidgetSubtitle, true)]
    [Guid("bbe11785-b1cb-4a7d-adc3-571fb58ae96a")]
    [WidgetPermissions([Scopes.SystemInformation])]
    public partial class GpuWidget : Widget
    {
        #region Consts
        private const string GpuWidgetSubtitle = nameof(GpuWidgetSubtitle);
        private const string IconSource = "pack://application:,,,/Assets/GpuWidget/icon-48.png";
        #endregion

        public GpuWidget() => InitializeComponent();
    }
}
