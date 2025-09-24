using Newtonsoft.Json;

namespace BetterWidgets.Model.Weather
{
    public class CloudsUnit
    {
        [JsonProperty("all", NullValueHandling = NullValueHandling.Ignore)]
        public int Cloudiness { get; set; }
    }
}
