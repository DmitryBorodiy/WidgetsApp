using BetterWidgets.Consts;

namespace BetterWidgets.Model.Weather
{
    public class WeatherServiceOptions
    {
        public string ApiKey { get; set; }
        public string UnitsMode { get; set; } = WeatherUnitsMode.Metric;
    }
}
