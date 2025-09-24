using Newtonsoft.Json;
using BetterWidgets.Behaviours.Converters;

namespace BetterWidgets.Model
{
    public class TimeZoneModel
    {
        public TimeZoneModel() { }
        public TimeZoneModel(TimeZoneInfo timeZoneInfo)
        {
            TimeZone = timeZoneInfo;
        }

        [JsonConverter(typeof(StringTimeZoneConverter))]
        public TimeZoneInfo TimeZone { get; set; }

        [JsonIgnore]
        public static TimeZoneModel Default => new TimeZoneModel(TimeZoneInfo.Local);
    }
}
