using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetterWidgets.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AppointmentStatus
    {
        Unknown,
        Free,
        Tentative,
        Busy,
        Oof,
        WorkingElsewhere
    }
}
