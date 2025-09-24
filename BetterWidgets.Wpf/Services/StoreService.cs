using BetterWidgets.Abstractions;
using Microsoft.Extensions.Logging;
using Windows.Services.Store;
using BetterWidgets.Consts;
using BetterWidgets.Helpers;
using BetterWidgets.Extensions;
using WinRT.Interop;
using BetterWidgets.Properties;
using Windows.System;

namespace BetterWidgets.Services
{
    public sealed class StoreService : IStoreService
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Configuration _config;

        private StoreContext _store;
        #endregion

        public StoreService(ILogger<StoreService> logger, Configuration configuration)
        {
            _logger = logger;
            _config = configuration;
        }

        public async Task<(StoreProduct product, Exception ex)> GetProductAsync(string productId)
        {
            try
            {
                if(string.IsNullOrEmpty(productId)) throw new ArgumentNullException(Errors.IdNullOrEmpty);
                
                if(_store == null) _store = StoreContext.GetDefault();

                IntPtr hwnd = ShellHelper.GetAppShellHwnd();
                InitializeWithWindow.Initialize(_store, hwnd);

                var product = await _store.GetStoreProductsAsync(["Durable"], [productId]);

                if(product.ExtendedError != null) throw product.ExtendedError;

                return (product.Products.Values.FirstOrDefault(), null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task RateReviewAsync()
        {
            try
            {
                string uri = Endpoints.StoreRateReviewUri + _config.StorePfn;

                await Launcher.LaunchUriAsync(uri.ToUri());
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }
    }
}
