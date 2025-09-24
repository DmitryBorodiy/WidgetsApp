using CommunityToolkit.Mvvm.ComponentModel;

namespace BetterWidgets.ViewModel.Components
{
    public partial class DateViewModel : ObservableObject
    {
        public DateViewModel()
        {
            DateTime = DateTime.Now;
        }

        public DateViewModel(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MonthName))]
        [NotifyPropertyChangedFor(nameof(ShortMonthName))]
        public DateTime dateTime;

        public string MonthName => DateTime.ToString("MMMM");
        public string ShortMonthName => DateTime.ToString("MMM");
    }
}
