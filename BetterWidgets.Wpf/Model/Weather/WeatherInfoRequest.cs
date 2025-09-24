using BetterWidgets.Enums;

namespace BetterWidgets.Model.Weather
{
    public class WeatherInfoRequest
    {
        public string Query { get; set; } = string.Empty;
        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;

        public WeatherRequestMode Mode { get; set; }

        public static WeatherInfoRequest FromQuery(string query) => new WeatherInfoRequest 
        { 
            Query = query,
            Mode = WeatherRequestMode.Place
        };

        public static WeatherInfoRequest FromGeocoordinate(double latitude, double longitude) => new WeatherInfoRequest
        {
            Latitude = latitude,
            Longitude = longitude,
            Mode = WeatherRequestMode.Geocoordinate
        };
    }
}
