using BetterWidgets.Model;

namespace BetterWidgets.Abstractions
{
    public interface ICalendar
    {
		string Id { get; set; }
		string Name { get; set; }
		bool? IsDefault { get; set; }
		CalendarOwner Owner { get; set; }
	}
}
