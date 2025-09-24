using Newtonsoft.Json;

namespace BetterWidgets.Model.Weather
{
    public class WeatherInfo
    {
        [JsonProperty("cod", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("cnt", NullValueHandling = NullValueHandling.Ignore)]
        public int Cnt { get; set; }

        [JsonProperty("list", NullValueHandling = NullValueHandling.Ignore)]
        public List<WeatherDay> Days { get; set; }

        [JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
        public CityUnit City { get; set; }
    }
}
