using BetterWidgets.Behaviours.Converters;
using BetterWidgets.Extensions;
using BetterWidgets.Properties;
using BetterWidgets.Widgets;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace BetterWidgets.Model.Notes
{
    public class StickyNote
    {
        private Color? _color;

        public string Id { get; set; }
        public string Title { get; set; }
        public Guid ClientId { get; set; }

        public bool IsPinned { get; set; }

        [JsonConverter(typeof(JsonColorConverter))]
        public Color? Color
        {
            get => _color;
            set => SetColor(value);
        }

        public string Content { get; set; }
        public string PreviewContent { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? LastEditedDateTime { get; set; }
        public DateTime? DueDateTime { get; set; }

        private Settings<StickyNotesWidget> GetSettings()
        {
            return App.Services?.GetService<Settings<StickyNotesWidget>>();
        }

        public void ClearColorCache()
        {
            var settings = App.Services?.GetService<Settings<StickyNotesWidget>>();

            if(settings == null) return;
            string key = $"{Id}:color";

            if(settings.ContainsKey(key)) settings.RemoveKey(key);
        }

        private void SetColor(Color? color)
        {
            _color = color;

            if(color.HasValue)
            {
                var settings = GetSettings();
                if(settings == null) return;

                string key = $"{Id}:color";
                settings.SetValue(key, color.Value.ToHex());
            }
        }
    }
}
