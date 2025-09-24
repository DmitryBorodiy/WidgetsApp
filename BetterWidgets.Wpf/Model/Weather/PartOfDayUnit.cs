using Newtonsoft.Json;

namespace BetterWidgets.Model.Weather
{
    public class PartOfDayUnit
    {
        [JsonProperty("pod", NullValueHandling = NullValueHandling.Ignore)]
        public string Part { get; set; }
    }
}
