using Microsoft.Graph;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using BetterWidgets.Properties;
using BetterWidgets.Helpers;
using Windows.Networking.Connectivity;
using System.Windows.Threading;

namespace BetterWidgets.Services
{
    public sealed class MSGraphService : IMSGraphService
    {
        #region Services
        private readonly ILogger _logger;
        private readonly IMsalService _msalService;
        private readonly Settings _settings;
        #endregion

        public MSGraphService(ILogger<MSGraphService> logger, IMsalService msalService, Settings settings)
        {
            _logger = logger;
            _settings = settings;
            _msalService = msalService;

            AccessTokenUpdateInterval = new DispatcherTimer();

            AccessTokenUpdateInterval.Tick += OnTokenUpdateIntervalTick;
            NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
        }

        #region Props

        private DispatcherTimer AccessTokenUpdateInterval { get; set; }

        private AuthenticationResult AuthResult { get; set; }
        public GraphServiceClient Client { get; private set; }

        public bool IsSignedIn
        {
            get => _settings.GetValue(nameof(IsSignedIn), false);
            private set => _settings.SetValue(nameof(IsSignedIn), value);
        }

        #endregion

        #region Events
        public event EventHandler SignedIn;
        public event EventHandler SignedOut;
        #endregion

        #region PublicMembers

        public async Task<GraphServiceClient> SignInAsync(bool raiseEvent = true)
        {
            try
            {
                if(!NetworkHelper.IsConnected) return null;

                AuthResult = await _msalService.AuthenticateAsync();

                CalculateExpireTime(AuthResult.ExpiresOn);

                var authProvider = new DelegateAuthenticationProvider(() => Task.FromResult(AuthResult.AccessToken));
                Client = new GraphServiceClient(authProvider);

                IsSignedIn = true;
                if(raiseEvent) SignedIn?.Invoke(this, EventArgs.Empty);

                return Client;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        public async Task SignOutAsync(bool raiseEvent = true)
        {
            await _msalService.ForgetTokenAndSignOutAsync();

            IsSignedIn = false;
            if(raiseEvent) SignedOut?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region PrivateMembers

        private async void RefreshToken()
        {
            if(NetworkHelper.IsConnected)
               await SignInAsync(false);
        }

        private void CalculateExpireTime(DateTimeOffset expiresOn)
        {
            var expiresIn = expiresOn - DateTimeOffset.UtcNow;

            if(expiresIn <= TimeSpan.Zero) expiresIn = TimeSpan.FromMinutes(5);

            AccessTokenUpdateInterval.Stop();
            AccessTokenUpdateInterval.Interval = expiresIn;
            AccessTokenUpdateInterval.Start();
        }

        private void OnNetworkStatusChanged(object sender) => RefreshToken();

        private void OnTokenUpdateIntervalTick(object sender, EventArgs e) => RefreshToken();

        #endregion
    }
}
