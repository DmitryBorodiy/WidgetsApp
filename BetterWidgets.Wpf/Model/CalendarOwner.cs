namespace BetterWidgets.Model
{
    public class CalendarOwner
    {
        public CalendarOwner() { }
        public CalendarOwner(Microsoft.Graph.Models.EmailAddress address)
        {
            Name = address.Name;
            Email = address.Address;
        }

        public string Name { get; set; }
        public string Email { get; set; }
    }
}
