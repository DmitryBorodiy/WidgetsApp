using Microsoft.Graph.Models;
using BetterWidgets.Abstractions;

namespace BetterWidgets.Model.DTO
{
    public class MSAppointmentRequest : IAppointmentRequest
    {
        public string CalendarId { get; set; }

        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsAllDay { get; set; }
        public string Body { get; set; }
        public string WebLink { get; set; }
        public string Location { get; set; }
        public Recipient Organizer { get; set; }
        public List<Attendee> Attendees { get; set; } = new List<Attendee>();
    }
}
