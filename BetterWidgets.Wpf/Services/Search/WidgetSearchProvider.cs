using BetterWidgets.Enums;
using BetterWidgets.Abstractions;

namespace BetterWidgets.Services.Search
{
    public sealed class WidgetSearchProvider : ISearchProvider
    {
        private readonly WidgetManager _widgetManager;

        public WidgetSearchProvider()
        {
            _widgetManager = WidgetManager.Current;
        }

        private SearchType[] SupportedTypes => [SearchType.Everything, SearchType.Widget];

        public bool CanSearch(string query, SearchType searchType)
        {
            if(string.IsNullOrEmpty(query)) return false;

            return SupportedTypes.Contains(searchType);
        }

        public Task<(IEnumerable<ISearchable> results, Exception ex)> SearchAsync(string query)
        {
            try
            {
                if(!_widgetManager.Widgets.Any()) return Task.FromResult<(IEnumerable<ISearchable> results, Exception ex)>((Enumerable.Empty<ISearchable>(), null));

                var widgets = _widgetManager.Widgets.Values.Where
                    (w => w.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                          w.Subtitle.Contains(query, StringComparison.OrdinalIgnoreCase));

                return Task.FromResult<(IEnumerable<ISearchable> results, Exception ex)>((widgets, null));
            }
            catch(Exception ex)
            {
                return Task.FromResult<(IEnumerable<ISearchable> results, Exception ex)>((Enumerable.Empty<ISearchable>(), ex));
            }
        }
    }
}
