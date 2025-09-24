using BetterWidgets.Enums;

namespace BetterWidgets.Abstractions
{
    public interface ICalendarAppointment
    {
        string Id { get; set; }
        string Subject { get; set; }
        string BodyPreview { get; set; }
        string WebLink { get; set; }
        string Location { get; set; }
        DateTime? Created { get; set; }
        DateTime? LastUpdated { get; set; }
        DateTime? Start { get; set; }
        DateTime? End { get; set; }
        TimeSpan? MinutesBeforeStart { get; set; }
        bool? IsReminderOn { get; set; }
        bool? HasAttachments { get; set; }
        bool? IsAllDay { get; set; }
        CalendarImportance CalendarImportance { get; set; }
        AppointmentStatus CalendarStatus { get; set; }
    }
}
