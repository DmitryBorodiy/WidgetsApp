using BetterWidgets.Abstractions;
using BetterWidgets.Model.DTO;

namespace BetterWidgets.Tests.Helper
{
    public class TestCalendar
    {
        public static ICalendarRequest CreateMSCalendar() => new MSCalendarCreationRequest()
        {
            Name = Guid.NewGuid().ToString()
        };
    }
}
