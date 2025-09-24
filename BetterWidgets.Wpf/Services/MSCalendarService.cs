using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Extensions;
using BetterWidgets.Model;
using Microsoft.Extensions.Logging;
using Permission = BetterWidgets.Model.Permission;

namespace BetterWidgets.Services
{
    public class MSCalendarService<TWidget> : ICalendarService<TWidget> where TWidget : IWidget
    {
        #region Fields
        private readonly string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        #endregion

        #region Services
        private readonly ILogger _logger;
        private readonly IMSGraphService _graph;
        private readonly DataService<TWidget> _data;
        private readonly IPermissionManager<TWidget> _permissions;
        #endregion

        public MSCalendarService(ILogger<MSCalendarService<TWidget>> logger, IMSGraphService graphService, IPermissionManager<TWidget> permissions, DataService<TWidget> data)
        {
            _logger = logger;
            _graph = graphService;
            _permissions = permissions;
            _data = data;
        }

        #region Events
        public event EventHandler<CalendarAppointment> EventCreated;
        public event EventHandler<CalendarAppointment> EventUpdated;
        public event EventHandler EventDeleted;
        #endregion

        public async Task<(IEnumerable<CalendarAppointment> appointments, Exception ex)> GetAllAppointmentsAsync()
        {
            try
            {
                if(_graph == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);
                if(await RequestAccessAsync() != PermissionState.Allowed) 
                   throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);

                if(!_graph.IsSignedIn || _graph.Client == null) 
                   return (Enumerable.Empty<CalendarAppointment>(), new UnauthorizedAccessException(Errors.UserIsNotSignedIn));

                var events = await _graph.Client.Me.Calendar.Events.GetAsync();
                var appointments = events.Value.Select(CalendarAppointment.FromMSEvent);

                return (appointments, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (Enumerable.Empty<CalendarAppointment>(), ex);
            }
        }

        public async Task<(IEnumerable<Calendar> calendars, Exception ex)> GetAllCalendarsAsync()
        {
            try
            {
                if(_graph == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);
                if(await RequestAccessAsync() != PermissionState.Allowed)
                   throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);

                if(!_graph.IsSignedIn || _graph.Client == null) 
                   return (Enumerable.Empty<Calendar>(), new UnauthorizedAccessException(Errors.UserIsNotSignedIn));

                var result = await _graph.Client?.Me.Calendars.GetAsync();
                var calendars = result.Value.Select(Calendar.FromMSCalendar);

                return (calendars, null);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (Enumerable.Empty<Calendar>(), ex);
            }
        }

