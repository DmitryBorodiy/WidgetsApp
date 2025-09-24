using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace BetterWidgets.Controls
{
    public partial class SettingCard : UserControl
    {
        public SettingCard()
        {
            InitializeComponent();
        }

        #region PropsRegistration

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(SettingCard),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(SettingCard),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(IconElement),
            typeof(SettingCard),
            new PropertyMetadata(null));

        public static readonly DependencyProperty QueryIconProperty = DependencyProperty.Register(
            nameof(QueryIcon),
            typeof(IconElement),
            typeof(SettingCard),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(SettingCard),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(SettingCard),
            new PropertyMetadata(null));

        #endregion

        #region Event
        public event EventHandler<RoutedEventArgs> Click;
        #endregion

        #region Props

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public IconElement QueryIcon
        {
            get => (IconElement)GetValue(QueryIconProperty);
            set => SetValue(QueryIconProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        #endregion

        private void RootLayout_Click(object sender, RoutedEventArgs e) => Click?.Invoke(this, e);
    }
}
