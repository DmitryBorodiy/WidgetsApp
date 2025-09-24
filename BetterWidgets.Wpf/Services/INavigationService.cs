using System.Windows.Navigation;

namespace BetterWidgets.Services
{
    public interface INavigation
    {
        bool IsShellActive { get; }

        bool CanGoBack { get; }
        bool CanGoForward { get; }

        void ActivateShell();
        void Navigate(object view, object args = null);
        void GoBack();
        void GoForward();

        NavigationService GetShellNavigationService();
    }
}
