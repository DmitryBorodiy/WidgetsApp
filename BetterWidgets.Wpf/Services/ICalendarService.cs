using BetterWidgets.Model;
using BetterWidgets.Abstractions;

namespace BetterWidgets.Services
{
    public interface ICalendarService<TWidget> : IPermissionable, ICachable
    {
        event EventHandler<CalendarAppointment> EventCreated;
        event EventHandler<CalendarAppointment> EventUpdated;
        event EventHandler EventDeleted;

        Task<(Calendar calendar, Exception ex)> GetCalendarByIdAsync(string id);
        Task<(IEnumerable<Calendar> calendars, Exception ex)> GetAllCalendarsAsync();
        Task<(IEnumerable<CalendarAppointment> appointments, Exception ex)> GetAllAppointmentsAsync();
        Task<(CalendarAppointment appointment, Exception ex)> GetAppointmentByIdAsync(string id);
        Task<(IEnumerable<CalendarAppointment> appointments, Exception ex)> GetAppointmentsByCalendarAsync(string calendarId);
        Task<(IEnumerable<CalendarAppointment> appointments, Exception ex)> GetAppointmentsByDateAsync(DateTime startDate, DateTime endDate);
        Task<(IEnumerable<CalendarAppointment> appointments, Exception ex)> GetAppointmentsByQuery(AppointmentsQuery query);

        Task<(Calendar calendar, Exception ex)> CreateCalendarAsync(ICalendarRequest request);
        Task<(CalendarAppointment appointment, Exception ex)> CreateAppointmentAsync(IAppointmentRequest appointment);
        Task<(CalendarAppointment appointment, Exception ex)> UpdateAppointmentAsync(IAppointmentRequest appointment);
        
        Task DeleteAppointmentAsync(string id);
        Task DeleteCalendarAsync(string id);

        (string serialized, Exception ex) SerializeIcs(CalendarAppointment appointment);
    }
}
