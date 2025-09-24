using BetterWidgets.ViewModel.Components;
using System.Windows.Controls;
using System.Windows.Input;

namespace BetterWidgets.Views.Dialogs
{
    public partial class NoteView : Page
    {
        public NoteView() => InitializeComponent();
        public NoteView(NoteViewModel dataContext)
        {
            InitializeComponent();

            VM = dataContext;
            DataContext = dataContext;
        }

        private NoteViewModel VM { get; set; }
    }
}
