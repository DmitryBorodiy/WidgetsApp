namespace BetterWidgets.Model
{
    public class AppointmentsQuery
    {
        public AppointmentsQuery()
        {
            Start = DateTime.Now;
            End = DateTime.Now;
        }

        public AppointmentsQuery(DateTime start, DateTime end) : this(start, end, null) { }
        public AppointmentsQuery(DateTime start, DateTime end, string calendarId)
        {
            End = end;
            Start = start;
            CalendarId = calendarId;
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string CalendarId { get; set; }
    }
}
