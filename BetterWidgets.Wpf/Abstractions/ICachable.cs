namespace BetterWidgets.Abstractions
{
    public interface ICachable
    {
        Task<(T data, Exception ex)> GetCachedAsync<T>(string key, Func<Task<(T data, Exception ex)>> fetchFunc, bool forceRefresh = false, bool fetchData = true);
        Task SetCacheAsync<T>(string key, T data);
        Task ResetCacheAsync(string key);
    }
}
