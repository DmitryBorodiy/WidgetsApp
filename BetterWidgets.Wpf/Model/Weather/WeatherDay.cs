using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetterWidgets.Model.Weather
{
    public class WeatherDay
    {
        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty("dt", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime DateTime { get; set; }

        [JsonProperty("main", NullValueHandling = NullValueHandling.Ignore)]
        public MainUnit Main { get; set; }

        [JsonProperty("weather", NullValueHandling = NullValueHandling.Ignore)]
        public List<WeatherCondition> Condition { get; set; }

        [JsonProperty("clouds", NullValueHandling = NullValueHandling.Ignore)]
        public CloudsUnit Clouds { get; set; }

        [JsonProperty("wind", NullValueHandling = NullValueHandling.Ignore)]
        public WindUnit Wind { get; set; }

        [JsonProperty("visibility", NullValueHandling = NullValueHandling.Ignore)]
        public int Visibility { get; set; }

        [JsonProperty("sys", NullValueHandling = NullValueHandling.Ignore)]
        public PartOfDayUnit PartOfDay { get; set; }
    }
}
