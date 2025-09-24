using System.Windows;
using System.Windows.Controls;
using BetterWidgets.ViewModel.SettingsViews;

namespace BetterWidgets.Views.Settings
{
    public partial class SettingsView : Page
    {
        public SettingsView()
        {
            InitializeComponent();

            Loaded += SettingsView_Loaded;
        }

        public SettingsViewModel VM => DataContext as SettingsViewModel;

        private void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            VM?.LoadedCommand?.Execute(this);
        }
    }
}
