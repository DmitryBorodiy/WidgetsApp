using BetterWidgets.Enums;

namespace BetterWidgets.Abstractions
{
    public interface ISearchProvider
    {
        bool CanSearch(string query, SearchType searchType);

        Task<(IEnumerable<ISearchable> results, Exception ex)> SearchAsync(string query);
    }
}
