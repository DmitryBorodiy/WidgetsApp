using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Model.Weather;

namespace BetterWidgets.Extensions
{
    public static class WeatherRequestExtensions
    {
        public static bool IsValidCoordinates(this WeatherInfoRequest request)
            => request.Latitude >= -90 && request.Latitude <= 90 && 
               request.Longitude >= -180 && request.Longitude <= 180;

        public static void Validate(this WeatherInfoRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.Query) && 
               request.Mode == WeatherRequestMode.Place) 
               throw new ArgumentNullException(Errors.PlaceNameIsNullOrEmpty);
            
            if(!request.IsValidCoordinates() &&
               request.Mode == WeatherRequestMode.Geocoordinate) 
               throw new ArgumentException(Errors.GeocoordinatesIsNotValid);
        }
    }
}
