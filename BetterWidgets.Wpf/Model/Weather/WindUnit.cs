using Newtonsoft.Json;

namespace BetterWidgets.Model.Weather
{
    public class WindUnit
    {
        [JsonProperty("speed", NullValueHandling = NullValueHandling.Ignore)]
        public double Speed { get; set; }

        [JsonProperty("deg", NullValueHandling = NullValueHandling.Ignore)]
        public int DirectionDegrees { get; set; }

        [JsonProperty("gust", NullValueHandling = NullValueHandling.Ignore)]
        public double Gust { get; set; }
    }
}
