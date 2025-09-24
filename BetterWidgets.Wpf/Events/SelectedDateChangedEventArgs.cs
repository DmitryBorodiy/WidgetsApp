using System.Windows;

namespace BetterWidgets.Events
{
    public class SelectedDateChangedEventArgs : RoutedEventArgs
    {
        public SelectedDateChangedEventArgs(RoutedEvent routedEvent, DateTime? newDateTime) 
        : base(routedEvent) 
        {
            NewDateTime = newDateTime;
        }

        public DateTime? NewDateTime { get; set; }
    }
}
