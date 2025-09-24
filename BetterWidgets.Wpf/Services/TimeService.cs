using BetterWidgets.Model;
using BetterWidgets.Abstractions;
using Microsoft.Extensions.Logging;

namespace BetterWidgets.Services
{
    public class TimeService<TWidget> : ITimeService<TWidget> where TWidget : IWidget
    {
        #region Consts
        private const string _dataFileName = "timezones.json";
        #endregion

        #region Services
        private readonly ILogger _logger;
        private readonly DataService<TWidget> _data;
        #endregion

        public TimeService(ILogger<TimeService<TWidget>> logger, DataService<TWidget> data)
        {
            _logger = logger;
            _data = data;
        }

        #region Props

        private List<TimeZoneModel> TimeZones { get; set; }

        #endregion

        #region Events
        public event EventHandler<IEnumerable<TimeZoneModel>> TimeZonesChanged;
        #endregion

        public async Task<TimeZoneModel> AddUsingTimezoneAsync(TimeZoneInfo timeZoneInfo)
        {
            try
            {
                if(TimeZones == null) await GetAllUsingTimezonesAsync();

                var timezone = new TimeZoneModel(timeZoneInfo);

                TimeZones.Add(timezone);
                TimeZonesChanged?.Invoke(this, TimeZones);

                await SaveAsync();

                return timezone;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return TimeZoneModel.Default;
            }
        }

        public async Task<IEnumerable<TimeZoneModel>> GetAllUsingTimezonesAsync()
        {
            try
            {
                if(TimeZones != null) return TimeZones;

                var data = await _data.GetFromFileAsync<IEnumerable<TimeZoneModel>>(_dataFileName);
                
                if(data.data != null) TimeZones = new List<TimeZoneModel>(data.data);
                else TimeZones = new List<TimeZoneModel>();

                return TimeZones;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return Enumerable.Empty<TimeZoneModel>();
            }
        }

        public async Task RemoveUsingTimezoneAsync(TimeZoneInfo timeZoneInfo)
        {
            try
            {
                if(TimeZones == null) await GetAllUsingTimezonesAsync();

                TimeZones.Remove
                    (TimeZones.FirstOrDefault(i => i.TimeZone == timeZoneInfo));
                TimeZonesChanged?.Invoke(this, TimeZones);

                await SaveAsync();
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        public TimeZoneInfo GetTimeZoneByName(string name)
        {
            var timezones = TimeZoneInfo.GetSystemTimeZones();
            var timezone = timezones.FirstOrDefault(tz => tz.DisplayName == name);

            return timezone;
        }

        private async Task SaveAsync()
        {
            if(TimeZones == null)
               TimeZones = new List<TimeZoneModel>();

            await _data?.SetToFileAsync<IEnumerable<TimeZoneModel>>(_dataFileName, TimeZones);
        }
    }
}
