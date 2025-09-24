using BetterWidgets.Enums;
using BetterWidgets.Abstractions;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BetterWidgets.Services
{
    public sealed class SearchService : ISearchService
    {
        #region Services
        private readonly ILogger _logger;
        #endregion

        public SearchService(ILogger<SearchService> logger)
        {
            _logger = logger;
            Providers = GetProviders();
        }

        #region Props

        private IEnumerable<ISearchProvider> Providers { get; set; }

        #endregion

        #region Methods

        public IEnumerable<ISearchProvider> GetProviders()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var types = assembly.GetTypes();
                var providersTypes = types.Where(t => t.IsAssignableTo(typeof(ISearchProvider)) &&
                                                 !t.IsInterface && !t.IsAbstract);

                return providersTypes.Select(p => (ISearchProvider)Activator.CreateInstance(p));
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return Enumerable.Empty<ISearchProvider>();
            }
        }

        public async Task<(IEnumerable<ISearchable> results, Exception ex)> SearchAsync(string query, SearchType type = SearchType.Everything)
        {
            try
            {
                if(string.IsNullOrEmpty(query)) 
                   return (Enumerable.Empty<ISearchable>(), null);

                List<ISearchable> results = new List<ISearchable>();

                foreach(var provider in Providers)
                {
                    if(!provider.CanSearch(query, type)) continue;

                    var result = await provider.SearchAsync(query);

                    if(result.ex != null) 
                       _logger.LogError(result.ex, result.ex.Message, result.ex.StackTrace);

                    results.AddRange(result.results);
                }

                return (results, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (Enumerable.Empty<ISearchable>(), ex);
            }
        }

        #endregion
    }
}
