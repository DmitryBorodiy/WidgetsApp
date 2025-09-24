using System.Windows.Media;

namespace BetterWidgets.Abstractions
{
    public interface IMediaPlayerService
    {
        MediaPlayer Player { get; }

        Task PlayAsync(Uri source);
    }
}
