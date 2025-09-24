using System.Windows;

namespace BetterWidgets.Events
{
    public class SelectedTimeChangedEventArgs : RoutedEventArgs
    {
        public DateTime? OldTime { get; set; }
        public DateTime? NewTime { get; set; }

        public SelectedTimeChangedEventArgs(RoutedEvent routedEvent, object source, DateTime? oldTime, DateTime? newTime) : base(routedEvent, source)
        {
            OldTime = oldTime;
            NewTime = newTime;
        }
    }
}
