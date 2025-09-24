using System.Globalization;
using System.Net.Http;
using System.Net.NetworkInformation;
using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Extensions;
using BetterWidgets.Helpers;
using BetterWidgets.Model.Weather;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BetterWidgets.Services
{
    public class WeatherService<TWidget> : IWeatherService<TWidget> where TWidget : IWidget
    {
        #region Services
        private readonly ILogger _logger;
        private readonly WidgetHttpClient<TWidget> _httpClient;
        #endregion        

        public WeatherService(
            ILogger<WeatherService<TWidget>> logger, 
            IOptionsSnapshot<WeatherServiceOptions> options,
            WidgetHttpClient<TWidget> httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;

            Configure(options.Value);
        }

        #region Fields
        private string appId;
        private WeatherServiceOptions _options;
        #endregion

        #region Props

        private WeatherInfo Weather { get; set; }

        public string UnitsMode
        {
            get => _options.UnitsMode;
            set => SetUnitsMode(value);
        }

        #endregion

        #region Utils

        private void Configure(WeatherServiceOptions options)
        {
            if(string.IsNullOrWhiteSpace(options.ApiKey)) throw new FormatException(Errors.ApiKeyIsNullOrEmpty);

            _options = options;

            appId = options?.ApiKey;
        }

        private void SetUnitsMode(string value)
        {
            if(string.IsNullOrEmpty(value)) throw new ArgumentNullException(Errors.UnitsModeIsNullOrEmpty);

            if(value != WeatherUnitsMode.Imperial &&
               value != WeatherUnitsMode.Metric)
               throw new ArgumentException(Errors.UnitsModeHasInvalidValue);
            
            _options.UnitsMode = value;
        }

        private Uri BuildEndpoint(Dictionary<string, string> queries)
        {
            string query = string.Join("&", queries.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

            return new UriBuilder(Endpoints.WeatherForecast)
            {
                Query = query
            }
            .Uri;
        }

        private Uri GetRequestEndpoint(WeatherInfoRequest request)
        {
            Dictionary<string, string> queries = new Dictionary<string, string>()
            {
                { EndpointParams.WeatherAppId, appId },
                { EndpointParams.WeatherUnits, UnitsMode },
                { EndpointParams.Language, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName }
            };

            if(request.Mode == WeatherRequestMode.Place)
            {
                queries.Add(EndpointParams.Query, request.Query);
            }
            else if(request.Mode == WeatherRequestMode.Geocoordinate)
            {
                queries.Add(EndpointParams.Longitude, request.Longitude.ToString());
                queries.Add(EndpointParams.Latitude, request.Latitude.ToString());
            }

            return BuildEndpoint(queries);
        }

        private async Task<WeatherInfo> ProccessHttpResponceAsync(HttpResponseMessage response)
        {
            if(!response.IsSuccessStatusCode) return null;

            string content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<WeatherInfo>(content);
        }

        #endregion

        #region Methods

        public void Dispose() => _httpClient?.Dispose();

        public async Task<WeatherInfo> GetWeatherForecastAsync(WeatherInfoRequest request, bool updateData = false, CancellationToken token = default)
        {
            try
            {
                if(Weather != null && !updateData) return Weather;
                if(!NetworkInterface.GetIsNetworkAvailable()) return null;

                request.Validate();

                var endpoint = GetRequestEndpoint(request);
                var response = await _httpClient.GetAsync(endpoint, token);

                Weather = await ProccessHttpResponceAsync(response);

                return Weather;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        #endregion
    }
}
