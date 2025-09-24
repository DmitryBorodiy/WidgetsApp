using BetterWidgets.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Media;
using Windows.UI.ViewManagement;
using Wpf.Ui;

namespace BetterWidgets.Helpers
{
    public class AccentColorHelper
    {
        public static Color AccentColor => GetAccentColor();

        private static Color GetAccentColor()
            => (Color)Application.Current.Resources["SystemAccentColor"];

        public static void SetSystemAccentColor(bool isDarkTheme)
        {
            var theme = App.Services?.GetService<IThemeService>();
            var uiSettings = App.Services?.GetService<UISettings>();

            var colorType = isDarkTheme ? UIColorType.AccentLight2 : UIColorType.Accent;
            var accent = uiSettings?.GetColorValue(colorType);

            Color frColor = accent.ToFrameworkColor();

            if(accent.HasValue)
            {
                Application.Current.Resources["SystemAccentColor"] = frColor;

                theme?.SetAccent(frColor);
            }
        }

        public static void SetAccentColor(bool isDarkTheme, Color color)
        {
            var theme = App.Services?.GetService<IThemeService>();

            Application.Current.Resources["SystemAccentColor"] = color;
            theme?.SetAccent(color);
        }
    }
}
