using System.IO;
using System.Windows;
using System.Windows.Media;
using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using Microsoft.Extensions.Logging;

namespace BetterWidgets.Services
{
    public class MediaPlayerService : IMediaPlayerService
    {
        #region Services
        private readonly ILogger _logger;
        #endregion

        public MediaPlayerService(ILogger<MediaPlayerService> logger, MediaPlayer player)
        {
            _logger = logger;
            Player = player;
        }

        #region Props

        public MediaPlayer Player { get; private set; }

        #endregion

        public async Task PlayAsync(Uri source)
        {
            try
            {
                if(source == null) throw new ArgumentNullException(nameof(source));
                if(Player == null) throw new InvalidOperationException(Errors.MediaPlayerIsNotInitialized);

                string tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(source.AbsoluteUri));

                if(!File.Exists(tempPath))
                {
                    using var stream = Application.GetResourceStream(source).Stream;
                    using var fileStream = File.Create(tempPath);
                    
                    await stream.CopyToAsync(fileStream);
                }

                Player.Open(new Uri(tempPath));
                Player.Play();
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }
    }
}
