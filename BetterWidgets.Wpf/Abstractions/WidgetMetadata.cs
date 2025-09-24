using BetterWidgets.Attributes;
using BetterWidgets.Consts;
using BetterWidgets.Model;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using BetterWidgets.Properties;
using BetterWidgets.Enums;

namespace BetterWidgets.Abstractions
{
    public class WidgetMetadata : ISearchable
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings _settings;
        #endregion

        public WidgetMetadata() {}
        public WidgetMetadata(Type widgetType)
        {
            _logger = App.Services?.GetService<ILogger<WidgetMetadata>>();
            _settings = App.Services?.GetRequiredService<Settings>();

            Type = widgetType;
            Id = GetId(widgetType);
            Permissions = GetPermissions(widgetType);
            IsRequireNetwork = GetIsRequireNetwork(widgetType);
            Icon = GetIconSource(widgetType);
            Title = GetTitleKey(widgetType);
            Subtitle = GetSubtitleKey(widgetType);
            ProductId = GetProductId(widgetType);
#if !DEBUG
            DevMode = GetDevMode(widgetType);
#endif
        }

        #region Props

        public Guid Id { get; set; }
        public string ProductId { get; set; }
        public IEnumerable<Permission> Permissions { get; set; }
        public Uri Icon { get; set; }
        public string Title { get; set; }
        public string  Subtitle { get; set; }
        public bool IsRequireNetwork { get; set; }
        public Type Type { get; set; }
        public bool IsPinnedDesktop => GetIsPinnedDesktop();
        public bool DevMode { get; set; }

        public SearchType SearchType => SearchType.Widget;

        #endregion

        #region Utils

        private Guid GetId(Type type)
        {
            var attribute = type.GetCustomAttribute<GuidAttribute>();

            if(attribute == null) return Guid.Empty;

            return new Guid(attribute.Value);
        }

        private IEnumerable<Permission> GetPermissions(Type type)
        {
            var permissionsAttribute = type.GetCustomAttribute<WidgetPermissions>();

            if(permissionsAttribute == null) return Enumerable.Empty<Permission>();

            return permissionsAttribute.Permissions.Select(p => new Permission(p));
        }

        private bool GetIsRequireNetwork(Type type)
        {
            var a = type.GetCustomAttribute<RequireNetwork>();

            if(a == null) return false;

            return a.IsRequire;
        }

        private Uri GetIconSource(Type type)
        {
            try
            {
                var a = type.GetCustomAttribute<WidgetIcon>();

                if(a == null) return new Uri(FileNames.defaultWidgetIc);
                if(string.IsNullOrEmpty(a.Source)) return new Uri(FileNames.defaultWidgetIc);
                if(!Uri.IsWellFormedUriString(a.Source, UriKind.RelativeOrAbsolute)) throw new FormatException(Errors.WidgetIconUriIsNotWellFormed);

                return new Uri(a.Source);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return new Uri(FileNames.defaultWidgetIc);
            }
        }

        private string GetTitleKey(Type type)
        {
            try
            {
                var a = type.GetCustomAttribute<WidgetTitle>();

                if(a == null) return null;
                if(string.IsNullOrEmpty(a.Title)) return null;

                return a.UseResources ?
                       Resources.Resources.ResourceManager.GetString(a.Title) : a.Title;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        private string GetSubtitleKey(Type type)
        {
            try
            {
                var a = type.GetCustomAttribute<WidgetSubtitle>();

                if(a == null) return null;
                if(string.IsNullOrEmpty(a.Subtitle)) return null;

                return a.UseResources ?
                       Resources.Resources.ResourceManager.GetString(a.Subtitle) : a.Subtitle;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        private bool GetIsPinnedDesktop()
            => _settings?.GetWidgetValue(Id, nameof(IsPinnedDesktop), false) ?? false;

        private string GetProductId(Type type)
        {
            var attribute = type.GetCustomAttribute<StoreProductId>();

            if(attribute == null) return null;

            return attribute.Id;
        }

        private bool GetDevMode(Type type)
        {
            var attribute = type.GetCustomAttribute<DevMode>();

            return attribute != null && attribute.Enabled;
        }

        #endregion
    }
}
