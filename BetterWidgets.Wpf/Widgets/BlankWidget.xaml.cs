using System.Windows;
using BetterWidgets.Controls;
using System.Runtime.InteropServices;
using BetterWidgets.Attributes;
using BetterWidgets.Consts;
using Wpf.Ui.Controls;

namespace BetterWidgets.Widgets
{
    [DevMode]
    [Guid("87cd8a59-399a-4843-b467-f709af0f6928")]
    [WidgetPermissions([Scopes.Clipboard, Scopes.Location])]
    [WidgetTitle(TitleKey)]
    [WidgetSubtitle(SubtitleKey)]
    [WidgetIcon(IconSource)]
    public partial class BlankWidget : Widget
    {
        private const string TitleKey = "Blank widget";
        private const string SubtitleKey = "This is the sample blank widget.";
        private const string IconSource = "pack://application:,,,/Assets/BlankWidget/icon-48.png";

        public BlankWidget()
        {
            InitializeComponent();
            Loaded += BlankWidget_Loaded;
        }

        private void BlankWidget_Loaded(object sender, RoutedEventArgs e)
        {
            SetTitleBar(TitleBarUI);
        }

        private void OnHelloClick(object sender, RoutedEventArgs e) 
        {
            ShowNotify
            (
                "This is just sample message for notify API check.", 
                "Hello world!", 
                true, 
                InfoBarSeverity.Success,
                true,
                TimeSpan.FromSeconds(10)
            );
        }
    }
}
