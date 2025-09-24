using BetterWidgets.Enums;
using BetterWidgets.Abstractions;
using BetterWidgets.Extensions.Appointments;

namespace BetterWidgets.Model
{
    public class CalendarAppointment : ICalendarAppointment
    {
        public string Id { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastUpdated { get; set; }
        public TimeSpan? MinutesBeforeStart { get; set; }

        public bool? IsReminderOn { get; set; }
        public string Subject { get; set; }
        public string BodyPreview { get; set; }
        public string WebLink { get; set; }
        public string Location { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public bool? HasAttachments { get; set; }
        public bool? IsAllDay { get; set; }
        
        public CalendarImportance CalendarImportance { get; set; }
        public AppointmentStatus CalendarStatus { get; set; }

        public static CalendarAppointment FromMSEvent(Microsoft.Graph.Models.Event msEvent) => new CalendarAppointment()
        {
            Id = msEvent.Id,
            Subject = msEvent.Subject,
            BodyPreview = msEvent.BodyPreview,
            WebLink = msEvent.WebLink,
            Location = msEvent.Location.DisplayName,
            Created = msEvent.CreatedDateTime?.DateTime,
            LastUpdated = msEvent.LastModifiedDateTime?.DateTime,
            MinutesBeforeStart = TimeSpan.FromMinutes(msEvent.ReminderMinutesBeforeStart ?? 0),
            IsReminderOn = msEvent.IsReminderOn,
            HasAttachments = msEvent.HasAttachments,
            IsAllDay = msEvent.IsAllDay,
            Start = msEvent.Start.GetMSDateTime(msEvent.IsAllDay),
            End = msEvent.End.GetMSDateTime(msEvent.IsAllDay),

            CalendarImportance = msEvent.GetMSImportance(),
            CalendarStatus = msEvent.GetMSStatus()
        };
    }
}