        public async Task<(CalendarAppointment appointment, Exception ex)> GetAppointmentByIdAsync(string id)
        {
            try
            {
                if(_graph == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);
                if(await RequestAccessAsync() != PermissionState.Allowed)
                   throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);

                if(!_graph.IsSignedIn || _graph.Client == null)
                   return (null, new UnauthorizedAccessException(Errors.UserIsNotSignedIn));

                var msEvent = await _graph.Client?.Me.Events[id].GetAsync();

                var appointment = CalendarAppointment.FromMSEvent(msEvent);

                return (appointment, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(IEnumerable<CalendarAppointment> appointments, Exception ex)> GetAppointmentsByCalendarAsync(string calendarId)
        {
            try
            {
                if(_graph == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);
                if(await RequestAccessAsync() != PermissionState.Allowed)
                   throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);

                if(!_graph.IsSignedIn || _graph.Client == null) 
                   return (Enumerable.Empty<CalendarAppointment>(), new UnauthorizedAccessException(Errors.UserIsNotSignedIn));

                var events = await _graph.Client?.Me.Calendars[calendarId].Events.GetAsync();
                var appointments = events.Value.Select(e => CalendarAppointment.FromMSEvent(e));

                return (appointments, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (Enumerable.Empty<CalendarAppointment>(), ex);
            }
        }

        public async Task<(Calendar calendar, Exception ex)> GetCalendarByIdAsync(string id)
        {
            try
            {
                if(_graph == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);
                if(string.IsNullOrEmpty(id)) throw new ArgumentNullException(Errors.IdNullOrEmpty);
                if(await RequestAccessAsync() != PermissionState.Allowed)
                   throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);

                if(!_graph.IsSignedIn || _graph.Client == null)
                   return (null, new UnauthorizedAccessException(Errors.UserIsNotSignedIn));

                var result = await _graph.Client?.Me.Calendars[id].GetAsync();
                var calendar = Calendar.FromMSCalendar(result);

                return (calendar, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(Calendar calendar, Exception ex)> CreateCalendarAsync(ICalendarRequest request)
        {
            try
            {
                if(_graph == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);
                if(await RequestAccessAsync() != PermissionState.Allowed)
                   throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);

                if(!_graph.IsSignedIn || _graph.Client == null)
                   return (null, new UnauthorizedAccessException(Errors.UserIsNotSignedIn));

                var calendarCreation = request.ToMSCalendar();
                var result = await _graph.Client?.Me.Calendars.PostAsync(calendarCreation);

                var calendar = Calendar.FromMSCalendar(result);

                return (calendar, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(CalendarAppointment appointment, Exception ex)> CreateAppointmentAsync(IAppointmentRequest request)
        {
            try
            {
                if(_graph == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);
                if(await RequestAccessAsync() != PermissionState.Allowed)
                   throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);

                if(!_graph.IsSignedIn || _graph.Client == null)
                   return (null, new UnauthorizedAccessException(Errors.UserIsNotSignedIn));

                var msRequest = request.ToMSGraphEvent();

                var msEvent = string.IsNullOrEmpty(request.CalendarId) ?
                              await _graph.Client?.Me.Events.PostAsync(msRequest) :
                              await _graph.Client?.Me.Calendars[request.CalendarId].Events.PostAsync(msRequest);

                var appointment = CalendarAppointment.FromMSEvent(msEvent);

                EventCreated?.Invoke(this, appointment);

                return (appointment, null);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(CalendarAppointment appointment, Exception ex)> UpdateAppointmentAsync(IAppointmentRequest request)
        {
            try
            {
                if(_graph == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);
                if(string.IsNullOrEmpty(request.Id)) throw new ArgumentNullException(Errors.IdNullOrEmpty);
                
                if(await RequestAccessAsync() != PermissionState.Allowed)
                   throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);

                if(!_graph.IsSignedIn || _graph.Client == null)
                   return (null, new UnauthorizedAccessException(Errors.UserIsNotSignedIn));

                var msRequest = request.ToMSGraphEvent();

                var msEvent = string.IsNullOrEmpty(request.CalendarId) ?
                              await _graph.Client?.Me.Events[request.Id].PatchAsync(msRequest) :
                              await _graph.Client?.Me.Calendars[request.CalendarId].Events[request.Id].PatchAsync(msRequest);

                var updatedAppointment = CalendarAppointment.FromMSEvent(msEvent);

                EventUpdated?.Invoke(this, updatedAppointment);

                return (updatedAppointment, null);
            }
            catch(Exception ex)
            {
                return (null, ex);
            }
        }

        public async Task DeleteAppointmentAsync(string id)
        {
            if(_graph == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);
            if(string.IsNullOrEmpty(id)) throw new ArgumentNullException(Errors.IdNullOrEmpty);
            if(await RequestAccessAsync() != PermissionState.Allowed)
               throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);

            if(!_graph.IsSignedIn || _graph.Client == null)
               throw new UnauthorizedAccessException(Errors.UserIsNotSignedIn);

            EventDeleted?.Invoke(this, EventArgs.Empty);

            await _graph.Client?.Me.Events[id].DeleteAsync();
        }

        public async Task DeleteCalendarAsync(string id)
        {
            if(_graph == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);
            if(string.IsNullOrEmpty(id)) throw new ArgumentNullException(Errors.IdNullOrEmpty);
            if(await RequestAccessAsync() != PermissionState.Allowed)
               throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);

            if(!_graph.IsSignedIn || _graph.Client == null)
               throw new UnauthorizedAccessException(Errors.UserIsNotSignedIn);

            await _graph.Client?.Me.Calendars[id].DeleteAsync();
        }

        public async Task<PermissionState> RequestAccessAsync(PermissionLevel level = PermissionLevel.HighLevel)
        {
            var permission = new Permission(Scopes.Appointments, level);
            var state = _permissions.TryCheckPermissionState(permission);

            if(state == PermissionState.Undefined)
               state = await _permissions.RequestAccessAsync(permission);

            return state;
        }

        public async Task<(IEnumerable<CalendarAppointment> appointments, Exception ex)> GetAppointmentsByDateAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if(_graph == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);
                if(await RequestAccessAsync() != PermissionState.Allowed)
                   throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);

                if(!_graph.IsSignedIn || _graph.Client == null)
                   throw new UnauthorizedAccessException(Errors.UserIsNotSignedIn);

                var events = await _graph.Client.Me.CalendarView.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.StartDateTime = startDate.ToString(DateTimeFormat);
                    requestConfiguration.QueryParameters.EndDateTime = endDate.ToString(DateTimeFormat);
                });

                var appointments = events.Value.Select(CalendarAppointment.FromMSEvent);

                return (appointments, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (Enumerable.Empty<CalendarAppointment>(), null);
            }
        }

