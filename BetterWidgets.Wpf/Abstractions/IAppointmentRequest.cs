namespace BetterWidgets.Abstractions
{
    public interface IAppointmentRequest
    {
        string CalendarId { get; set; }
        string Id { get; set; }
        string Title { get; set; }
        bool IsAllDay { get; set; }
        string Body { get; set; }
        string WebLink { get; set; }
        string Location { get; set; }
        DateTime Start { get; set; }
        DateTime End { get; set; }
    }
}
