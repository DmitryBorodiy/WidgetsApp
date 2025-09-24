using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace BetterWidgets.ViewModel.Dialogs
{
    public partial class DateTimePickerViewModel : ObservableObject
    {
        #region Props

        [ObservableProperty]
        public bool isOpen;

        [ObservableProperty]
        public bool isTimePickerEnabled = true;

        [ObservableProperty]
        public DateTime selectedDate = DateTime.Now.Date;

        [ObservableProperty]
        public DateTime selectedTime = DateTime.Now;

        #endregion

        #region Commands

        public ICommand OnPickCommand { get; set; }

        [RelayCommand]
        private void Pick()
        {
            IsOpen = false;

            OnPickCommand?.Execute(this);
        }

        [RelayCommand]
        private void Close() => IsOpen = false;

        #endregion
    }
}
