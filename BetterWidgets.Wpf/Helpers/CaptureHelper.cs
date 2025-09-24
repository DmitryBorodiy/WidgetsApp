using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BetterWidgets.Consts;

namespace BetterWidgets.Helpers
{
    public class CaptureHelper
    {
        public static BitmapImage CaptureControl(FrameworkElement control)
        {
            if(control == null) throw new ArgumentNullException(nameof(control));
            if(!control.IsLoaded) throw new InvalidOperationException(Errors.UIControlIsNotLoaded);

            var dpi = 96d;
            var width = (int)control.ActualWidth;
            var height = (int)control.ActualHeight;

            var rtb = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);
            rtb.Render(control);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);
            memoryStream.Position = 0;

            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
