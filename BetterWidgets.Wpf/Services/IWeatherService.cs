using BetterWidgets.Abstractions;
using BetterWidgets.Model.Weather;

namespace BetterWidgets.Services
{
    public interface IWeatherService<TWidget> : IDisposable where TWidget : IWidget
    {
        string UnitsMode { get; set; }

        Task<WeatherInfo> GetWeatherForecastAsync(WeatherInfoRequest request, bool updateData = false, CancellationToken token = default);
    }
}
