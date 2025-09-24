using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Storage;
using BetterWidgets.Consts;
using BetterWidgets.Model;
using BetterWidgets.Properties;
using Microsoft.Extensions.Logging;

namespace BetterWidgets.Services
{
    public class MSAccountInformation : IMSAccountInformation
    {
        #region Consts
        private const string CACHED_ACCOUNT_ID = nameof(CACHED_ACCOUNT_ID);
        private const string CACHED_ACCOUNT_NAME = nameof(CACHED_ACCOUNT_NAME);
        private const string CACHED_ACCOUNT_EMAIL = nameof(CACHED_ACCOUNT_EMAIL);
        private const string CASHED_ACCOUNT_AVATAR = nameof(CASHED_ACCOUNT_AVATAR);

        private const string ACCOUNT_CACHE_FOLDER = nameof(ACCOUNT_CACHE_FOLDER);
        #endregion

        #region Services
        private readonly ILogger _logger;
        private readonly Settings _settings;
        private readonly IMSGraphService _graphService;
        private readonly IDataService _data;
        #endregion

        public MSAccountInformation(ILogger<MSAccountInformation> logger, IMSGraphService graphService, IDataService data, Settings settings)
        {
            _logger = logger;
            _graphService = graphService;
            _settings = settings;
            _data = data;
        }

        #region Props

        public bool IsSignedIn => _graphService?.IsSignedIn ?? false;

        #endregion

        private async Task<Account> TryGetCachedAccountInformation()
        {
            if(_settings == null) throw new InvalidOperationException(Errors.SettingsServiceNotLoaded);
            if(_graphService == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);

            if(!_settings.ContainsKey(nameof(_graphService.IsSignedIn))) return null;

            string id = _settings.GetValue<string>(CACHED_ACCOUNT_ID);
            string name = _settings.GetValue<string>(CACHED_ACCOUNT_NAME);
            string email = _settings.GetValue<string>(CACHED_ACCOUNT_EMAIL);

            if(string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email)) return null;

            var cachedAvatar = await TryGetCachedAvatarAsync();

            return new Account()
            {
                Id = id,
                DisplayName = name,
                Email = email,
                AvatarImageSource = cachedAvatar
            };
        }

        private void FlushCachedAccountInformation()
        {
            _settings.RemoveKey(CACHED_ACCOUNT_ID);
            _settings.RemoveKey(CACHED_ACCOUNT_NAME);
            _settings.RemoveKey(CACHED_ACCOUNT_EMAIL);
        }

        private async Task<ImageSource> TryGetCachedAvatarAsync()
        {
            var avatarFile = await _data.GetFileAsync(CASHED_ACCOUNT_AVATAR, ACCOUNT_CACHE_FOLDER);

            if(avatarFile == null) return null;

            var imageSource = new BitmapImage();
            using var stream = await avatarFile.OpenReadAsync();
            
            imageSource.BeginInit();
            imageSource.StreamSource = stream.AsStream();
            imageSource.CacheOption = BitmapCacheOption.OnLoad;
            imageSource.EndInit();

            return imageSource;
        }

        private async Task FlushCachedAvatarImageAsync()
        {
            var avatarFile = await _data.GetFileAsync(CASHED_ACCOUNT_AVATAR, ACCOUNT_CACHE_FOLDER);

            if(avatarFile != null)
               await avatarFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        private async Task<ImageSource> FetchAccountAvatarAsync()
        {
            try
            {
                using var avatarStream = await _graphService.Client.Me.Photo.Content.GetAsync();
                using var memoryStream = new MemoryStream();

                await avatarStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var image = new BitmapImage();

                image.BeginInit();
                image.StreamSource = memoryStream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();

                return image;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        public async Task<Account> GetAccountInformationAsync()
        {
            try
            {
                var account = await TryGetCachedAccountInformation();

                if(account == null)
                {
                    var user = await _graphService.Client.Me.GetAsync();
                    var userAvatar = await FetchAccountAvatarAsync();

                    if(user == null) throw new NullReferenceException(nameof(user));

                    account = new Account(user, userAvatar);
                }

                return account;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        public async Task FlushDataAsync()
        {
            try
            {
                FlushCachedAccountInformation();
                await FlushCachedAvatarImageAsync();
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }
    }
}
