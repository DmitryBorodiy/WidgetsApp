using BetterWidgets.Abstractions;

namespace BetterWidgets.Model
{
    public class Calendar : ICalendar
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool? IsDefault { get; set; }
        public CalendarOwner Owner { get; set; }

        public override string ToString() => Name;

        public static Calendar FromMSCalendar(Microsoft.Graph.Models.Calendar msCalendar) => new()
        {
            Id = msCalendar.Id,
            Name = msCalendar.Name,
            IsDefault = msCalendar.IsDefaultCalendar,
            Owner = new CalendarOwner(msCalendar.Owner)
        };
    }
}
