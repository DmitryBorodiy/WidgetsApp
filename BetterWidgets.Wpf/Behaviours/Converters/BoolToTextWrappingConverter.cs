using System.Windows;
using System.Windows.Data;
using System.Globalization;
using BetterWidgets.Consts;

namespace BetterWidgets.Behaviours.Converters
{
    public sealed class BoolToTextWrappingConverter : IValueConverter
    {
        public bool IsInvert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is not bool) throw new ArgumentException(string.Format(Errors.UnexpectedValueType, value.GetType().Name));

            return !IsInvert ?
                (bool)value ? TextWrapping.Wrap : TextWrapping.NoWrap :
                (bool)value ? TextWrapping.NoWrap : TextWrapping.Wrap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is not TextWrapping) throw new ArgumentException(string.Format(Errors.UnexpectedValueType, value.GetType().Name));

            return !IsInvert ?
                (TextWrapping)value == TextWrapping.Wrap :
                (TextWrapping)value != TextWrapping.Wrap;
        }
    }
}
