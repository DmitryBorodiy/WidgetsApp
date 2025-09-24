using BetterWidgets.Abstractions;
using BetterWidgets.ViewModel;
using System.Windows;
using Wpf.Ui.Controls;

namespace BetterWidgets.Views.Dialogs
{
    public partial class PermissionsDialog : FluentWindow
    {
        private readonly WidgetMetadata _widget;

        public PermissionsDialog() => InitializeComponent();
        public PermissionsDialog(WidgetMetadata widget)
        {
            _widget = widget;

            InitializeComponent();
            Loaded += PermissionsDialog_Loaded;
        }

        private void PermissionsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if(DataContext is PermissionsViewModel vm)
               vm.Widget = _widget;
        }
    }
}
