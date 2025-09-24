using System.Windows.Media;

namespace BetterWidgets.Extensions
{
    public static class ColorExtensions
    {
        public static Color ToFrameworkColor(this Windows.UI.Color? color)
        {
            if(!color.HasValue) return System.Windows.Media.Colors.Transparent;

            var value = color.Value;

            return Color.FromArgb(value.A, value.R, value.G, value.B);
        }

        public static string ToHex(this Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        public static string ToAlphaHex(this Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
