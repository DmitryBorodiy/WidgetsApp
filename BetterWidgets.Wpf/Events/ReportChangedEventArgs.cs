using BetterWidgets.Model;

namespace BetterWidgets.Events
{
    public class ReportChangedEventArgs : EventArgs
    {
        public ReportChangedEventArgs(UtilizationReport oldValue, UtilizationReport newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public UtilizationReport OldValue { get; set; }
        public UtilizationReport NewValue { get; set; }
    }
}
