using System.Windows;
using System.Windows.Input;
using BetterWidgets.Helpers;
using BetterWidgets.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BetterWidgets.ViewModel.WidgetsSettingsViews
{
    public partial class BlankWidgetSettingsViewModel : ObservableObject
    {
        private readonly MainWindow mainWindow;

        public BlankWidgetSettingsViewModel()
        {
            mainWindow = Application.Current.MainWindow as MainWindow;
        }

        #region Commands

        public ICommand GoBackCommand => new RelayCommand(ShellHelper.GoBack);

        #endregion
    }
}
