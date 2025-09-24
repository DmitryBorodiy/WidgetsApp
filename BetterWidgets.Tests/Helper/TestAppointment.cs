using BetterWidgets.Abstractions;
using BetterWidgets.Model.DTO;

namespace BetterWidgets.Tests.Helper
{
    public class TestAppointment
    {
        public static IAppointmentRequest CreateMSTestAppointment() => new MSAppointmentRequest()
        {
            Title = Guid.NewGuid().ToString(),
            Start = DateTime.Now,
            End = DateTime.Now.AddDays(1)
        };

        public static IAppointmentRequest CreateMSTestAppointment(DateTime start = default, DateTime end = default, string calendarId = null) => new MSAppointmentRequest()
        {
            Title = Guid.NewGuid().ToString(),
            Start = start,
            End = end,
            CalendarId = calendarId
        };
    }
}
