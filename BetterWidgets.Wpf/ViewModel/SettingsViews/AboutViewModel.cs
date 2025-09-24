using BetterWidgets.Helpers;
using BetterWidgets.Properties;
using BetterWidgets.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace BetterWidgets.ViewModel.SettingsViews
{
    public class AboutViewModel : ObservableObject
    {
        private readonly Configuration _config;

        public AboutViewModel()
        {
            _config = App.Services?.GetService<Configuration>();
        }

        #region Props

        public string Version => _config?.ProductVersion ?? string.Empty;

        public string Build => _config?.ProductBuild ?? string.Empty;

        public string Chanel => _config?.ProductChanel ?? string.Empty;

        public string Developer => _config?.ProductDeveloper ?? string.Empty;

        #endregion

        public ICommand BackCommand => new RelayCommand(ShellHelper.GoBack);
    }
}
