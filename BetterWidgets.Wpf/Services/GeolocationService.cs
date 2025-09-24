using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Model;
using Microsoft.Extensions.Logging;
using Windows.Devices.Geolocation;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.Services
{
    public class GeolocationService<TWidget> : IGeolocationService<TWidget> where TWidget : IWidget
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Geolocator _geolocator;
        private readonly IPermissionManager<TWidget> _permissionManager;
        #endregion

        public GeolocationService(ILogger<GeolocationService<TWidget>> logger)
        {
            _logger = logger;
            _geolocator = new Geolocator();
            _permissionManager = App.Services?.GetService<IPermissionManager<TWidget>>();

            if(_geolocator != null) _geolocator.PositionChanged += Geolocator_PositionChanged;
        }

        #region Fields
        private BasicGeoposition? geoposition;
        #endregion

        #region Props

        private BasicGeoposition CurrentGeoposition
        {
            get => geoposition ?? Geolocator.DefaultGeoposition.Value;
            set => geoposition = value;
        }

        #endregion

        public async Task<(PermissionState state, Geopoint geoposition)> GetCurrentGeopositionAsync()
        {
            try
            {
                var access = await RequestAccessAsync();

                return (access, access == PermissionState.Allowed ?
                                new Geopoint(CurrentGeoposition) : null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (PermissionState.Undefined, null);
            }
        }

        public async Task<(PermissionState state, Geopoint geoposition)> RequestGeopositionAsync()
        {
            try
            {
                var access = await RequestAccessAsync();

                if(access != PermissionState.Allowed) return (access, null);

                var geoposition = await _geolocator.GetGeopositionAsync();

                return (access, geoposition.Coordinate.Point);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (PermissionState.Undefined, null);
            }
        }

        public async Task<PermissionState> RequestAccessAsync(PermissionLevel level = PermissionLevel.HighLevel)
        {
            PermissionState state = PermissionState.Undefined;
            var systemAccess = await Geolocator.RequestAccessAsync();
            
            if(systemAccess != GeolocationAccessStatus.Allowed) return PermissionState.Denied;

            var permission = new Permission(Scopes.Location, level);
            state = _permissionManager.TryCheckPermissionState(permission);

            if(state == PermissionState.Undefined)
              state = await _permissionManager.RequestAccessAsync(permission);

            return state;
        }

        private void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            if(args.Position != null)
               CurrentGeoposition = args.Position.Coordinate.Point.Position;
        }
    }
}
