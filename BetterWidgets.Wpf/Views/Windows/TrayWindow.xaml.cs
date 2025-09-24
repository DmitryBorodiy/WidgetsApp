using BetterWidgets.ViewModel;
using System.ComponentModel;
using System.Windows;
using Wpf.Ui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.Views.Windows
{

    public partial class TrayWindow : FluentWindow
    {
        private readonly MainWindowViewModel _viewModel;

        public TrayWindow()
        {
            _viewModel = App.Services?.GetRequiredService<MainWindowViewModel>();
            DataContext = _viewModel;

            InitializeComponent();
        }

        private bool _isPositioned = false;

        public bool IsDeactivate { get; set; } = true;

        private void SetTrayPosition()
        {
            if(_isPositioned) return;

            var workingArea = SystemParameters.WorkArea;

            Left = workingArea.Right - ActualWidth - 10;
            Top = workingArea.Bottom - ActualHeight - 10;
        }

        private void OnActivated(object sender, EventArgs e)
        {
            SetTrayPosition();
            Show();
            Activate();
            Focus();
        }

        private void OnDeactivated(object sender, EventArgs e)
        {
            if(IsDeactivate) Hide();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            if(IsDeactivate) Hide();
        }
    }
}
