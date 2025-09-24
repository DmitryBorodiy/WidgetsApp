using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BetterWidgets.Views.Dialogs
{
    public partial class EditAppointmentView : Page
    {
        public EditAppointmentView() => InitializeComponent();

        public EditAppointmentView(ObservableObject viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
