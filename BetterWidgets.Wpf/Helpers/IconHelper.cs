using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BetterWidgets.Consts;
using Wpf.Ui.Controls;

namespace BetterWidgets.Helpers
{
    public class IconHelper
    {
        public static IconElement CreateRegular(SymbolRegular symbol) => new SymbolIcon()
        {
            Symbol = symbol
        };

        public static IconElement CreateFilled(SymbolRegular symbol) => new SymbolIcon()
        {
            Filled = true,
            Symbol = symbol,
            Foreground = (SolidColorBrush)Application.Current.Resources[KnownResources.SystemAccentColorBrush]
        };

        public static ImageSource GetWidgetAssetIcon<T>(string fileName)
        {
            string packPath = "pack://application:,,,/Assets";
            string path = Path.Combine(packPath, typeof(T).Name, fileName);
            var uri = new Uri(path);

            return new BitmapImage(uri);
        }
    }
}
