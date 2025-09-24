using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Model.DTO;
using Microsoft.Graph.Models;

namespace BetterWidgets.Extensions
{
    public static class CalendarCreationRequestExtensions
    {
        private static readonly string _utcFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        //private static readonly string _utcFormatMidnight = "yyyy-MM-ddT00:00:00.000Z";

        public static Event ToMSGraphEvent(this IAppointmentRequest request)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));
            
            if(request is MSAppointmentRequest msRequest)
            {
                DateTime start = request.IsAllDay ? request.Start.Date : request.Start.ToUniversalTime();
                DateTime end = request.IsAllDay
                    ? request.End.Date.AddDays(1)
                    : request.End.ToUniversalTime();

                return new Event()
                {
                    Subject = request.Title,
                    IsAllDay = request.IsAllDay,
                    Start = new DateTimeTimeZone()
                    {
                        TimeZone = "UTC",
                        DateTime = start.ToString(_utcFormat)
                    },
                    End = new DateTimeTimeZone()
                    {
                        TimeZone = "UTC",
                        DateTime = end.ToString(_utcFormat)
                    },
                    Body = new ItemBody()
                    {
                        ContentType = BodyType.Text,
                        Content = request.Body
                    },
                    Organizer = msRequest.Organizer,
                    Attendees = msRequest.Attendees
                };
            }
            else throw new ArgumentException(Errors.ArgumentHasInvalidType);
        }

        public static Calendar ToMSCalendar(this ICalendarRequest request)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            if(request is MSCalendarCreationRequest msRequest)
            {
                return new Calendar()
                {
                    Name = msRequest.Name
                };
            }
            else throw new ArgumentException(Errors.ArgumentHasInvalidType);
        }
    }
}
