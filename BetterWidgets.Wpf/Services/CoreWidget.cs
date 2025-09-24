using BetterWidgets.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using BetterWidgets.Consts;

namespace BetterWidgets.Services
{
    public sealed class CoreWidget
    {
        #region Services
        private readonly ILogger _logger;
        #endregion

        public CoreWidget()
        {
            _logger = App.Services?.GetRequiredService<ILogger<CoreWidget>>();
        }

        private Dictionary<Guid, Widget> _views = new Dictionary<Guid, Widget>();

        #region Methods

        public bool HasViewsInGroupName(string groupName)
        {
            if(string.IsNullOrEmpty(groupName)) throw new ArgumentNullException(nameof(groupName));

            return _views.Any(v => v.Value.WidgetGroupName == groupName);
        }

        public Widget GetViewById(Guid id)
            => _views.FirstOrDefault(v => v.Key == id).Value;

        public IEnumerable<Widget> GetCreatedViews()
        {
            if(_views.Count == 0) return Enumerable.Empty<Widget>();

            return _views.Values;
        }

        public IEnumerable<Widget> GetViewsByGroupName(string groupName)
            => _views.Where(v => v.Value.WidgetGroupName == groupName).Select(v => v.Value);

        public (Widget createdView, Exception ex) CreateNewView(Guid? guid, string groupName = null)
        {
            try
            {
                if(guid.HasValue && _views.ContainsKey(guid.Value)) 
                   return (null, new InvalidOperationException(Errors.ViewWithIdAlreadyExists));

                var titleBar = new WidgetTitleBar();
                var view = new Widget(guid, true)
                {
                    TitleBar = titleBar,
                    WidgetGroupName = groupName
                };

                _views.Add(view.Id, view);

                return (view, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        #endregion
    }
}
