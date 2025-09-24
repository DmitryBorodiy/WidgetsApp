using System.Text.Json.Serialization;

namespace BetterWidgets.Model
{
    public class MSGraphConfig
    {
        [JsonPropertyName("Scopes")]
        public string[] Scopes { get; set; }
    }
}
