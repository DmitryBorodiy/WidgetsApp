using BetterWidgets.Enums;
using BetterWidgets.Abstractions;
using Windows.Devices.Geolocation;

namespace BetterWidgets.Services
{
    public interface IGeolocationService<TWidget> : IPermissionable where TWidget : IWidget
    {
        Task<(PermissionState state, Geopoint geoposition)> GetCurrentGeopositionAsync();
        Task<(PermissionState state, Geopoint geoposition)> RequestGeopositionAsync();
    }
}
