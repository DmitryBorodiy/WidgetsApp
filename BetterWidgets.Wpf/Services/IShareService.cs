using BetterWidgets.Controls;
using System.Windows.Media.Imaging;

namespace BetterWidgets.Services
{
    public interface IShareService
    {
        bool AllowShare { get; }

        (bool success, Exception ex) RequestShare<T>(T shareData, Widget widget, string title = null, string description = null);
        (BitmapImage image, Exception ex) CreateWidgetCard(Widget widget);
    }
}
