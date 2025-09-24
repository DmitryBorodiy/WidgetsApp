using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using BetterWidgets.Abstractions;
using BetterWidgets.Attributes;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Enums;
using BetterWidgets.Model;

namespace BetterWidgets.Extensions
{
    public static class AttributeExtensions
    {
        public static Guid GetId(this IWidget widget)
        {
            var attribute = widget.GetType().GetCustomAttribute<GuidAttribute>();
            
            if(attribute == null) return default;

            return Guid.Parse(attribute.Value);
        }

        public static Guid GetId(this WidgetSettingsView widgetSettings)
        {
            var attribute = widgetSettings.GetType().GetCustomAttribute<GuidAttribute>();
            
            if(attribute == null) return Guid.Empty;

            return Guid.Parse(attribute.Value);
        }

        public static IEnumerable<Permission> GetPermissions(this IWidget widget)
        {
            var permissions = new List<Permission>();
            var attribute = widget.GetType().GetCustomAttribute<WidgetPermissions>();

            if(attribute == null) return permissions;

            foreach(var permission in attribute?.Permissions)
                permissions.Add(new Permission()
                {
                    Scope = permission,
                    Level = PermissionLevel.HighLevel
                });

            return permissions;
        }

        public static bool GetIsRequireNetwork(this IWidget widget)
        {
            var attribute = widget.GetType().GetCustomAttribute<RequireNetwork>();

            return attribute?.IsRequire ?? false;
        }

        public static string GetWidgetTitle(this IWidget widget)
        {
            var attribute = widget.GetType().GetCustomAttribute<WidgetTitle>();

            if(attribute == null) return string.Empty;

            return attribute.UseResources ?
                   Resources.Resources.ResourceManager.GetString(attribute.Title) : attribute.Title;
        }

        public static string GetWidgetSubtitle(this IWidget widget)
        {
            var attribute = widget.GetType().GetCustomAttribute<WidgetSubtitle>();

            if(attribute == null) return null;

            return attribute.UseResources ?
                   Resources.Resources.ResourceManager.GetString(attribute.Subtitle) : attribute.Subtitle;
        }

        public static BitmapImage GetWidgetIcon(this IWidget widget)
        {
            var attribute = widget.GetType().GetCustomAttribute<WidgetIcon>();

            if(attribute == null) return new BitmapImage(new Uri(FileNames.defaultWidgetIc));
            if(!Uri.IsWellFormedUriString(attribute.Source, UriKind.RelativeOrAbsolute)) return new BitmapImage(new Uri(FileNames.defaultWidgetIc));

            return new BitmapImage(new Uri(attribute.Source));
        }

        public static bool GetIsDevMode(this IWidget widget)
        {
            var attribute = widget.GetType().GetCustomAttribute<DevMode>();

            return attribute != null && attribute.Enabled;
        }
    }
}
