using BetterWidgets.Consts;
using BetterWidgets.Extensions;
using BetterWidgets.Services;
using BetterWidgets.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.Helpers
{
    public class ShellHelper
    {
        public static MainWindow GetAppShell() => App.Services?.GetService<MainWindow>();
        public static IntPtr GetAppShellHwnd()
        {
            var shell = GetAppShell();

            if(shell == null) throw new InvalidOperationException(Errors.CannotAccessAppShell);

            return shell.GetHwnd();
        }

        public static void GoBack()
        {
            var navigation = App.Services?.GetService<INavigation>();

            if(navigation.CanGoBack) navigation.GoBack();
        }

        public static void LaunchSettingsById(Guid id, object parameter = null)
        {
            var navigation = App.Services?.GetService<INavigation>();
            var settings = App.Services?.GetService<ISettingsManager>();

            if(settings == null) return;

            var page = settings.GetById(id.ToString());

            if(page == null) return;
            if(navigation.IsShellActive) navigation.ActivateShell();

            navigation.Navigate(page, parameter);
        }
    }
}
