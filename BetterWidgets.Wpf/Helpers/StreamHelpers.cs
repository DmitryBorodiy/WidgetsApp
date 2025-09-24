using System.IO;
using Windows.Storage.Streams;
using System.Windows.Media.Imaging;

namespace BetterWidgets.Helpers
{
    public class StreamHelpers
    {
        public static RandomAccessStreamReference ConvertBitmapImageToStreamReference(BitmapImage bitmapImage)
        {
            if(bitmapImage == null) throw new ArgumentNullException(nameof(bitmapImage));

            using(var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(stream);

                stream.Position = 0;

                var randomAccessStream = new InMemoryRandomAccessStream();
                var outputStream = randomAccessStream.AsStreamForWrite();
                stream.CopyTo(outputStream);
                outputStream.Flush();
                randomAccessStream.Seek(0);

                return RandomAccessStreamReference.CreateFromStream(randomAccessStream);
            }
        }
    }
}