        public async Task<(IEnumerable<CalendarAppointment> appointments, Exception ex)> GetAppointmentsByQuery(AppointmentsQuery query)
        {
            try
            {
                if(query == null) throw new ArgumentNullException(nameof(query));
                if(_graph == null) throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);

                if(!_graph.IsSignedIn || _graph.Client == null)
                   throw new UnauthorizedAccessException(Errors.UserIsNotSignedIn);

                if(await RequestAccessAsync() != PermissionState.Allowed)
                  throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);

                var events = string.IsNullOrEmpty(query.CalendarId) ? 
                    await _graph.Client.Me.CalendarView.GetAsync(requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.StartDateTime = query.Start.ToString(DateTimeFormat);
                        requestConfiguration.QueryParameters.EndDateTime = query.End.ToString(DateTimeFormat);
                    }) :
                    await _graph.Client.Me.Calendars[query.CalendarId].CalendarView.GetAsync(requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.StartDateTime = query.Start.ToString(DateTimeFormat);
                        requestConfiguration.QueryParameters.EndDateTime = query.End.ToString(DateTimeFormat);
                    });

                var appointments = events.Value.Select(CalendarAppointment.FromMSEvent);

                return (appointments, null);
            }
            catch(Exception ex)
            {
                return (Enumerable.Empty<CalendarAppointment>(), ex);
            }
        }

        public async Task<(T data, Exception ex)> GetCachedAsync<T>(string key, Func<Task<(T data, Exception ex)>> fetchFunc, bool forceRefresh = false, bool fetchData = true)
        {
            try
            {
                if(string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(Errors.FileNameIsNullOrEmpty));

                if(forceRefresh) await ResetCacheAsync(key);

                var cache = await _data.GetFromFileAsync<T>(key);

                if(cache.ex != null) return (default, cache.ex);
                if(cache.data != null && !forceRefresh) return (cache.data, null);
                if(!fetchData) return (default, null);

                var data = await fetchFunc();

                if(data.ex != null) return (default, data.ex);

                await SetCacheAsync(key, data.data);

                return (data.data, null);
            }
            catch(Exception ex) { return (default, ex); }
        }

        public async Task SetCacheAsync<T>(string key, T data)
        {
            if(string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(Errors.FileNameIsNullOrEmpty));
            if(data == null) throw new NullReferenceException(Errors.NullReference);

            await _data.SetToFileAsync(key, data);
        }

        public async Task ResetCacheAsync(string key)
        {
            if(string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(Errors.FileNameIsNullOrEmpty));

            await _data.DeleteFileAsync(key);
        }

        public (string serialized, Exception ex) SerializeIcs(CalendarAppointment appointment)
        {
            try
            {
                if(appointment == null) throw new ArgumentNullException(nameof(appointment));

                var calendar = new Ical.Net.Calendar();
                var calendarEvent = new Ical.Net.CalendarComponents.CalendarEvent
                {
                    Summary = appointment.Subject,
                    Start = new Ical.Net.DataTypes.CalDateTime(appointment.Start?.ToUniversalTime() ?? DateTime.UtcNow),
                    End = new Ical.Net.DataTypes.CalDateTime(appointment.End?.ToUniversalTime() ?? DateTime.UtcNow),
                    Description = appointment.BodyPreview,
                    Location = appointment.Location,
                    Name = appointment.Subject,
                    Url = appointment.WebLink?.ToUri(),
                    Uid = appointment.Id
                };

                calendar.Events.Add(calendarEvent);

                var serializer = new Ical.Net.Serialization.CalendarSerializer();

                return (serializer.SerializeToString(calendar), null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (default, ex);
            }
        }
    }
}
