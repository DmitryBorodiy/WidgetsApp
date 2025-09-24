using System.Windows;
using BetterWidgets.Enums;

namespace BetterWidgets.Consts
{
    public static class WidgetSize
    {
        public readonly static Size Small = new Size(160, 160);
        public readonly static Size Medium = new Size(200, 200);
        public readonly static Size Large = new Size(300, 300);

        public static WidgetSizes GetSize(Size size)
        {
            if(size.Width <= Small.Width || size.Height <= Small.Height)
               return WidgetSizes.Small;

            if(size.Width <= Medium.Width || size.Height <= Medium.Height)
               return WidgetSizes.Medium;

            if(size.Width <= Large.Width || size.Height <= Large.Height)
               return WidgetSizes.Large;

            return WidgetSizes.Default;
        }
    }
}
