using BetterWidgets.Consts;
using BetterWidgets.Helpers;
using BetterWidgets.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Windows.Storage;

namespace BetterWidgets.Services
{
    public sealed class MsalService : IMsalService
    {
        #region Consts
        private const string ACCOUNT_MSAL_TOKEN = nameof(ACCOUNT_MSAL_TOKEN);
        private const string ACCOUNT_CACHE_FOLDER = nameof(ACCOUNT_CACHE_FOLDER);
        #endregion

        #region Services
        private readonly ILogger _logger;
        private readonly Configuration _config;
        private readonly IDataService _data;
        #endregion

        public MsalService(ILogger<MsalService> logger, Configuration configuration, IDataService data)
        {
            _logger = logger;
            _config = configuration;
            _data = data;
        }

        private static IAccount CurrentAccount { get; set; }
        private static IPublicClientApplication ClientApp { get; set; }

        private async Task ConfigureAsync()
        {
            var tokenCache = await _data.GetBytesFromFileAsync(ACCOUNT_MSAL_TOKEN, ACCOUNT_CACHE_FOLDER);

            ClientApp = BuildPublicClientApp(ApiKeys.MSGraphClient);

            ClientApp.UserTokenCache.SetBeforeAccess(o =>
            {
                if(tokenCache != null && tokenCache.Length > 0)
                   o.TokenCache.DeserializeAdalV3(tokenCache);
            });

            ClientApp.UserTokenCache.SetAfterAccess(async o =>
            {
                if(o.HasStateChanged && o.Account != null && o.HasTokens)
                {
                    byte[] token = o.TokenCache.SerializeAdalV3();

                    await _data.SaveBytesToFileAsync(ACCOUNT_MSAL_TOKEN, token, ACCOUNT_CACHE_FOLDER);
                }
            });
        }

        private IPublicClientApplication BuildPublicClientApp(string clientId) 
            => PublicClientApplicationBuilder.Create(clientId)
                      .WithRedirectUri("http://localhost")
                      .Build();

        public async Task<IAccount> TryGetAccountAsync(string accountId = default)
        {
            var accounts = await TryGetAllAccountsAsync();
            
            var account = string.IsNullOrEmpty(accountId) ?
                          accounts.FirstOrDefault() :
                          accounts.FirstOrDefault(a => a.HomeAccountId.Identifier == accountId);

            return account;
        }

        public async Task<IEnumerable<IAccount>> TryGetAllAccountsAsync()
            => await ClientApp?.GetAccountsAsync() ?? Enumerable.Empty<IAccount>();

        public async Task<AuthenticationResult> AuthenticateAsync(string accoutId = default, CancellationToken token = default)
        {
            if(!NetworkHelper.IsConnected) return null;
            if(ClientApp == null) await ConfigureAsync();

            CurrentAccount = await TryGetAccountAsync(accoutId);

            try
            {
                return await ClientApp.AcquireTokenSilent(_config.MSGraph.Scopes, CurrentAccount)
                                      .ExecuteAsync(token);
            }
            catch(MsalUiRequiredException)
            {
                return await ClientApp.AcquireTokenInteractive(_config.MSGraph.Scopes)
                                      .WithUseEmbeddedWebView(false)
                                      .WithAccount(CurrentAccount)
                                      .WithPrompt(Prompt.SelectAccount)
                                      .ExecuteAsync(token);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        public async Task ForgetTokenAndSignOutAsync(string accountId = null)
        {
            try
            {
                var account = string.IsNullOrEmpty(accountId) ? 
                              CurrentAccount : await TryGetAccountAsync(accountId);

                if(account != null)
                {
                    ClientApp?.RemoveAsync(account);

                    var tokenCache = await _data.GetFileAsync(ACCOUNT_MSAL_TOKEN, ACCOUNT_CACHE_FOLDER);

                    await tokenCache?.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }
    }
}
