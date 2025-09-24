using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetterWidgets.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CalendarImportance
    {
        Unknown,
        Low,
        Normal,
        High
    }
}
