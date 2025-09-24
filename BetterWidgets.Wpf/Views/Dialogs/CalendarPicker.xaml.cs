using System.Windows.Controls;
using System.Windows.Input;
using BetterWidgets.ViewModel.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BetterWidgets.Views.Dialogs
{
    public partial class CalendarPicker : Page
    {
        public CalendarPicker() => InitializeComponent();

        public CalendarPicker(ObservableObject viewModel)
        {
            DataContext = viewModel;

            InitializeComponent();
        }

        private CalendarPickerViewModel VM => DataContext as CalendarPickerViewModel;

        private void OnSelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if(UICalendar.IsMouseOver)
               VM.Widget?.HideContentDialog(true);
        }
    }
}
