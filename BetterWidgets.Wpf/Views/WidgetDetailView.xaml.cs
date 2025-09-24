using System.Windows.Controls;
using BetterWidgets.ViewModel;
using BetterWidgets.Abstractions;
using BetterWidgets.Services;
using ListView = Wpf.Ui.Controls.ListView;

namespace BetterWidgets.Views
{
    public partial class WidgetDetailView : Page
    {
        public WidgetDetailView() => InitializeComponent();

        public WidgetDetailView(IWidget widget, IWidgetManager widgetManager, Properties.Settings settings)
        {
            DataContext = new WidgetDetailViewModel(widget, widgetManager, settings);
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}
