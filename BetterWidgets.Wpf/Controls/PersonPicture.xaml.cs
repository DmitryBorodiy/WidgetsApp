using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BetterWidgets.Controls
{
    public partial class PersonPicture : UserControl
    {
        public PersonPicture() => InitializeComponent();

        #region PropsRegistrations

        public static readonly DependencyProperty PersonImageSourceProperty = DependencyProperty.Register(
            nameof(PersonImageSource),
            typeof(ImageSource),
            typeof(PersonPicture),
            new PropertyMetadata(null));

        #endregion

        #region Props

        public ImageSource PersonImageSource
        {
            get => (ImageSource)GetValue(PersonImageSourceProperty);
            set => SetValue(PersonImageSourceProperty, value);
        }

        #endregion
    }
}
