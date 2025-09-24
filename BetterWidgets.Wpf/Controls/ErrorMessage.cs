using System.Windows;
using System.ComponentModel;
using Wpf.Ui.Controls;

namespace BetterWidgets.Controls
{
    public class ErrorMessage : InfoBar
    {
        private readonly string UICloseCommand = nameof(UICloseCommand);

        public ErrorMessage()
        {
            DefaultStyleKey = typeof(ErrorMessage);
        }

        #region UI
        private Button CloseCommand;
        #endregion

        #region PropsRegistrations

        public static readonly new DependencyProperty IsOpenProperty = DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(ErrorMessage),
            new PropertyMetadata(true, OnOpenChanged));

        #endregion

        #region Props

        public string Id { get; set; }
        public new bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        #endregion

        #region Handlers

        public override void OnApplyTemplate()
        {
            if(GetTemplateChild(UICloseCommand) is Button closeCommand)
            {
                CloseCommand = closeCommand;
                CloseCommand.Click += OnClose;
            }
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            IsOpen = false;
        }

        private static void OnOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is bool value &&
               d is ErrorMessage errorMessage)
               errorMessage.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion
    }
}
