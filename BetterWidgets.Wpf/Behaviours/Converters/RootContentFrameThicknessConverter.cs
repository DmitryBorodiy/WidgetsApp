using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace BetterWidgets.Behaviours.Converters
{
    public class RootContentFrameThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value.GetType() != typeof(bool)) return new Thickness(0);

            return (bool)value ?
                   new Thickness(0) :
                   new Thickness(22, -18, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
