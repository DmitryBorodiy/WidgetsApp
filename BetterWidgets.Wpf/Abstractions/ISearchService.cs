using BetterWidgets.Enums;
using BetterWidgets.Abstractions;

namespace BetterWidgets.Abstractions
{
    public interface ISearchService
    {
        IEnumerable<ISearchProvider> GetProviders();
        Task<(IEnumerable<ISearchable> results, Exception ex)> SearchAsync(string query, SearchType type = SearchType.Everything);
    }
}
