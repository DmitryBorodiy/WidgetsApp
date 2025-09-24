using System.Windows;
using Wpf.Ui.Controls;

namespace BetterWidgets.Controls
{
    public sealed class CustomToggleButton : Button
    {
        #region PropsRegistration

        public static readonly DependencyProperty IsToggledProperty = DependencyProperty.Register(
            nameof(IsToggled),
            typeof(bool),
            typeof(CustomToggleButton),
            new PropertyMetadata(false));

        #endregion

        #region Props

        public bool IsToggled
        {
            get => (bool)GetValue(IsToggledProperty);
            set => SetValue(IsToggledProperty, value);
        }

        #endregion
    }
}
