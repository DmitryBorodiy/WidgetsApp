using System.Windows;
using System.Windows.Controls;

namespace BetterWidgets.Views.Dialogs
{
    public partial class InputDialog : Page
    {
        public InputDialog()
        {
            DataContext = this;
            InitializeComponent();
        }

        #region PropsRegistration

        public static readonly DependencyProperty SubtileProperty = DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string), 
            typeof(InputDialog),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(InputDialog),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(InputDialog),
            new PropertyMetadata(string.Empty));

        #endregion

        #region Props

        public string Subtitle
        {
            get => (string)GetValue(SubtileProperty);
            set => SetValue(SubtileProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        #endregion
    }
}
