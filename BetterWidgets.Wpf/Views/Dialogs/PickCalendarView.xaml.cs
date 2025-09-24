using System.Windows.Controls;
using BetterWidgets.ViewModel.Dialogs;

namespace BetterWidgets.Views.Dialogs
{
    public partial class PickCalendarView : Page
    {
        public PickCalendarView()
        {
            InitializeComponent();
            Loaded += (s, e) => { VM?.UpdateCommand.Execute(false); };
        }

        public PickCalendarDialogViewModel VM => DataContext as PickCalendarDialogViewModel;
    }
}
