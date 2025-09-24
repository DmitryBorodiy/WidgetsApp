using BetterWidgets.Abstractions;
using TimeZoneModel = BetterWidgets.Model.TimeZoneModel;

namespace BetterWidgets.Services
{
    public interface ITimeService<TWidget> where TWidget : IWidget
    {
        event EventHandler<IEnumerable<TimeZoneModel>> TimeZonesChanged;

        Task<IEnumerable<TimeZoneModel>> GetAllUsingTimezonesAsync();
        Task<TimeZoneModel> AddUsingTimezoneAsync(TimeZoneInfo timeZoneInfo);
        Task RemoveUsingTimezoneAsync(TimeZoneInfo timeZoneInfo);

        TimeZoneInfo GetTimeZoneByName(string name);
    }
}
