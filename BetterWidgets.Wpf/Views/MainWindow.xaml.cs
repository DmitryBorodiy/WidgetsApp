using BetterWidgets.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Wpf.Ui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.Views
{
    public partial class MainWindow : FluentWindow
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            _viewModel = App.Services?.GetRequiredService<MainWindowViewModel>();
            DataContext = _viewModel;

            InitializeComponent();
            Loaded += OnLoaded;
        }

        #region Props

        private DispatcherTimer MessageBarTimer;
        public MainWindowViewModel VM => DataContext as MainWindowViewModel;

        #endregion

        #region Utils

        public void Minimize()
        {
            WindowState = WindowState.Minimized;
        }

        public void NotifyUser(
            string message, 
            string title = null, 
            InfoBarSeverity severity = InfoBarSeverity.Warning,
            string actionText = null,
            ICommand actionCommand = null,
            bool enableDuration = true,
            double duration = 7000)
        {
            if(VM == null) return;
            if(enableDuration && MessageBarTimer == null)
            {
                MessageBarTimer = new DispatcherTimer();
                MessageBarTimer.Interval = TimeSpan.FromMilliseconds(duration);

                MessageBarTimer.Tick += OnMessageBarTick;
            }

            VM.MessageBarTitle = title;
            VM.MessageBarSubtitle = message;
            VM.MessageBarSeverity = severity;
            VM.MessageBarActionText = actionText;
            VM.MessageBarCommand = actionCommand;

            VM.IsMessageBarOpen = true;
            MessageBarTimer?.Start();
        }

        #endregion

        #region Handlers

        private void OnMessageBarTick(object sender, EventArgs e)
        {
            MessageBarTimer.Stop();

            VM.IsMessageBarOpen = false;
        }

        private void OnCloseClicked(TitleBar sender, RoutedEventArgs args)
        {
            args.Handled = true;
            Hide();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            VM?.LoadedCommand?.Execute(default);
        }

        #endregion
    }
}
