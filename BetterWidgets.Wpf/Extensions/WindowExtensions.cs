using System.Windows;
using System.Windows.Interop;

namespace BetterWidgets.Extensions
{
    public static class WindowExtensions
    {
        public static IntPtr GetHwnd(this Window window)
            => new WindowInteropHelper(window.IsActive ? window : Application.Current.MainWindow).Handle;
    }
}
