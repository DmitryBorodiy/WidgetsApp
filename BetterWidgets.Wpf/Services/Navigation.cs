using BetterWidgets.Consts;
using BetterWidgets.Views;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Navigation;

namespace BetterWidgets.Services
{
    public sealed class Navigation : INavigation
    {
        #region Services
        private readonly ILogger<Navigation> _logger;
        private MainWindow _mainWindow;
        #endregion

        public Navigation(ILogger<Navigation> logger, MainWindow mainWindow)
        {
            _logger = logger;
            _mainWindow = mainWindow;
        }

        public bool IsShellActive => GetIsActivated();

        public bool CanGoBack => GetCanGoBack();
        public bool CanGoForward => GetCanGoForward();

        private bool CanNavigateShell()
            => _mainWindow != null && _mainWindow.IsLoaded;

        private bool GetIsActivated()
            => _mainWindow.IsLoaded &&
               (_mainWindow.WindowState == WindowState.Normal ||
                _mainWindow.WindowState == WindowState.Maximized);

        public NavigationService GetShellNavigationService()
        {
            if(!IsShellActive)
               ActivateShell();

            return _mainWindow.VM.Frame.NavigationService;
        }

        public void ActivateShell()
        {
            if(CanNavigateShell())
            {
                _mainWindow.Show();
                _mainWindow.Activate();

                _mainWindow.WindowState = WindowState.Minimized;
                _mainWindow.WindowState = WindowState.Normal;
            }
            else
            {
                _mainWindow = new MainWindow();

                _mainWindow.Show();
                _mainWindow.Activate();
            }
        }

        private bool GetCanGoBack()
        {
            if(!IsShellActive) return false;

            var shell = GetShellNavigationService();
            return shell.CanGoBack;
        }

        public void GoBack()
        {
            if(IsShellActive)
            {
                var shell = GetShellNavigationService();

                if(shell.CanGoBack) shell.GoBack();
            }
        }

        private bool GetCanGoForward()
        {
            if(!IsShellActive) return false;

            var shell = GetShellNavigationService();
            return shell.CanGoForward;
        }

        public void GoForward()
        {
            if(IsShellActive)
            {
                var shell = GetShellNavigationService();

                if(shell.CanGoForward) shell.GoForward();
            }
        }

        public void Navigate(object view, object args = null)
        {
            var shell = GetShellNavigationService();

            shell.Navigate(view, args);
        }
    }
}
