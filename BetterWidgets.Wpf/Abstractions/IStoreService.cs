using Windows.Services.Store;

namespace BetterWidgets.Abstractions
{
    public interface IStoreService
    {
        Task<(StoreProduct product, Exception ex)> GetProductAsync(string productId);
        Task RateReviewAsync();
    }
}
