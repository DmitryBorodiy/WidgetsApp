using System.Globalization;
using System.Windows.Data;

namespace BetterWidgets.Behaviours.Converters
{
    public sealed class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter?.ToString();
        }
    }
}
