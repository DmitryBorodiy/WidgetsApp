using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using BetterWidgets.Behaviours.Converters;

namespace BetterWidgets.Model.Weather
{
    public class CityUnit
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public int Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }

        [JsonConverter(typeof(UnixTimeZoneConverter))]
        [JsonProperty("timezone", NullValueHandling = NullValueHandling.Ignore)]
        public TimeZoneInfo Timezone { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty("sunrise", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime Sunrise { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty("sunset", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime Sunset { get; set; }
    }
}
