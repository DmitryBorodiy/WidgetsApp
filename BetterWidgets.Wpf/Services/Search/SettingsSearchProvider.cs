using BetterWidgets.Enums;
using BetterWidgets.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.Services.Search
{
    public sealed class SettingsSearchProvider : ISearchProvider
    {
        private readonly ISettingsManager _settings;

        public SettingsSearchProvider()
        {
            _settings = App.Services?.GetService<ISettingsManager>();
        }

        private SearchType[] SearchTypes => [SearchType.Everything, SearchType.Settings];

        public bool CanSearch(string query, SearchType searchType)
        {
            if(string.IsNullOrEmpty(query)) return false;

            return SearchTypes.Contains(searchType);
        }

        public Task<(IEnumerable<ISearchable> results, Exception ex)> SearchAsync(string query)
        {
            try
            {
                var settings = _settings.Find(query);

                return Task.FromResult<(IEnumerable<ISearchable> results, Exception ex)>((settings, null));
            }
            catch(Exception ex)
            {
                return Task.FromResult<(IEnumerable<ISearchable> results, Exception ex)>((Enumerable.Empty<ISearchable>(), ex));
            }
        }
    }
}
